using System;

namespace MSFS2MQTTBridge.Models
{
    public class ControlData
    {
        public required ValueWithUnit control_elevator_trim { get; set; }
        public required ValueWithUnit control_aileron_trim { get; set; }
        public required ValueWithUnit control_rudder_trim { get; set; }
        public required ValueWithUnit control_parking_brake { get; set; }
        public required ValueWithUnit control_spoilers { get; set; }
        public required ValueWithUnit control_flaps { get; set; }
        public required ValueWithUnit control_gear_down { get; set; }
    }
}
