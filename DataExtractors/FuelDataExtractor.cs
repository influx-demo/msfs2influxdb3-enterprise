using MSFS2MQTTBridge.Models;

namespace MSFS2MQTTBridge.DataExtractors
{
    public static class FuelDataExtractor
    {
        public static FuelData Extract(MemoryBlock block)
        {
            // Extract raw values
            double totalFuelQuantityGallons = block.GetInt32(0) / 128.0; // 0x0B74
            double fuelWeightPerGallon = block.GetInt32(4) / 256.0; // 0x0B78
            double totalFuelQuantityLbs = totalFuelQuantityGallons * fuelWeightPerGallon;
            double leftMainTankQuantity = block.GetInt32(8) / 128.0; // 0x0B7C
            double rightMainTankQuantity = block.GetInt32(12) / 128.0; // 0x0B80
            double centerTankQuantity = block.GetInt32(16) / 128.0; // 0x0B84

            // Create object with all required properties initialized
            var data = new FuelData
            {
                fuel_total_quantity = ValueWithUnit.Gallons(totalFuelQuantityGallons),
                fuel_weight_per_gallon = ValueWithUnit.PoundsPerGallon(fuelWeightPerGallon),
                fuel_total_weight = ValueWithUnit.Pounds(totalFuelQuantityLbs),
                fuel_tank_left_quantity = ValueWithUnit.Gallons(leftMainTankQuantity),
                fuel_tank_right_quantity = ValueWithUnit.Gallons(rightMainTankQuantity),
                fuel_tank_center_quantity = ValueWithUnit.Gallons(centerTankQuantity),
            };

            return data;
        }
    }
}
