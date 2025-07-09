using MSFS2MQTTBridge.Models;

namespace MSFS2MQTTBridge.DataExtractors
{
    public static class SystemSwitchesExtractor
    {
        public static SystemSwitchesData Extract(MemoryBlock block)
        {
            // Extract system switches data from the memory block
            // The block starts at 0x029C and is now 48 bytes long (up to 0x02CC)
            // This allows us to explore more memory locations for potential switches

            // Extract raw boolean values
            bool pitotHeat = block.GetByte(0) != 0; // 0x029C Pitot Heat switch (0=off, 1=on)

            // Explore additional bytes for potential switches
            // Note: These are experimental and need to be verified in the simulator
            bool propellerDeIce = block.GetByte(1) != 0; // 0x029D Propeller De-Ice (educated guess)
            bool windowHeat = block.GetByte(2) != 0; // 0x029E Window Heat (educated guess)
            bool carbHeat = block.GetByte(3) != 0; // 0x029F Carburetor Heat (educated guess)
            bool wingDeIce = block.GetByte(4) != 0; // 0x02A0 Wing De-Ice (educated guess)
            bool alternatorSwitch = block.GetByte(5) != 0; // 0x02A1 Alternator Switch (educated guess)
            bool batteryMaster = block.GetByte(6) != 0; // 0x02A2 Battery Master (educated guess)
            bool avionicsMaster = block.GetByte(7) != 0; // 0x02A3 Avionics Master (educated guess)

            // Extract whiskey compass data from the end of the block (0x02CC)
            // This is a double-precision floating point value (8 bytes)
            double whiskeyCompass = 0;
            try
            {
                // The offset is 0x02CC - 0x029C = 0x30 (48 decimal)
                whiskeyCompass = block.GetDouble(48); // 0x02CC Whiskey Compass in degrees
            }
            catch
            {
                // If we can't read the whiskey compass (perhaps the block isn't large enough),
                // just use a default value
                whiskeyCompass = 0;
            }

            // Create object with all required properties initialized
            var data = new SystemSwitchesData
            {
                switch_pitot_heat = ValueWithUnit.Boolean(pitotHeat),
                switch_propeller_deice = ValueWithUnit.Boolean(propellerDeIce),
                switch_window_heat = ValueWithUnit.Boolean(windowHeat),
                switch_carb_heat = ValueWithUnit.Boolean(carbHeat),
                switch_wing_deice = ValueWithUnit.Boolean(wingDeIce),
                switch_alternator = ValueWithUnit.Boolean(alternatorSwitch),
                switch_master_battery = ValueWithUnit.Boolean(batteryMaster),
                switch_avionics_master = ValueWithUnit.Boolean(avionicsMaster),
                whiskey_compass = ValueWithUnit.Degrees(whiskeyCompass),
            };

            // Note: The remaining bytes (8-47) between the system switches and whiskey compass
            // might contain additional switches or other data that can be explored

            return data;
        }
    }
}
