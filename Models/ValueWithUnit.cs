using System;
using Newtonsoft.Json;

namespace MSFS2MQTTBridge.Models
{
    public class ValueWithUnit
    {
        [JsonProperty("value")]
        public object Value { get; set; }

        [JsonProperty("unit")]
        public string Unit { get; set; }

        public ValueWithUnit(object value, string unit)
        {
            Value = value;
            Unit = unit;
        }

        // Helper method for boolean values
        public static ValueWithUnit Boolean(bool value)
        {
            return new ValueWithUnit(value ? 1 : 0, "switch");
        }

        // Helper method for percentage values
        public static ValueWithUnit Percent(double value)
        {
            return new ValueWithUnit(value, "%");
        }

        // Helper method for speed in knots
        public static ValueWithUnit Knots(double value)
        {
            return new ValueWithUnit(value, "knots");
        }

        // Helper method for altitude in feet
        public static ValueWithUnit Feet(double value)
        {
            return new ValueWithUnit(value, "ft");
        }

        // Helper method for vertical speed in feet per minute
        public static ValueWithUnit FeetPerMinute(double value)
        {
            return new ValueWithUnit(value, "ft/min");
        }

        // Helper method for temperature in degrees Celsius
        public static ValueWithUnit Celsius(double value)
        {
            return new ValueWithUnit(value, "°C");
        }

        // Helper method for pressure in millibars/hectopascals
        public static ValueWithUnit Millibars(double value)
        {
            return new ValueWithUnit(value, "mb");
        }

        // Helper method for pressure in pounds per square inch
        public static ValueWithUnit PSI(double value)
        {
            return new ValueWithUnit(value, "psi");
        }

        // Helper method for angles in degrees
        public static ValueWithUnit Degrees(double value)
        {
            return new ValueWithUnit(value, "deg");
        }

        // Helper method for fuel flow in pounds per hour
        public static ValueWithUnit PoundsPerHour(double value)
        {
            return new ValueWithUnit(value, "lb/h");
        }

        // Helper method for fuel quantity in pounds
        public static ValueWithUnit Pounds(double value)
        {
            return new ValueWithUnit(value, "lb");
        }

        // Helper method for fuel quantity in gallons
        public static ValueWithUnit Gallons(double value)
        {
            return new ValueWithUnit(value, "gal");
        }

        // Helper method for fuel weight per gallon
        public static ValueWithUnit PoundsPerGallon(double value)
        {
            return new ValueWithUnit(value, "lb/gal");
        }

        // Helper method for elevation in meters
        public static ValueWithUnit Meters(double value)
        {
            return new ValueWithUnit(value, "m");
        }

        // Helper method for dimensionless values (like ratios)
        public static ValueWithUnit Dimensionless(double value)
        {
            return new ValueWithUnit(value, "");
        }

        // Helper method for enumerated states (like switch positions)
        public static ValueWithUnit State(object value, string stateName)
        {
            return new ValueWithUnit(value, stateName);
        }
    }
}
