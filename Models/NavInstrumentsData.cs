using System;

namespace MSFS2MQTTBridge.Models
{
    public class NavInstrumentsData
    {
        public required ValueWithUnit nav_stall_warning { get; set; }
        public required ValueWithUnit nav_overspeed_warning { get; set; }
        public required ValueWithUnit nav_turn_coordinator_ball { get; set; } // -128 is extreme left, +127 is extreme right, 0 is balanced
        public required ValueWithUnit nav_selector { get; set; } // 256=NAV1, 512=NAV2
        public required ValueWithUnit nav_dme_selector { get; set; } // 1=DME1, 2=DME2
        public required ValueWithUnit nav_turn_rate { get; set; } // In standard rate turns (0=level, -1=Left standard rate, +1=Right standard rate)
    }
}
