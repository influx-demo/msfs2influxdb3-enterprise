using System;

namespace MSFS2MQTTBridge.Models
{
    public class SystemSwitchesData
    {
        // Known system switches
        public required ValueWithUnit switch_pitot_heat { get; set; } // Pitot Heat switch (0=off, 1=on) - 0x029C

        // Experimental switches (need verification)
        public required ValueWithUnit switch_propeller_deice { get; set; } // Propeller De-Ice - 0x029D (experimental)
        public required ValueWithUnit switch_window_heat { get; set; } // Window Heat - 0x029E (experimental)
        public required ValueWithUnit switch_carb_heat { get; set; } // Carburetor Heat - 0x029F (experimental)
        public required ValueWithUnit switch_wing_deice { get; set; } // Wing De-Ice - 0x02A0 (experimental)
        public required ValueWithUnit switch_alternator { get; set; } // Alternator Switch - 0x02A1 (experimental)
        public required ValueWithUnit switch_master_battery { get; set; } // Battery Master - 0x02A2 (experimental)
        public required ValueWithUnit switch_avionics_master { get; set; } // Avionics Master - 0x02A3 (experimental)

        // Flight instruments
        public required ValueWithUnit whiskey_compass { get; set; } // Whiskey Compass heading in degrees - 0x02CC

        // Note: These properties are based on educated guesses about the memory layout
        // and will need to be verified by testing in the simulator
    }
}
