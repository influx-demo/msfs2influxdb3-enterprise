using MSFS2MQTTBridge.Models;

namespace MSFS2MQTTBridge.DataExtractors
{
    public static class EngineDataExtractor
    {
        public static EngineData Extract(MemoryBlock block, int engineNumber)
        {
            // Offsets within the engine data block
            int combustionOffset = 0x08; // 0x0894 - 0x088C for Engine 1
            int n2Offset = 0x0A; // 0x0896 - 0x088C for Engine 1
            int n1Offset = 0x0C; // 0x0898 - 0x088C for Engine 1
            int oilTempOffset = 0x2C; // 0x08B8 - 0x088C for Engine 1
            int oilPressOffset = 0x2E; // 0x08BA - 0x088C for Engine 1
            int egtOffset = 0x32; // 0x08BE - 0x088C for Engine 1

            // Extract raw values
            bool running = block.GetInt16(combustionOffset) != 0;
            double n1 = block.GetInt16(n1Offset) / 16384.0 * 100.0;
            double n2 = block.GetInt16(n2Offset) / 16384.0 * 100.0;
            double egt = block.GetInt16(egtOffset) / 16384.0 * 860.0;
            double oilTemp = block.GetInt16(oilTempOffset) / 16384.0 * 140.0;
            double oilPressure = block.GetInt16(oilPressOffset) / 16384.0 * 55.0;
            double fuelFlow = block.GetInt16(0x14) / 16384.0 * 20.0; // Fuel flow at 0x08A0 (offset from 0x088C is 0x14)

            // Create object with all required properties initialized
            var data = new EngineData
            {
                engine_number = engineNumber,
                engine_running = ValueWithUnit.Boolean(running),
                engine_n1 = ValueWithUnit.Percent(n1),
                engine_n2 = ValueWithUnit.Percent(n2),
                engine_egt = ValueWithUnit.Celsius(egt),
                engine_oil_temperature = ValueWithUnit.Celsius(oilTemp),
                engine_oil_pressure = ValueWithUnit.PSI(oilPressure),
                engine_fuel_flow = ValueWithUnit.PoundsPerHour(fuelFlow),
            };

            return data;
        }
    }
}
