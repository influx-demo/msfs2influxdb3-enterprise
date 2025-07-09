using MSFS2MQTTBridge.Models;

namespace MSFS2MQTTBridge.Models
{
    public class AircraftData
    {
        // Initialize with empty values to satisfy non-null requirements
        public ValueWithUnit aircraft_callsign { get; set; } =
            ValueWithUnit.State(string.Empty, "callsign");
        public ValueWithUnit aircraft_tailnumber { get; set; } =
            ValueWithUnit.State(string.Empty, "tailnumber");
        public ValueWithUnit aircraft_airline { get; set; } =
            ValueWithUnit.State(string.Empty, "airline");
        public ValueWithUnit aircraft_type { get; set; } =
            ValueWithUnit.State(string.Empty, "type");
    }
}
