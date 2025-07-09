using MSFS2MQTTBridge.Models;

namespace MSFS2MQTTBridge.DataExtractors
{
    public static class FlightDataExtractor
    {
        // Private methods for data extraction with proper calculations
        private static double GetLatitude(MemoryBlock block) =>
            block.GetInt64(0) * 90.0 / (10001750.0 * 65536.0 * 65536.0);

        private static double GetLongitude(MemoryBlock block) =>
            block.GetInt64(8) * 360.0 / (65536.0 * 65536.0 * 65536.0 * 65536.0);

        private static double GetAltitude(MemoryBlock block)
        {
            uint altitudeFraction = block.GetUInt32(16);
            int altitudeInteger = block.GetInt32(20);
            return altitudeInteger + (altitudeFraction / 65536.0 / 65536.0);
        }

        private static double GetPitch(MemoryBlock block) =>
            block.GetInt32(24) * 360.0 / (65536.0 * 65536.0);

        private static double GetBank(MemoryBlock block) =>
            block.GetInt32(28) * 360.0 / (65536.0 * 65536.0);

        // Extract TRUE heading (not magnetic) from memory offset 0x0580
        // Note: This will differ from the magnetic heading shown in the simulator's compass
        // by the magnetic variation at the current location
        private static double GetHeading(MemoryBlock block)
        {
            // Get the raw value
            uint rawHeading = block.GetUInt32(32);

            // Convert to degrees (0-360) - this is TRUE heading according to FSUIPC docs
            double heading = rawHeading * 360.0 / (65536.0 * 65536.0);

            // Ensure the heading is within 0-360 range
            heading = heading % 360.0;
            if (heading < 0)
                heading += 360.0;

            return heading;
        }

        // Magnetic variation offset in FSUIPC - this is the difference between true and magnetic north
        // Positive values mean magnetic north is east of true north
        // Negative values mean magnetic north is west of true north
        private static double GetMagneticVariation(MemoryBlock variationBlock)
        {
            // Read magnetic variation from FSUIPC offset 0x2A00
            // The value is stored as degrees * 65536 / 360
            int rawVariation = variationBlock.GetInt32(0);
            double variation = rawVariation * 360.0 / 65536.0;

            // Ensure the variation is within a reasonable range (-180 to +180)
            if (variation > 180.0)
                variation -= 360.0;
            if (variation < -180.0)
                variation += 360.0;

            return variation;
        }

        public static FlightData Extract(MemoryBlock block, MemoryBlock? variationBlock = null)
        {
            // Extract flight data from the memory block with appropriate units
            double latitude = GetLatitude(block);
            double longitude = GetLongitude(block);
            double altitudeMeters = GetAltitude(block);
            double altitudeFeet = altitudeMeters * 3.28084; // Convert meters to feet
            double pitch = GetPitch(block);
            double bank = GetBank(block);
            double trueHeading = GetHeading(block);

            // Get magnetic variation and calculate magnetic heading
            double magneticVariation = 0;
            if (variationBlock != null)
            {
                magneticVariation = GetMagneticVariation(variationBlock);
            }
            else
            {
                // Use a default value if the variation block is not available
                magneticVariation = -15.0; // Typical for central/eastern US
            }

            // Calculate magnetic heading from true heading and variation
            // Magnetic = True + Variation (East is positive, West is negative)
            double magneticHeading = (trueHeading + magneticVariation + 360.0) % 360.0;

            // Create object with all required properties initialized
            var data = new FlightData
            {
                flight_latitude = ValueWithUnit.Degrees(latitude),
                flight_longitude = ValueWithUnit.Degrees(longitude),
                flight_altitude = ValueWithUnit.Feet(altitudeFeet),
                flight_pitch = ValueWithUnit.Degrees(pitch),
                flight_bank = ValueWithUnit.Degrees(bank),
                flight_heading_true = ValueWithUnit.Degrees(trueHeading),
                flight_heading_magnetic = ValueWithUnit.Degrees(magneticHeading),
            };

            return data;
        }
    }
}
