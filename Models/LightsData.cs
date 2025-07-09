using System;

namespace MSFS2MQTTBridge.Models
{
    public class LightsData
    {
        public required ValueWithUnit light_nav { get; set; }
        public required ValueWithUnit light_beacon { get; set; }
        public required ValueWithUnit light_landing { get; set; }
        public required ValueWithUnit light_taxi { get; set; }
        public required ValueWithUnit light_strobe { get; set; }
        public required ValueWithUnit light_panel { get; set; }
        public required ValueWithUnit light_recognition { get; set; }
        public required ValueWithUnit light_wing { get; set; }
        public required ValueWithUnit light_logo { get; set; }
        public required ValueWithUnit light_cabin { get; set; }
    }
}
