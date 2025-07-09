using System;

namespace MSFS2MQTTBridge.Models
{
    public class EngineControlsData
    {
        public int engine_number { get; set; }
        public required ValueWithUnit engine_throttle { get; set; } // 0x088C - Engine throttle lever position
        public required ValueWithUnit engine_propeller { get; set; } // 0x088E - Engine propeller lever position
        public required ValueWithUnit engine_mixture { get; set; } // 0x0890 - Engine mixture lever position
        public required ValueWithUnit engine_magnetos { get; set; } // 0x0892 - Engine magnetos/starter position
        public required ValueWithUnit engine_anti_ice { get; set; } // 0x08B2 - Engine anti-ice or carb heat
    }
}
