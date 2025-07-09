using System.Text;
using MSFS2MQTTBridge.Models;

namespace MSFS2MQTTBridge.DataExtractors
{
    public static class AircraftDataExtractor
    {
        public static AircraftData Extract(MemoryBlock block)
        {
            // Extract aircraft identification data from the memory block
            // The block contains all aircraft identification data starting at 0x3130

            // Extract callsign (0x3130, 12 bytes)
            string callsign = ExtractString(block, 0, 12);

            // Extract tail number (0x313C, 12 bytes)
            string tailNumber = ExtractString(block, 12, 12);

            // Extract airline (0x3148, 24 bytes)
            string airline = ExtractString(block, 24, 24);

            // Extract aircraft type (0x3160, 24 bytes)
            string aircraftType = ExtractString(block, 48, 24);

            // Create object with all properties initialized
            var data = new AircraftData
            {
                aircraft_callsign = ValueWithUnit.State(callsign, "callsign"),
                aircraft_tailnumber = ValueWithUnit.State(tailNumber, "tailnumber"),
                aircraft_airline = ValueWithUnit.State(airline, "airline"),
                aircraft_type = ValueWithUnit.State(aircraftType, "type"),
            };

            return data;
        }

        private static string ExtractString(MemoryBlock block, int offset, int maxLength)
        {
            // Read the raw bytes from the memory block
            byte[] rawBytes = new byte[maxLength];
            for (int i = 0; i < maxLength; i++)
            {
                rawBytes[i] = block.GetByte(offset + i);
            }

            // Convert the bytes to a string, stopping at the first null terminator
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < rawBytes.Length; i++)
            {
                if (rawBytes[i] == 0)
                    break; // Stop at null terminator
                sb.Append((char)rawBytes[i]);
            }

            return sb.ToString().Trim();
        }
    }
}
