using System;
using System.Threading.Tasks;
using FSUIPC;

namespace MSFS2MQTTBridge
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("MSFS to InfluxDB Bridge starting...");

            try
            {
                await using var bridge = new MSFSDataBridge();
                await bridge.Initialize();
                await bridge.Start();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
            }
        }
    }
}
