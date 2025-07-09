using System;

namespace MSFS2MQTTBridge.Models
{
    public class FuelData
    {
        public required ValueWithUnit fuel_total_quantity { get; set; }
        public required ValueWithUnit fuel_total_weight { get; set; }
        public required ValueWithUnit fuel_tank_left_quantity { get; set; }
        public required ValueWithUnit fuel_tank_right_quantity { get; set; }
        public required ValueWithUnit fuel_tank_center_quantity { get; set; }
        public required ValueWithUnit fuel_weight_per_gallon { get; set; }
    }
}
