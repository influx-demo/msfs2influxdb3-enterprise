using System;

namespace MSFS2MQTTBridge.Models
{
    public class EngineData
    {
        public int engine_number { get; set; }
        public required ValueWithUnit engine_running { get; set; }
        public required ValueWithUnit engine_n1 { get; set; }
        public required ValueWithUnit engine_n2 { get; set; }
        public required ValueWithUnit engine_fuel_flow { get; set; }
        public required ValueWithUnit engine_oil_temperature { get; set; }
        public required ValueWithUnit engine_oil_pressure { get; set; }
        public required ValueWithUnit engine_egt { get; set; }
    }
}
