using System;
using System.IO;

namespace MSFS2MQTTBridge.Configuration
{
    public class InfluxDbSettings
    {
        public string Url { get; set; }
        public string Token { get; set; }
        public string Bucket { get; set; }
    }

    public class BatchSettings
    {
        // Define the maximum batch size: this is the number of records that will be sent in a single batch
        public int MaxBatchSize { get; set; }

        // Define the maximum batch age: this is the maximum time that a batch can be held in memory before being sent
        public int MaxBatchAgeMs { get; set; }
    }

    public class ApplicationSettings
    {
        public int DataPublishIntervalMs { get; set; } = 0;
        public BatchSettings BatchSettings { get; set; }
    }

    public class AppSettings
    {
        public InfluxDbSettings InfluxDbSettings { get; set; }
        public ApplicationSettings ApplicationSettings { get; set; }

        public AppSettings()
        {
            // Load environment variables from .env file if it exists
            var envPath = Path.Combine(Directory.GetCurrentDirectory(), ".env");
            if (File.Exists(envPath))
            {
                Console.WriteLine($"Loading environment variables from {envPath}");
                DotNetEnv.Env.Load(envPath);
            }
            else
            {
                Console.WriteLine($"Warning: .env file not found at {envPath}");
            }

            // Load settings from environment variables with defaults
            InfluxDbSettings = new InfluxDbSettings
            {
                Url =
                    Environment.GetEnvironmentVariable("INFLUX_URL")
                    ?? throw new Exception("INFLUX_URL environment variable is not set"),
                Token =
                    Environment.GetEnvironmentVariable("INFLUX_TOKEN")
                    ?? throw new Exception("INFLUX_TOKEN environment variable is not set"),
                Bucket =
                    Environment.GetEnvironmentVariable("INFLUX_BUCKET")
                    ?? throw new Exception("INFLUX_BUCKET environment variable is not set"),
            };

            var batchSettings = new BatchSettings
            {
                MaxBatchSize = int.TryParse(
                    Environment.GetEnvironmentVariable("BATCH_SIZE"),
                    out int batchSize
                )
                    ? batchSize
                    : 100,
                MaxBatchAgeMs = int.TryParse(
                    Environment.GetEnvironmentVariable("BATCH_AGE_MS"),
                    out int batchAgeMs
                )
                    ? batchAgeMs
                    : 100,
            };

            ApplicationSettings = new ApplicationSettings { BatchSettings = batchSettings };

            Console.WriteLine(
                $"Loaded configuration: InfluxDB URL={InfluxDbSettings.Url}, Bucket={InfluxDbSettings.Bucket}"
            );
            Console.WriteLine(
                $"Batch settings: Size={batchSettings.MaxBatchSize}, Age={batchSettings.MaxBatchAgeMs}ms"
            );
        }
    }
}
