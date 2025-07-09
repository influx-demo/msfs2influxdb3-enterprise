using MSFS2MQTTBridge.Models;

namespace MSFS2MQTTBridge.DataExtractors
{
    public static class AutopilotDataExtractor
    {
        public static AutopilotData Extract(MemoryBlock block)
        {
            // Extract raw values
            bool master = block.GetInt32(0) != 0; // 0x07BC
            bool wingLeveler = block.GetInt32(4) != 0; // 0x07C0
            bool nav1Lock = block.GetInt32(8) != 0; // 0x07C4
            bool headingLock = block.GetInt32(12) != 0; // 0x07C8
            double headingValue = block.GetInt16(16) * 360.0 / 65536.0; // 0x07CC - degrees
            bool altitudeLock = block.GetInt32(20) != 0; // 0x07D0
            double altitudeValueMeters = block.GetInt32(24) / 65536.0; // 0x07D4 - stored as meters*65536
            double altitudeValueFeet = altitudeValueMeters * 3.28084; // Convert to feet
            bool attitudeHold = block.GetInt32(28) != 0; // 0x07D8
            bool airspeedHold = block.GetInt32(32) != 0; // 0x07DC
            double airspeedValue = block.GetInt16(38); // 0x07E2
            bool machHold = block.GetInt32(40) != 0; // 0x07E4
            double machValue = block.GetInt32(44) / 65536.0; // 0x07E8
            bool verticalSpeedHold = block.GetInt32(48) != 0; // 0x07EC
            double verticalSpeedValue = block.GetInt16(54); // 0x07F2
            bool yawDamper = block.GetInt32(76) != 0; // 0x0808 (offset from 0x07BC = 76)
            bool autothrottleTOGA = block.GetInt32(80) != 0; // 0x080C
            bool autothrottleArm = block.GetInt32(84) != 0; // 0x0810

            // Create object with all required properties initialized
            var data = new AutopilotData
            {
                autopilot_master = ValueWithUnit.Boolean(master),
                autopilot_wing_leveler = ValueWithUnit.Boolean(wingLeveler),
                autopilot_nav1_lock = ValueWithUnit.Boolean(nav1Lock),
                autopilot_heading_lock = ValueWithUnit.Boolean(headingLock),
                autopilot_heading_target = ValueWithUnit.Degrees(headingValue),
                autopilot_altitude_lock = ValueWithUnit.Boolean(altitudeLock),
                autopilot_altitude_target = ValueWithUnit.Feet(altitudeValueFeet),
                autopilot_attitude_hold = ValueWithUnit.Boolean(attitudeHold),
                autopilot_airspeed_hold = ValueWithUnit.Boolean(airspeedHold),
                autopilot_airspeed_target = ValueWithUnit.Knots(airspeedValue),
                autopilot_mach_hold = ValueWithUnit.Boolean(machHold),
                autopilot_mach_target = ValueWithUnit.State(machValue, "mach"),
                autopilot_vertical_speed_hold = ValueWithUnit.Boolean(verticalSpeedHold),
                autopilot_vertical_speed_target = ValueWithUnit.FeetPerMinute(verticalSpeedValue),
                autopilot_yaw_damper = ValueWithUnit.Boolean(yawDamper),
                autopilot_autothrottle_toga = ValueWithUnit.Boolean(autothrottleTOGA),
                autopilot_autothrottle_arm = ValueWithUnit.Boolean(autothrottleArm),
            };

            return data;
        }
    }
}
