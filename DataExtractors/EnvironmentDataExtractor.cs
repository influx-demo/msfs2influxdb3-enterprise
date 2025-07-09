using MSFS2MQTTBridge.Models;

namespace MSFS2MQTTBridge.DataExtractors
{
    public static class EnvironmentDataExtractor
    {
        // NOTE: This extractor now only handles the AltimeterPressureMB value.
        // The other values that were previously read from incorrect offsets have been moved
        // to the NavInstrumentsExtractor, which reads from the correct memory locations.

        public static EnvironmentData Extract(MemoryBlock block)
        {
            // Extract environment data from the memory block
            double altimeterPressure = block.GetInt16(0) / 16.0; // 0x0330 - Correct
            double outsideAirTemp = block.GetInt16(2) / 256.0; // 0x0332 - Outside Air Temperature in Celsius

            // Create object with all required properties initialized
            var data = new EnvironmentData
            {
                environment_altimeter_pressure = ValueWithUnit.Millibars(altimeterPressure),
                environment_outside_temperature = ValueWithUnit.Celsius(outsideAirTemp),
            };

            return data;
        }
    }
}
