using System;

namespace MSFS2MQTTBridge.Models
{
    public class AutopilotData
    {
        public required ValueWithUnit autopilot_master { get; set; }
        public required ValueWithUnit autopilot_wing_leveler { get; set; }
        public required ValueWithUnit autopilot_nav1_lock { get; set; }
        public required ValueWithUnit autopilot_heading_lock { get; set; }
        public required ValueWithUnit autopilot_heading_target { get; set; }
        public required ValueWithUnit autopilot_altitude_lock { get; set; }
        public required ValueWithUnit autopilot_altitude_target { get; set; }
        public required ValueWithUnit autopilot_attitude_hold { get; set; }
        public required ValueWithUnit autopilot_airspeed_hold { get; set; }
        public required ValueWithUnit autopilot_airspeed_target { get; set; }
        public required ValueWithUnit autopilot_mach_hold { get; set; }
        public required ValueWithUnit autopilot_mach_target { get; set; }
        public required ValueWithUnit autopilot_vertical_speed_hold { get; set; }
        public required ValueWithUnit autopilot_vertical_speed_target { get; set; }
        public required ValueWithUnit autopilot_yaw_damper { get; set; }
        public required ValueWithUnit autopilot_autothrottle_arm { get; set; }
        public required ValueWithUnit autopilot_autothrottle_toga { get; set; }
    }
}
