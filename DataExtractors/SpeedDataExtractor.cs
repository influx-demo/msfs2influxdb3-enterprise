using MSFS2MQTTBridge.Models;

namespace MSFS2MQTTBridge.DataExtractors
{
    public static class SpeedDataExtractor
    {
        public static SpeedData Extract(MemoryBlock block)
        {
            // Calculate offsets based on the expanded SystemSwitches block
            // Original Speeds block started at 0x02B4, SystemSwitches starts at 0x029C
            // So the offset is 0x02B4 - 0x029C = 0x18 (24 decimal)
            int baseOffset = 24; // 0x18 in hex (difference between 0x029C and 0x02B4)

            // Calculate the speed values with their appropriate units
            // 0x02B4 - GS: Ground Speed, as 65536*metres/sec - convert to knots
            double groundSpeed = block.GetInt32(baseOffset + 0) / 65536.0 * 1.943844492457361;
            // 0x02B8 - TAS: True Air Speed, as knots * 128
            double trueAirSpeed = block.GetInt32(baseOffset + 4) / 128.0;
            // 0x02BC - IAS: Indicated Air Speed, as knots * 128
            double indicatedAirSpeed = block.GetInt32(baseOffset + 8) / 128.0;
            // 0x02C4 - Barber pole airspeed, as knots * 128
            double barberPoleSpeed = block.GetInt32(baseOffset + 16) / 128.0;
            // 0x02C8 - Vertical speed, signed, as 256 * metres/sec
            // For ft/min, apply the conversion *60*3.28084/256 (as per FSUIPC docs)
            double verticalSpeed = block.GetInt32(baseOffset + 20) * 60.0 * 3.28084 / 256.0;

            return new SpeedData
            {
                // Create ValueWithUnit objects with appropriate units
                speed_ground = ValueWithUnit.Knots(groundSpeed),
                speed_true_airspeed = ValueWithUnit.Knots(trueAirSpeed),
                speed_indicated_airspeed = ValueWithUnit.Knots(indicatedAirSpeed),
                speed_barber_pole = ValueWithUnit.Knots(barberPoleSpeed),
                speed_vertical = ValueWithUnit.FeetPerMinute(verticalSpeed),
            };
        }
    }
}
