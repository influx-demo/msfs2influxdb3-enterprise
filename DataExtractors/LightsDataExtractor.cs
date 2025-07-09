using MSFS2MQTTBridge.Models;

namespace MSFS2MQTTBridge.DataExtractors
{
    public static class LightsDataExtractor
    {
        public static LightsData Extract(MemoryBlock block)
        {
            // Extract raw boolean values
            bool navLights = (block.GetInt16(0) & 0x0001) != 0; // 0x0D0C
            bool beaconLights = (block.GetInt16(0) & 0x0002) != 0; // 0x0D0C
            bool landingLights = (block.GetInt16(0) & 0x0004) != 0; // 0x0D0C
            bool taxiLights = (block.GetInt16(0) & 0x0008) != 0; // 0x0D0C
            bool strobeLights = (block.GetInt16(0) & 0x0010) != 0; // 0x0D0C
            bool instrumentLights = (block.GetInt16(0) & 0x0020) != 0; // 0x0D0C
            bool recognitionLights = (block.GetInt16(0) & 0x0040) != 0; // 0x0D0C
            bool wingLights = (block.GetInt16(0) & 0x0080) != 0; // 0x0D0C
            bool logoLights = (block.GetInt16(0) & 0x0100) != 0; // 0x0D0C
            bool cabinLights = (block.GetInt16(0) & 0x0200) != 0; // 0x0D0C

            // Create object with all required properties initialized
            var data = new LightsData
            {
                light_nav = ValueWithUnit.Boolean(navLights),
                light_beacon = ValueWithUnit.Boolean(beaconLights),
                light_landing = ValueWithUnit.Boolean(landingLights),
                light_taxi = ValueWithUnit.Boolean(taxiLights),
                light_strobe = ValueWithUnit.Boolean(strobeLights),
                light_panel = ValueWithUnit.Boolean(instrumentLights),
                light_recognition = ValueWithUnit.Boolean(recognitionLights),
                light_wing = ValueWithUnit.Boolean(wingLights),
                light_logo = ValueWithUnit.Boolean(logoLights),
                light_cabin = ValueWithUnit.Boolean(cabinLights),
            };

            return data;
        }
    }
}
