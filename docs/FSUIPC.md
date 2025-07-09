# FSUIPC Technical Reference

## Overview

[FSUIPC](https://www.fsuipc.com/) (Flight Simulator Universal Inter-Process Communication) is a utility that provides access to Microsoft Flight Simulator's internal data. It exposes simulator variables through memory offsets that external applications can read and write.

This document provides technical details about how MSFS2MQTT interacts with FSUIPC, the memory blocks it reads, and the offset tables for reference.

## Memory Block Reading

### Efficient Block Reading

Reading individual offsets from FSUIPC is inefficient, as each read operation has overhead. MSFS2MQTT uses memory blocks to read multiple data points in a single operation:

```csharp
_memoryBlocks = new Dictionary<string, MemoryBlock>
{
    { "FlightData", new MemoryBlock(0x0560, 48) },    // Lat, lon, alt, pitch, bank, heading
    { "Speeds", new MemoryBlock(0x02B4, 24) },        // Ground speed, TAS, IAS, vertical speed
    { "Engine1", new MemoryBlock(0x088C, 64) },      // Engine 1 parameters
    { "Engine2", new MemoryBlock(0x0924, 64) },      // Engine 2 parameters
    { "Controls", new MemoryBlock(0x0BC0, 40) },     // Trim, brakes, spoilers, flaps, gear
    { "Autopilot", new MemoryBlock(0x07BC, 96) },    // Autopilot settings
    { "Environment", new MemoryBlock(0x0330, 64) },  // Altimeter, temperatures, etc.
    { "Lights", new MemoryBlock(0x0D0C, 32) },       // Lights status
    { "Navigation", new MemoryBlock(0x085C, 32) },   // VOR and navigation data
    { "Fuel", new MemoryBlock(0x0B74, 24) },         // Fuel data
    { "Simulation", new MemoryBlock(0x0264, 24) }    // Simulation status
};
```

### FSUIPC Memory Management

When working with FSUIPC, it's critical to understand how offset management works:

1. **Persistent Offsets**: FSUIPC offsets should be declared only once during the lifetime of the program and reused. Creating new offset objects repeatedly will fill up the FSUIPC memory file and cause `FSUIPC_ERR_SIZE` errors.

2. **Process Calls**: Each memory block is processed with its own `FSUIPCConnection.Process(offset)` call to ensure efficient data retrieval.

3. **Error Handling**: The application includes robust error handling to continue operation even if some data points can't be read.

## Data Extraction

The application uses specialized extractors for each data type. These extractors know how to interpret the raw memory data and convert it to usable values with appropriate units.

Example for flight data extraction:

```csharp
public static FlightData Extract(MemoryBlock block)
{
    var data = new FlightData();
    
    // Extract all flight data from the combined block
    data.Latitude = block.GetLatitude();
    data.Longitude = block.GetLongitude();
    data.AltitudeMeters = block.GetAltitude();
    data.AltitudeFeet = data.AltitudeMeters * 3.28084;
    data.Pitch = block.GetPitch();
    data.Bank = block.GetBank();
    data.Heading = block.GetHeading();
    
    return data;
}
```

## FSUIPC Offset Reference Table

The following table lists the FSUIPC offsets used by MSFS2MQTT. Data is sourced from [Project Magenta](https://www.projectmagenta.com/all-fsuipc-offsets/).

| Offset | Size | Description |
|--------|------|-------------|
| 0274   | 2      | Frame rate is given by 32768/this value |
| 029C   | 1      | Pitot Heat switch (0=off, 1=on) |
| 02B4   | 4      | GS: Ground Speed, as 65536*metres/sec. Not updated in Slew mode! |
| 02B8   | 4      | TAS: True Air Speed, as knots * 128 |
| 02BC   | 4      | IAS: Indicated Air Speed, as knots * 128 |
| 02C4   | 4      | Barber pole airspeed, as knots * 128 |
| 02C8   | 4      | Vertical speed, signed, as 256 * metres/sec. For the more usual ft/min you need to apply the conversion *60*3.28084/256 |
| 02CC   | 8      | Whiskey Compass, degrees in 'double' floating point format (FLOAT64) |
| 0330   | 2      | Altimeter pressure setting ("Kollsman" window). As millibars (hectoPascals) * 16 |
| 036C   | 1      | Stall warning (0=no, 1=stall) |
| 036D   | 1      | Overspeed warning (0=no, 1=overspeed) |
| 036E   | 1      | Turn co-ordinator ball position (slip and skid). –128 is extreme left, +127 is extreme right, 0 is balanced. |
| 0374   | 2      | NAV1 or NAV2 select (256=NAV1, 512=NAV2) |
| 0378   | 2      | DME1 or DME2 select (1=DME1, 2=DME2) |
| 037C   | 2      | Turn Rate (for turn coordinator). 0=level, –512=2min Left, +512=2min Right |
| 0560   | 8      | Latitude of aircraft in FS units.To convert to Degrees:If your compiler supports long long (64-bit) integers then use such a variable to simply copy this 64-bit value into a double floating point variable and multiply by 90.0/(10001750.0 * 65536.0 * 65536.0).<br>Otherwise you will have to handle the high 32-bits and the low 32-bits separately, combining them into one double floating point value (say dHi). To do, copy the high part (the 32-bit int at 0564) to one double and the low part (the 32-bit unsigned int at 0560) to another (say dLo). Remember that the low part is only part of a bigger number, so doesn't have a sign of its own. Divide dLo by (65536.0 * 65536.0) to give it its proper magnitude compared to the high part, then either add it to or subtract it from dHi according to whether dHi is positive or negative. This preserves the integrity of the original positive or negative number. Finally multiply the result by 90.0/10001750.0 to get degrees.<br>Either way, a negative result is South, positive North.<br>[Can be written to move aircraft: in FS2002 only in slew or pause states] |
| 0568   | 8      | Longitude of aircraft in FS format.To convert to Degrees:If your compiler supports long long (64-bit) integers then use such a variable to simply copy this 64-bit value into a double floating point variable and multiply by 360.0/(65536.0 * 65536.0 * 65536.0 * 65536.0).<br>Otherwise you will have to handle the high 32-bits and the low 32-bits separately, combining them into one double floating point value (say dHi). To do, copy the high part (the 32-bit int at 056C) to one double and the low part (the 32-bit unsigned int at 0568) to another (say dLo). Remember that the low part is only part of a bigger number, so doesn't have a sign of its own. Divide dLo by (65536.0 * 65536.0) to give it its proper magnitude compared to the high part, then either add it to or subtract it from dHi according to whether dHi is positive or negative. This preserves the integrity of the original positive or negative number. Finally multiply the result by 360.0/(65536.0 * 65536.0) to get degrees.<br>Either way, a negative result is West, positive East. If you did it all unsigned then values over 180.0 represent West longitudes of (360.0 – the value).<br>[Can be written to move aircraft: in FS2002 only in slew or pause states] |
| 0570   | 8      | Altitude, in metres and fractional metres. The units are in the high 32-bit integer (at 0574) and the fractional part is in the low 32-bit integer (at 0570). [Can be written to move aircraft: in FS2002 only in slew or pause states] |
| 0578   | 4      | Pitch, *360/(65536*65536) for degrees. 0=level, –ve=pitch up, +ve=pitch down[Can be set in slew or pause states] |
| 057C   | 4      | Bank, *360/(65536*65536) for degrees. 0=level, –ve=bank right, +ve=bank left[Can be set in slew or pause states] |
| 0580   | 4      | Heading, *360/(65536*65536) for degrees TRUE.[Can be set in slew or pause states] |
| 07BC   | 4      | Autopilot Master switch |
| 07C0   | 4      | Autopilot wing leveller |
| 07C4   | 4      | Autopilot NAV1 lock |
| 07C8   | 4      | Autopilot heading lock |
| 07CC   | 2      | Autopilot heading value, as degrees*65536/360 |
| 07D0   | 4      | Autopilot altitude lock |
| 07D4   | 4      | Autopilot altitude value, as metres*65536 |
| 07D8   | 4      | Autopilot attitude hold |
| 07DC   | 4      | Autopilot airspeed hold |
| 07E2   | 2      | Autopilot airspeed value, in knots |
| 07E4   | 4      | Autopilot mach hold |
| 07E8   | 4      | Autopilot mach value, as Mach*65536 |
| 07EC   | 4      | Autopilot vertical speed hold [Not connected in FS2002/4] |
| 07F2   | 2      | Autopilot vertical speed value, as ft/min |
| 07F4   | 4      | Autopilot RPM hold |
| 07FA   | 2      | Autopilot RPM value ?? |
| 0808   | 4      | Yaw damper |
| 080C   | 4      | Autothrottle TOGA (take off power) |
| 0810   | 4      | Autothrottle Arm |
| 0842   | 2      | Vertical speed in metres per minute, but with –ve for UP, +ve for DOWN. Multiply by 3.28084 and reverse the sign for the normal fpm measure. This works even in slew mode (except in FS2002). |
| 085C   | 4      | VOR1 Latitude in FS form. Convert to degrees by *90/10001750.If NAV1 is tuned to an ILS this gives the glideslope transmitter Latitude. |
| 0864   | 4      | VOR1 Longitude in FS form. Convert to degrees by *360/(65536*65536).If NAV1 is tuned to an ILS this gives the glideslope transmitter Longitude. |
| 086C   | 4      | VOR1 Elevation in metres. If NAV1 is tuned to an ILS this gives the glideslope transmitter Elevation. |
| 0870   | 2      | ILS localiser inverse runway heading if VOR1 is ILS. Convert to degrees by *360/65536. This is 180 degrees different to the direction of flight to follow the localiser. |
| 0872   | 2      | ILS glideslope inclination if VOR1 is ILS. Convert to degrees by *360/65536 |
| 088C   | 2      | Engine 1 Throttle lever, –4096 to +16384 |
| 088E   | 2      | Engine 1 Prop lever, –4096 to +16384 |
| 0890   | 2      | Engine 1 Mixture lever, 0 – 16384 |
| 0892   | 2      | Engine 1 Starter switch position (Magnetos), Jet/turbojet: 0=Off, 1=Start, 2=GenProp: 0=Off, 1=right, 2=Left, 3=Both, 4=Start |
| 0894   | 2      | Engine 1 combustion flag (TRUE if engine firing) |
| 0896   | 2      | Engine 1 Jet N2 as 0 – 16384 (100%). This also appears to be the Turbine RPM % for proper helo models. |
| 0898   | 2      | Engine 1 Jet N1 as 0 – 16384 (100%), or Prop RPM (derive RPM by multiplying this value by the RPM Scaler (see 08C8) and dividing by 65536). Note that Prop RPM is signed and negative for counter-rotating propellers. |
| 08A0   | 2      | Engine 1 Fuel Flow PPH SSL (pounds per hour, standardised to sea level). Don't know units, but it seems to match some gauges if divided by 128. Not maintained in all cases. |
| 08B2   | 2      | Engine 1 Anti-Ice or Carb Heat switch (1=On) |
| 08B8   | 2      | Engine 1 Oil temperature, 16384 = 140 C. |
| 08BA   | 2      | Engine 1 Oil pressure, 16384 = 55 psi. Not that in some FS2000 aircraft (the B777) this can exceed the 16-bit capacity of this location. FSUIPC limits it to fit, i.e.65535 = 220 psi |
| 08BE   | 2      | Engine 1 EGT, 16384 = 860 C. |
| 0BC0   | 2      | Elevator Trim, –16383 to +16383 |
| 0BC8   | 2      | Parking brake, 0=off, 32767=on |
| 0BD0   | 4      | Spoilers armed (0=no, 1=yes) |
| 0BD4   | 4      | Spoilers position (0=retracted, 16383=fully extended) |
| 0BD8   | 4      | Flaps position indicator (left). Note that in FS2002 and FS2004 this gives the correct proportional amount, with 16384=full deflection. |
| 0BDC   | 4      | Flaps handle position (0=retracted, 16383=fully extended) |
| 0BE8   | 4      | Gear control: 0=Up, 16383=Down |
| 3130   | 12     | ATC flight number string for currently loaded user aircraft, as declared in the AIRCRAFT.CFG file. This is limited to a maximum of 12 characters, including a zero terminator. [FS2002+ only]
| 313C   | 12     | ATC identifier (tail number) string for currently loaded user aircraft, as declared in the AIRCRAFT.CFG file. This is limited to a maximum of 12 characters, including a zero terminator. [FS2002+ only]
| 3148   | 24     | ATC airline name string for currently loaded user aircraft, as declared in the AIRCRAFT.CFG file. This is limited to a maximum of 24 characters, including a zero terminator. [FS2002+ only]
| 3160   | 24     | ATC aircraft type string for currently loaded user aircraft, as declared in the AIRCRAFT.CFG file. This is limited to a maximum of 24 characters, including a zero terminator. [FS2002+ only]


## Data Conversion

Many FSUIPC values require conversion to be human-readable. Here are common conversions used in MSFS2MQTT:

- **Latitude/Longitude**: Complex conversions from FS-specific formats to standard degrees
- **Speeds**: Often stored as fractional values (e.g., knots * 128)
- **Percentages**: Many values are stored on a 0-16384 scale (e.g., N1, N2)
- **Angles**: Often stored as fractions of complete rotations (e.g., heading * 65536/360)

Each extractor in MSFS2MQTT handles the appropriate conversions for its data type.
