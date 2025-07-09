using System;

namespace MSFS2MQTTBridge.Models
{
    public class EnvironmentData
    {
        public required ValueWithUnit environment_altimeter_pressure { get; set; }
        public required ValueWithUnit environment_outside_temperature { get; set; }
    }
}
