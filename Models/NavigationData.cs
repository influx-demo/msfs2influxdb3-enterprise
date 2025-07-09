using System;

namespace MSFS2MQTTBridge.Models
{
    public class NavigationData
    {
        public required ValueWithUnit nav_vor1_latitude { get; set; }
        public required ValueWithUnit nav_vor1_longitude { get; set; }
        public required ValueWithUnit nav_vor1_elevation { get; set; }
        public required ValueWithUnit nav_ils_localizer_heading { get; set; }
        public required ValueWithUnit nav_ils_glideslope_inclination { get; set; }
        // NAVSelect and DMESelect have been moved to NavInstrumentsData
    }
}
