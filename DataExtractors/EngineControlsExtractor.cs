using MSFS2MQTTBridge.Models;

namespace MSFS2MQTTBridge.DataExtractors
{
    public static class EngineControlsExtractor
    {
        public static EngineControlsData Extract(MemoryBlock block, int engineNumber)
        {
            // Extract raw values
            // These offsets are relative to the start of the engine block (0x088C for Engine 1)
            double throttle = block.GetInt16(0) / 16384.0 * 100.0; // 0x088C - Throttle lever (-4096 to +16384)
            double propeller = block.GetInt16(2) / 16384.0 * 100.0; // 0x088E - Prop lever (-4096 to +16384)
            double mixture = block.GetInt16(4) / 16384.0 * 100.0; // 0x0890 - Mixture lever (0 - 16384)
            int magnetos = block.GetInt16(6); // 0x0892 - Magnetos/starter position
            bool antiIce = block.GetInt16(0x26) != 0; // 0x08B2 - Anti-ice/carb heat (offset 0x26 from 0x088C)

            // Determine magneto position name
            string magnetoPosition;
            switch (magnetos)
            {
                case 0:
                    magnetoPosition = "Off";
                    break;
                case 1:
                    magnetoPosition = "Right";
                    break;
                case 2:
                    magnetoPosition = "Left";
                    break;
                case 3:
                    magnetoPosition = "Both";
                    break;
                case 4:
                    magnetoPosition = "Start";
                    break;
                default:
                    magnetoPosition = "Unknown";
                    break;
            }

            // Create object with all required properties initialized
            var data = new EngineControlsData
            {
                engine_number = engineNumber,
                engine_throttle = ValueWithUnit.Percent(throttle),
                engine_propeller = ValueWithUnit.Percent(propeller),
                engine_mixture = ValueWithUnit.Percent(mixture),
                engine_magnetos = ValueWithUnit.State(magnetos, magnetoPosition),
                engine_anti_ice = ValueWithUnit.Boolean(antiIce),
            };

            return data;
        }
    }
}
