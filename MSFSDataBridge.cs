using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FSUIPC;
using InfluxDB3.Client;
using InfluxDB3.Client.Write;
using MSFS2MQTTBridge.Configuration;
using MSFS2MQTTBridge.DataExtractors;
using MSFS2MQTTBridge.Models;
using Newtonsoft.Json;

namespace MSFS2MQTTBridge
{
    public class MSFSDataBridge : IAsyncDisposable
    {
        private readonly Dictionary<string, MemoryBlock> _memoryBlocks;
        private InfluxDBClient? _influxClient;
        private readonly AppSettings _settings;

        // Batch processing fields
        private readonly List<string> _batchBuffer = new List<string>();
        private readonly object _batchLock = new object();
        private DateTime _lastBatchFlush = DateTime.UtcNow;
        private readonly CancellationTokenSource _cancellationTokenSource = new();

        // Persistent offset for engine count to avoid memory file filling up
        private readonly Offset<short> _engineCountOffset = new Offset<short>(0x0AEC);

        public MSFSDataBridge()
        {
            _settings = new AppSettings();
            _memoryBlocks = new Dictionary<string, MemoryBlock>
            {
                { "FlightData", new MemoryBlock(0x0560, 48) }, // Lat, lon, alt, pitch, bank, heading
                { "Engine1", new MemoryBlock(0x088C, 64) }, // Engine 1 parameters
                { "Engine2", new MemoryBlock(0x0924, 64) }, // Engine 2 parameters
                { "Controls", new MemoryBlock(0x0BC0, 44) }, // Trim, brakes, spoilers, flaps, gear
                { "Autopilot", new MemoryBlock(0x07BC, 96) }, // Autopilot settings
                { "EnvironmentAndNav", new MemoryBlock(0x0330, 84) }, // Combined block: Environment (0x0330) and NavInstruments (0x036C)
                { "Lights", new MemoryBlock(0x0D0C, 32) }, // Lights status
                { "Navigation", new MemoryBlock(0x085C, 32) }, // VOR and navigation data
                { "SystemSwitches", new MemoryBlock(0x029C, 56) }, // System switches, speeds, and whiskey compass (0x029C-0x02D4)
                { "Fuel", new MemoryBlock(0x0B74, 24) }, // Fuel data
                { "MagneticVariation", new MemoryBlock(0x2A00, 4) }, // Magnetic variation data
                { "AircraftData", new MemoryBlock(0x3130, 72) }, // Aircraft identification data (callsign, tail number, airline, type)
            };
        }

        public async Task Initialize()
        {
            await InitializeInfluxDb();
            await InitializeFSUIPC();
        }

        private Task InitializeInfluxDb()
        {
            Console.WriteLine("Initializing InfluxDB connection...");
            _influxClient = new InfluxDBClient(
                _settings.InfluxDbSettings.Url,
                token: _settings.InfluxDbSettings.Token,
                database: _settings.InfluxDbSettings.Bucket
            );
            return Task.CompletedTask;
        }

        private async Task InitializeFSUIPC()
        {
            Console.WriteLine("Connecting to FSUIPC...");
            int retryCount = 0;
            const int maxRetries = 5;
            const int retryDelay = 3000; // 3 seconds

            while (retryCount < maxRetries)
            {
                try
                {
                    FSUIPCConnection.Open(FlightSim.MSFS2024);
                    Console.WriteLine("Connected to FSUIPC!");
                    Console.WriteLine($"FSUIPC Version: {FSUIPCConnection.FSUIPCVersion}");
                    Console.WriteLine(
                        $"Flight Simulator Version: {FSUIPCConnection.FlightSimVersionConnected}"
                    );

                    // Test a small read operation to verify connection is working properly
                    try
                    {
                        var testOffset = new Offset<short>(0x0AEC); // Engine count offset
                        FSUIPCConnection.Process();
                        var _ = testOffset.Value; // Just to test reading
                        Console.WriteLine("FSUIPC connection verified with test read.");
                    }
                    catch (Exception testEx)
                    {
                        Console.WriteLine($"Warning: Test read failed: {testEx.Message}");
                        Console.WriteLine(
                            "Connection established but may have limited functionality."
                        );
                    }

                    return;
                }
                catch (Exception ex)
                {
                    retryCount++;
                    Console.WriteLine($"Connection attempt {retryCount} failed: {ex.Message}");

                    if (retryCount >= maxRetries)
                    {
                        Console.WriteLine(
                            "Maximum retry attempts reached. Make sure MSFS is running and FSUIPC is correctly installed."
                        );
                        throw;
                    }

                    Console.WriteLine($"Retrying in {retryDelay / 1000} seconds...");
                    await Task.Delay(retryDelay);
                }
            }
        }

