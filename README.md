# MSFS to InfluxDB3 Enterprise Bridge

MSFS to InfluxDB Bridge is a CLI application that reads data from Microsoft Flight Simulator 2024 (MSFS) using FSUIPC and writes the data directly to InfluxDB 3 Enterprise. This allows you to monitor and visualize flight data in real-time using InfluxDB and tools like Grafana.

## Features

- Connects to Microsoft Flight Simulator using [FSUIPC](https://www.fsuipc.com/)
- Reads over 70 data points from the simulator including:
  - Flight data (position, altitude, attitude)
  - Speed data (ground, true air, indicated air speeds)
  - Engine parameters (N1, N2, temperatures, pressures)
  - Autopilot settings
  - Aircraft controls and surfaces
  - Environment data
  - Navigation systems
  - Fuel systems
  - Aircraft identification (callsign, tail number, airline, type)
- Writes data directly to InfluxDB for storage and visualization
- Uses efficient memory block reading to optimize performance
- Error recovery and reconnection logic
- Configuration via environment variables (.env file)

## Installation

### Required Software

1. **Microsoft Flight Simulator**: MSFS 2020 or MSFS 2024
2. **FSUIPC7**: Required to access flight simulator data. Check out [Pete & John Dowson's Software page](https://www.fsuipc.com/) for more information.
   - [Purchase from SimMarket](https://secure.simmarket.com/john-dowson-fsuipc7-for-msfs.phtml)
   - A registered version is recommended, but the free version works for many data points
3. **.NET 8.0 SDK**: Required to build and run the application
   - [Download from Microsoft](https://dotnet.microsoft.com/download/dotnet/8.0)
4. **InfluxDB**: Required to store and query flight simulator data
   - [Download InfluxDB](https://docs.influxdata.com/influxdb/v3/install/) (v3 Enterprise self-hosted recommended, v2 should also work)
   - Note: InfluxDB Cloud is not currently supported

## Step 1: Install and Configure FSUIPC

1. [Purchase](https://secure.simmarket.com/john-dowson-fsuipc7-for-msfs.phtml) and [download FSUIPC7](https://www.fsuipc.com/) for MSFS 2020/2024
2. Install FSUIPC following the instructions provided
3. Register FSUIPC if you purchased a license
4. Launch Microsoft Flight Simulator
5. Press **Ctrl+F** to open the FSUIPC configuration window
6. Ensure FSUIPC is working correctly

## Step 2: Set Up InfluxDB

1. Download and install InfluxDB v3 Enterprise (self-hosted) or v2
   - [InfluxDB Installation Guide](https://docs.influxdata.com/influxdb/v3/install/)
2. Start the InfluxDB service
3. Create a new bucket named `flightsim` (or your preferred name)
4. Generate an API token with write access to your bucket
5. Note your InfluxDB URL, token, and bucket name for configuration

## Step 3: Install and Configure MSFS to InfluxDB Bridge

1. Clone the repository:

```
git clone https://github.com/influx-demo/msfs2influxdb3-enterprise.git
cd msfs2influx
```

2. Build the project:

```
dotnet build
```

3. Configure InfluxDB connection:

Create a `.env` file in the root directory based on the provided `.env.example`:

```
INFLUX_URL=http://localhost:8181
INFLUX_TOKEN=your_influxdb_token_here
INFLUX_BUCKET=flightsim
BATCH_SIZE=100
BATCH_AGE_MS=100
```

Update the values to match your InfluxDB setup:
- `INFLUX_URL`: The URL of your InfluxDB instance
- `INFLUX_TOKEN`: Your InfluxDB API token with write access
- `INFLUX_BUCKET`: The name of your InfluxDB bucket
- `BATCH_SIZE`: Maximum number of data points to batch before writing
- `BATCH_AGE_MS`: Maximum age of a batch in milliseconds before writing

4. Run the application:

```
dotnet run
```

## Step 4: Test the Setup

1. Start Microsoft Flight Simulator
2. Start a flight or use an existing save
3. Run MSFS to InfluxDB Bridge
4. Verify you see "Connected to FSUIPC!" and "Loaded configuration" messages
5. Use the InfluxDB UI or API to verify data is being written to your bucket

## Troubleshooting

### FSUIPC Connection Issues

- Ensure Microsoft Flight Simulator is running before starting the bridge
- Check that FSUIPC7 is correctly installed and registered
- Try restarting both MSFS and the bridge application

### InfluxDB Connection Issues

- Verify your InfluxDB instance is running (`netstat -an | findstr 8181` on Windows)
- Check that your `.env` file contains the correct InfluxDB URL, token, and bucket name
- Ensure your token has write access to the specified bucket
- Check firewall settings if connecting to a remote InfluxDB instance

### Application Error Messages

- `Error reading FSUIPC data`: Check FSUIPC installation and ensure MSFS is running
- `InfluxDB client is not initialized`: Check your InfluxDB configuration in the `.env` file
- `Error extracting data from block`: This might indicate FSUIPC version incompatibility
- Environment variable errors: Make sure all required variables are set in your `.env` file

## Architecture

This application uses an efficient approach to reading data from FSUIPC by using memory blocks instead of individual offsets. This reduces overhead and improves performance.

### Design Principles

1. **Single Responsibility**: Each extractor is responsible for processing data from exactly one memory block
2. **Clear Boundaries**: Memory blocks are defined to align with logical groupings of simulator data
3. **Separation of Concerns**: Data extraction is separated from data storage
4. **Structured Data**: The `ValueWithUnit` class provides clear units for all flight data metrics

### Data Flow

1. The bridge connects to FSUIPC to access simulator data
2. Data is read in efficient memory blocks
3. Specialized extractors process each data type from its designated memory block
4. The `ValueWithUnit` class encapsulates values with their corresponding units
5. Data is written directly to InfluxDB in batches for efficient storage
6. InfluxDB stores the data for visualization and analysis

## Data Categories

The bridge extracts the following categories of data:

- **Flight Data**: Position, altitude, attitude
- **Speed Data**: Ground speed, true/indicated airspeeds, vertical speed
- **Engine Data**: N1/N2, temperatures, pressures, fuel flow
- **Controls**: Trim, flaps, gear, spoilers
- **Autopilot**: Modes, settings, targets
- **Environment**: Atmospheric conditions, warnings
- **Lights**: Aircraft lighting systems
- **Navigation**: VOR, ILS, and radio data
- **Fuel**: Quantities and consumption

## Documentation

- [FSUIPC Reference](docs/FSUIPC.md) - Technical details about FSUIPC offsets and data formats

## Visualization

Once you have flight data stored in InfluxDB, you can visualize it using the [InfluxDB MSFS Demo](https://github.com/influx-demo/FlightSim2024-InfluxDB3Enterprise) project. This provides a dashboard with various visualizations of your flight data, including:

- Aircraft position and attitude
- Flight parameters
- Engine performance
- Navigation data

Follow the instructions in that repository to set up an interactive dashboard for your flight simulator data.

## Environment Variables

The application uses environment variables loaded from a `.env` file for configuration. Create this file in the root directory with the following variables:

| Variable | Description | Default |
|----------|-------------|---------|
| `INFLUX_URL` | URL of your InfluxDB instance | Required |
| `INFLUX_TOKEN` | API token with write access to your bucket | Required |
| `INFLUX_BUCKET` | Name of your InfluxDB bucket | Required |
| `BATCH_SIZE` | Maximum number of data points to batch before writing | 100 |
| `BATCH_AGE_MS` | Maximum age of a batch in milliseconds before writing | 100 |

Example `.env` file:

```
INFLUX_URL=http://localhost:8181
INFLUX_TOKEN=your_influxdb_token_here
INFLUX_BUCKET=flightsim
BATCH_SIZE=100
BATCH_AGE_MS=100
```

## Contributing

Contributions are welcome! Feel free to submit pull requests or open issues for bugs and feature requests.

## License

This project is licensed under the MIT License - see the LICENSE file for details.
