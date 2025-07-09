using MSFS2MQTTBridge.Models;

namespace MSFS2MQTTBridge.DataExtractors
{
    public static class NavigationDataExtractor
    {
        public static NavigationData Extract(MemoryBlock block)
        {
            // Extract raw values
            double vor1Latitude = block.GetInt32(0) * 90.0 / 10001750.0; // 0x085C
            double vor1Longitude = block.GetInt32(8) * 360.0 / (65536.0 * 65536.0); // 0x0864
            double vor1Elevation = block.GetInt32(16); // 0x086C elevation in meters
            double ilsLocalizerHeading = block.GetInt16(20) * 360.0 / 65536.0; // 0x0870
            double ilsGlideslopeInclination = block.GetInt16(22) * 360.0 / 65536.0; // 0x0872

            // Create object with all required properties initialized
            var data = new NavigationData
            {
                nav_vor1_latitude = ValueWithUnit.Degrees(vor1Latitude),
                nav_vor1_longitude = ValueWithUnit.Degrees(vor1Longitude),
                nav_vor1_elevation = ValueWithUnit.Meters(vor1Elevation),
                nav_ils_localizer_heading = ValueWithUnit.Degrees(ilsLocalizerHeading),
                nav_ils_glideslope_inclination = ValueWithUnit.Degrees(ilsGlideslopeInclination),
            };

            // NOTE: NAVSelect and DMESelect are now handled by the NavInstrumentsExtractor
            // which reads from the correct memory locations (0x0374 and 0x0378)

            return data;
        }
    }
}
