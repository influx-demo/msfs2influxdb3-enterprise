using MSFS2MQTTBridge.Models;

namespace MSFS2MQTTBridge.DataExtractors
{
    public static class NavInstrumentsExtractor
    {
        public static NavInstrumentsData Extract(MemoryBlock block)
        {
            // Calculate the offset for navigation instruments data in the combined block
            // Original NavInstruments block started at 0x036C, EnvironmentAndNav starts at 0x0330
            // So the offset is 0x036C - 0x0330 = 0x3C (60 decimal)
            int baseOffset = 60; // 0x3C in hex (difference between 0x0330 and 0x036C)

            // Extract navigation instrument data from the combined block with adjusted offsets
            bool stallWarning = block.GetByte(baseOffset + 0) != 0; // 0x036C Stall warning
            bool overspeedWarning = block.GetByte(baseOffset + 1) != 0; // 0x036D Overspeed warning
            int turnCoordinatorBall = block.GetByte(baseOffset + 2); // 0x036E Turn coordinator ball position
            int navSelect = block.GetInt16(baseOffset + 8); // 0x0374 NAV1 or NAV2 select (256=NAV1, 512=NAV2)
            int dmeSelect = block.GetInt16(baseOffset + 12); // 0x0378 DME1 or DME2 select (1=DME1, 2=DME2)
            double turnRate = block.GetInt16(baseOffset + 16) / 512.0 * 2.0; // 0x037C Turn Rate (0=level, -512=2min Left, +512=2min Right)

            // Get the NAV and DME selection names
            string navName =
                navSelect == 256 ? "NAV1"
                : navSelect == 512 ? "NAV2"
                : "None";
            string dmeName =
                dmeSelect == 1 ? "DME1"
                : dmeSelect == 2 ? "DME2"
                : "None";

            // Create object with all required properties initialized
            var data = new NavInstrumentsData
            {
                nav_stall_warning = ValueWithUnit.Boolean(stallWarning),
                nav_overspeed_warning = ValueWithUnit.Boolean(overspeedWarning),
                nav_turn_coordinator_ball = ValueWithUnit.State(turnCoordinatorBall, "position"),
                nav_selector = ValueWithUnit.State(navSelect, navName),
                nav_dme_selector = ValueWithUnit.State(dmeSelect, dmeName),
                nav_turn_rate = ValueWithUnit.State(turnRate, "standard rate"),
            };

            return data;
        }
    }
}
