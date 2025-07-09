using MSFS2MQTTBridge.Models;

namespace MSFS2MQTTBridge.DataExtractors
{
    public static class ControlDataExtractor
    {
        public static ControlData Extract(MemoryBlock block)
        {
            // Extract all control data from the combined block
            double elevatorTrim = block.GetInt16(0) / 16383.0 * 100.0;
            double aileronTrim = block.GetInt16(2) / 16383.0 * 100.0;
            double rudderTrim = block.GetInt16(4) / 16383.0 * 100.0;
            bool parkingBrake = block.GetInt16(8) > 0;
            double spoilers = block.GetInt32(20) / 16383.0 * 100.0; // 0x0BD4 (offset from 0x0BC0 is 20)
            double flaps = block.GetInt32(28) / 16383.0 * 100.0; // 0x0BDC (offset from 0x0BC0 is 28)
            bool gearDown = block.GetInt32(40) > 0; // 0x0BE8 (offset from 0x0BC0 is 40)

            // Create object with all required properties initialized
            var data = new ControlData
            {
                control_elevator_trim = ValueWithUnit.Percent(elevatorTrim),
                control_aileron_trim = ValueWithUnit.Percent(aileronTrim),
                control_rudder_trim = ValueWithUnit.Percent(rudderTrim),
                control_parking_brake = ValueWithUnit.Boolean(parkingBrake),
                control_spoilers = ValueWithUnit.Percent(spoilers),
                control_flaps = ValueWithUnit.Percent(flaps),
                control_gear_down = ValueWithUnit.Boolean(gearDown),
            };

            return data;
        }
    }
}
