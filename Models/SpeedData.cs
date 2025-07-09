using System;

namespace MSFS2MQTTBridge.Models
{
    public class SpeedData
    {
        public required ValueWithUnit speed_ground { get; set; }
        public required ValueWithUnit speed_true_airspeed { get; set; }
        public required ValueWithUnit speed_indicated_airspeed { get; set; }
        public required ValueWithUnit speed_barber_pole { get; set; }
        public required ValueWithUnit speed_vertical { get; set; }
    }
}
