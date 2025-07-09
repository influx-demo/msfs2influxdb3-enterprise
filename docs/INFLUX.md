# Sending Data to InfluxDB

MSFS2MQTT can send data to InfluxDB using the [Telegraf](https://docs.influxdata.com/telegraf/v1/install/) agent. Telegraf is a data collection agent that can collect data from various sources and send it to InfluxDB.

Once in InfluxDB, why not check out the [InfluxDB MSFS Demo](https://github.com/influx-demo/FlightSim2024-InfluxDB3Enterprise) to see how to visualize the data?

### Installation

1. Install [Telegraf](https://docs.influxdata.com/telegraf/v1/install/)

2. Copy the example file `telegraf.example.conf` to `telegraf.conf` and edit to add your bucket name and token

3. Start Telegraf with your configuration by running this a PowerShell window in this project's directory:
   ```powershell
   & 'C:\Program Files\InfluxData\telegraf\telegraf.exe' --config .\telegraf.conf
   ```