        private async Task StartPeriodicFlushAsync()
        {
            Console.WriteLine(
                $"Starting periodic flush every {_settings.ApplicationSettings.BatchSettings.MaxBatchAgeMs}ms"
            );

            while (!_cancellationTokenSource.Token.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(
                        _settings.ApplicationSettings.BatchSettings.MaxBatchAgeMs,
                        _cancellationTokenSource.Token
                    );
                    await FlushBatchAsync();
                }
                catch (OperationCanceledException)
                {
                    // Expected on shutdown
                    break;
                }
                catch (Exception)
                {
                    // do nothing
                }
            }
        }

        public async Task Start()
        {
            int loopCount = 0;

            // Start the periodic flush task
            _ = StartPeriodicFlushAsync();

            Console.WriteLine("Bridge running. Press Ctrl+C to exit.");

            while (true)
            {
                try
                {
                    // Read all memory blocks individually
                    Dictionary<string, bool> blockReadStatus = new Dictionary<string, bool>();
                    foreach (var block in _memoryBlocks)
                    {
                        // Read each block with its own Process() call (now handled inside the Read method)
                        blockReadStatus[block.Key] = block.Value.Read();
                    }

                    // Create data dictionary for this cycle
                    var flightData = new Dictionary<string, object>();

                    // Only extract data from blocks that were successfully read
                    try
                    {
                        if (blockReadStatus["FlightData"])
                        {
                            // Extract flight data from block
                            // Also pass the magnetic variation block if available
                            MemoryBlock? variationBlock = null;
                            if (
                                blockReadStatus.ContainsKey("MagneticVariation")
                                && blockReadStatus["MagneticVariation"]
                            )
                            {
                                variationBlock = _memoryBlocks["MagneticVariation"];
                            }

                            var flightDataObj = FlightDataExtractor.Extract(
                                _memoryBlocks["FlightData"],
                                variationBlock
                            );
                            MergeData(flightData, ConvertToDict(flightDataObj));
                        }
                        else
                        {
                            Console.WriteLine(
                                "Skipping flight data extraction due to read failures."
                            );
                        }
                    }
                    catch (Exception)
                    {
                        // do nothing
                    }

                    try
                    {
                        if (blockReadStatus["SystemSwitches"])
                        {
                            // Extract speeds data from the expanded SystemSwitches block
                            var speedData = SpeedDataExtractor.Extract(
                                _memoryBlocks["SystemSwitches"]
                            );
                            MergeData(flightData, ConvertToDict(speedData));
                        }
                        else
                        {
                            Console.WriteLine(
                                "Skipping speed data extraction due to read failures."
                            );
                        }
                    }
                    catch (Exception)
                    {
                        // do nothing
                    }

                    try
                    {
                        if (blockReadStatus["Engine1"])
                        {
                            // Extract engine1 data from block
                            var engine1Data = EngineDataExtractor.Extract(
                                _memoryBlocks["Engine1"],
                                1
                            );
                            MergeData(flightData, ConvertToDict(engine1Data));

                            // Extract engine1 controls data from the same block
                            var engine1ControlsData = EngineControlsExtractor.Extract(
                                _memoryBlocks["Engine1"],
                                1
                            );
                            MergeData(flightData, ConvertToDict(engine1ControlsData));
                        }
                        else
                        {
                            Console.WriteLine(
                                "Skipping engine 1 data extraction due to read failures."
                            );
                        }
                    }
                    catch (Exception)
                    {
                        // do nothing
                    }

                    try
                    {
                        if (blockReadStatus["Engine2"])
                        {
                            // Extract engine2 data from block
                            var engine2Data = EngineDataExtractor.Extract(
                                _memoryBlocks["Engine2"],
                                2
                            );
                            MergeData(flightData, ConvertToDict(engine2Data));

                            // Extract engine2 controls data from the same block
                            var engine2ControlsData = EngineControlsExtractor.Extract(
                                _memoryBlocks["Engine2"],
                                2
                            );
                            MergeData(flightData, ConvertToDict(engine2ControlsData));
                        }
                        else
                        {
                            Console.WriteLine(
                                "Skipping engine 2 data extraction due to read failures."
                            );
                        }
                    }
                    catch (Exception)
                    {
                        // do nothing
                    }

                    try
                    {
                        if (blockReadStatus["Controls"])
                        {
                            // Extract controls data from block
                            var controlsData = ControlDataExtractor.Extract(
                                _memoryBlocks["Controls"]
                            );
                            MergeData(flightData, ConvertToDict(controlsData));
                        }
                        else
                        {
                            Console.WriteLine(
                                "Skipping controls data extraction due to read failures."
                            );
                        }
                    }
                    catch (Exception)
                    {
                        // do nothing
                    }

                    // Extract data from new memory blocks
                    try
                    {
                        if (blockReadStatus["Autopilot"])
                        {
                            var autopilotData = AutopilotDataExtractor.Extract(
                                _memoryBlocks["Autopilot"]
                            );
                            MergeData(flightData, ConvertToDict(autopilotData));
                        }
                        else
                        {
                            Console.WriteLine(
                                "Skipping autopilot data extraction due to read failures."
                            );
                        }
                    }
                    catch (Exception)
                    {
                        // do nothing
                    }

                    try
                    {
                        if (blockReadStatus["EnvironmentAndNav"])
                        {
                            var environmentData = EnvironmentDataExtractor.Extract(
                                _memoryBlocks["EnvironmentAndNav"]
                            );
                            MergeData(flightData, ConvertToDict(environmentData));
                        }
                        else
                        {
                            Console.WriteLine(
                                "Skipping environment data extraction due to read failures."
                            );
                        }
                    }
                    catch (Exception)
                    {
                        // do nothing
                    }

                    try
                    {
                        if (blockReadStatus["AircraftData"])
                        {
                            // Extract aircraft data from block
                            var aircraftData = AircraftDataExtractor.Extract(
                                _memoryBlocks["AircraftData"]
                            );

                            MergeData(flightData, ConvertToDict(aircraftData));
                        }
                        else
                        {
                            Console.WriteLine(
                                "Skipping aircraft data extraction due to read failures."
                            );
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error extracting aircraft data: {ex.Message}");
                    }

                    try
                    {
                        if (blockReadStatus["Lights"])
                        {
                            var lightsData = LightsDataExtractor.Extract(_memoryBlocks["Lights"]);
                            MergeData(flightData, ConvertToDict(lightsData));
                        }
                        else
                        {
                            Console.WriteLine(
                                "Skipping lights data extraction due to read failures."
                            );
                        }
                    }
                    catch (Exception)
                    {
                        // do nothing
                    }

                    try
                    {
                        if (blockReadStatus["Navigation"])
                        {
                            var navigationData = NavigationDataExtractor.Extract(
                                _memoryBlocks["Navigation"]
                            );
                            MergeData(flightData, ConvertToDict(navigationData));
                        }
                        else
                        {
                            Console.WriteLine(
                                "Skipping navigation data extraction due to read failures."
                            );
                        }
                    }
                    catch (Exception)
                    {
                        // do nothing
                    }

                    try
                    {
                        if (blockReadStatus["Fuel"])
                        {
                            var fuelData = FuelDataExtractor.Extract(_memoryBlocks["Fuel"]);
                            MergeData(flightData, ConvertToDict(fuelData));
                        }
                        else
                        {
                            Console.WriteLine(
                                "Skipping fuel data extraction due to read failures."
                            );
                        }
                    }
                    catch (Exception)
                    {
                        // do nothing
                    }

                    // Extract data from the combined EnvironmentAndNav block
                    try
                    {
                        if (blockReadStatus["EnvironmentAndNav"])
                        {
                            var navInstrumentsData = NavInstrumentsExtractor.Extract(
                                _memoryBlocks["EnvironmentAndNav"]
                            );
                            MergeData(flightData, ConvertToDict(navInstrumentsData));
                        }
                        else
                        {
                            Console.WriteLine(
                                "Skipping navigation instruments data extraction due to read failures."
                            );
                        }
                    }
                    catch (Exception)
                    {
                        // do nothing
                    }

                    // Extract data from the SystemSwitches block
                    try
                    {
                        if (blockReadStatus["SystemSwitches"])
                        {
                            var systemSwitchesData = SystemSwitchesExtractor.Extract(
                                _memoryBlocks["SystemSwitches"]
                            );
                            MergeData(flightData, ConvertToDict(systemSwitchesData));
                        }
                        else
                        {
                            Console.WriteLine(
                                "Skipping system switches data extraction due to read failures."
                            );
                        }
                    }
                    catch (Exception)
                    {
                        // do nothing
                    }

                    // Only publish if we have at least some data
                    if (flightData.Count > 0)
                    {
                        // Write to InfluxDB
                        await WriteToInfluxDb(flightData);
                    }

                    loopCount++;
                    await Task.Delay(_settings.ApplicationSettings.DataPublishIntervalMs);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error reading FSUIPC data: {ex.Message}");
                    Console.WriteLine("Attempting to reconnect...");

                    try
                    {
                        if (FSUIPCConnection.IsOpen)
                            FSUIPCConnection.Close();

                        await Task.Delay(1000);
                        await InitializeFSUIPC();
                    }
                    catch (Exception reconnectEx)
                    {
                        Console.WriteLine($"Reconnection failed: {reconnectEx.Message}");
                        await Task.Delay(5000);
                    }
                }
            }
        }

        // Helper function to determine number of engines
        private int GetEngineCount()
        {
            try
            {
                // Use the existing offset - Process() without arguments processes all registered offsets
                FSUIPCConnection.Process();
                return _engineCountOffset.Value;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading engine count: {ex.Message}");
                return 1; // Default to 1 if we can't determine
            }
        }

        private async Task FlushBatchAsync()
        {
            if (_influxClient == null)
            {
                Console.Write("[NoInflux]");
                return;
            }

            List<string> batchToSend;
            int batchSize;

            lock (_batchLock)
            {
                if (_batchBuffer.Count == 0)
                {
                    return;
                }

                batchToSend = new List<string>(_batchBuffer);
                batchSize = batchToSend.Count;
                _batchBuffer.Clear();
                _lastBatchFlush = DateTime.UtcNow;
            }

            if (batchToSend.Count > 0)
            {
                try
                {
                    var startTime = DateTime.UtcNow;
                    await _influxClient.WriteRecordsAsync(
                        batchToSend,
                        database: _settings.InfluxDbSettings.Bucket
                    );
                }
                catch (Exception)
                {
                    // do nothing
                }
            }
        }

        public async ValueTask DisposeAsync()
        {
            try
            {
                // Signal cancellation
                _cancellationTokenSource.Cancel();

                // Flush any remaining data
                await FlushBatchAsync();

                // Clean up resources
                _influxClient?.Dispose();
                _cancellationTokenSource.Dispose();

                if (FSUIPCConnection.IsOpen)
                {
                    FSUIPCConnection.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during disposal: {ex.Message}");
            }
        }

        // Helper to convert an object to a dictionary while preserving ValueWithUnit objects
        private Dictionary<string, object> ConvertToDict<T>(T obj)
        {
            var result = new Dictionary<string, object>();

            // Use reflection to get all properties
            foreach (var prop in typeof(T).GetProperties())
            {
                var value = prop.GetValue(obj);
                if (value != null)
                {
                    // Use the property name directly - now using snake_case in the model classes
                    string propName = prop.Name;
                    result[propName] = value;
                }
            }

            return result;
        }

        // Helper to merge dictionaries
        private static void MergeData(
            Dictionary<string, object> target,
            Dictionary<string, object> source
        )
        {
            foreach (var kvp in source)
            {
                target[kvp.Key] = kvp.Value;
            }
        }

        private async Task WriteToInfluxDb(Dictionary<string, object> data)
        {
            try
            {
                if (_influxClient == null)
                {
                    throw new InvalidOperationException("InfluxDB client is not initialized");
                }

                // Create line protocol string
                var lineBuilder = new StringBuilder("flight_data");

                // Add tags if available
                if (
                    data.ContainsKey("aircraft_tailnumber")
                    && data["aircraft_tailnumber"] is ValueWithUnit tailNumberValue
                )
                {
                    // Extract the tail number and add it as a tag
                    string tailNumber = tailNumberValue.Value?.ToString() ?? "";
                    if (!string.IsNullOrWhiteSpace(tailNumber))
                    {
                        lineBuilder.Append($",aircraft_tailnumber=\"{tailNumber}\"");
                    }
                }

                // Add fields
                lineBuilder.Append(" ");
                var firstField = true;
                foreach (var kvp in data)
                {
                    // Skip the tail number since we're using it as a tag
                    if (kvp.Key == "aircraft_tailnumber")
                        continue;

                    if (kvp.Value is ValueWithUnit valueWithUnit)
                    {
                        if (!firstField)
                            lineBuilder.Append(",");

                        // Get value
                        var value = valueWithUnit.Value;

                        // For int values, append 'i' to the value
                        if (valueWithUnit.Value is int)
                        {
                            lineBuilder.Append($"{kvp.Key}={valueWithUnit.Value}i");
                        }
                        else if (valueWithUnit.Value is string)
                        {
                            lineBuilder.Append($"{kvp.Key}=\"{valueWithUnit.Value}\"");
                        }
                        else
                        {
                            lineBuilder.Append($"{kvp.Key}={valueWithUnit.Value}");
                        }

                        firstField = false;
                    }
                }

                // Add timestamp in nanoseconds since Unix epoch
                DateTimeOffset now = DateTimeOffset.UtcNow;
                long epochNanoseconds =
                    now.ToUnixTimeMilliseconds() * 1_000_000
                    + (now.Ticks % TimeSpan.TicksPerMillisecond) * 100;
                lineBuilder.Append($" {epochNanoseconds}");

                // Add to batch buffer
                int currentBatchSize;
                bool shouldFlush = false;

                lock (_batchLock)
                {
                    _batchBuffer.Add(lineBuilder.ToString());
                    currentBatchSize = _batchBuffer.Count;

                    // Check if we should flush based on size
                    shouldFlush =
                        currentBatchSize
                        >= _settings.ApplicationSettings.BatchSettings.MaxBatchSize;

                    // If not flushing for size, check if we should flush based on time
                    if (!shouldFlush)
                    {
                        var timeSinceLastFlush = DateTime.UtcNow - _lastBatchFlush;
                        shouldFlush =
                            timeSinceLastFlush
                            >= TimeSpan.FromMilliseconds(
                                _settings.ApplicationSettings.BatchSettings.MaxBatchAgeMs
                            );
                    }

                    if (shouldFlush)
                    {
                        _ = FlushBatchAsync();
                    }
                }
            }
            catch (Exception)
            {
                Console.Write("X"); // Mark write failure
            }
        }
    }
}
