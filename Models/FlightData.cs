using System;

namespace MSFS2MQTTBridge.Models
{
    public class FlightData
    {
        public required ValueWithUnit flight_latitude { get; set; }
        public required ValueWithUnit flight_longitude { get; set; }
        public required ValueWithUnit flight_altitude { get; set; }
        public required ValueWithUnit flight_pitch { get; set; }
        public required ValueWithUnit flight_bank { get; set; }
        public required ValueWithUnit flight_heading_true { get; set; } // True heading
        public required ValueWithUnit flight_heading_magnetic { get; set; } // Magnetic heading
    }
}
