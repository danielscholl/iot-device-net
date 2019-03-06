# iot-device-net

The purpose of this solution is to be able to easily deploy and run IoT Devices to test different features.

[![Build Status](https://dascholl.visualstudio.com/IoT/_apis/build/status/danielscholl.iot-device-net?branchName=master)](https://dascholl.visualstudio.com/IoT/_build/latest?definitionId=23&branchName=master)

__PreRequisites__

Requires the use of [direnv](https://direnv.net/).  
Requires the use of [Azure CLI](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli?view=azure-cli-latest).  
Requires the use of [Docker](https://www.docker.com/get-started).  


### Related Repositories

- [iot-resources](https://github.com/danielscholl/iot-resources)  - Deploying IoT Resources and x509 Management
- [iot-device-edge](https://github.com/danielscholl/iot-device-edge) - Simple Edge Testing
- [iot-device-js](https://github.com/danielscholl/iot-device-js) - Simple Device Testing (NodeJS)
- [iot-device-js](https://github.com/danielscholl/iot-device-net) - Simple Device Testing (C#)
- [iot-control-js](https://github.com/danielscholl/iot-control-js) - Simple Control Testing


### Supported Use Cases

1. __Localhost Device Symmetric Key__

    _On a localhost register a device using Symmetric Key Authentication and send telemetry data_

1. __Docker Device Symmetric Key__

    _Within a container register a device using Symmetric Key Authentication and send telemetry data_

1. __Localhost Device x509__

    _On a localhost register a device using x509 Certificate Authentication and send telemetry data_

1. __Docker Device x509__

    _Within a container register a device using x509 Certificate Authentication and send telemetry data__



### Environment Settings

Environment Settings act differently depending upon the Operating System and the IDE that is being used.


_MacOSX_
The best method to work with environment settings here is for command line to put environment settings in a .envrc file.  The `package.json` is being used as a convenient task runner.

.envrc file format
```bash
export GROUP="<resource_group>"
export HUB="<hub_name>"

export DEVICE="<device_name>"
export APPINSIGHTS_INSTRUMENTATIONKEY="<metrics_key>"
export PROTOCOL="MQTT"
export EDGE_GATEWAY="<dns_hostname>"
```

_Windows_
The best method to work with environment settings here for command line is to put environment settings in a .env.ps1 file.  The `task.ps1` script is being used as a convenient task runner.

.env.ps1 file format
```bash
# These are for Task Runner
$Env:GROUP = "<resource_group>"
$Env:HUB = "<hub_name>"
$Env:REGISTRY_SERVER = "<docker_registry>"

# These are for Device
$Env:DEVICE="<device_name>"
$Env:APPINSIGHTS_INSTRUMENTATIONKEY = "<metrics_key"
$Env:PROTOCOL="<protocol>"

# These are for DPS Provisioning
$Env:DPS_HOST="global.azure-devices-provisioning.net"
$Env:ID_SCOPE="<id_scope>"
$Env:EDGE_GATEWAY="<edge_gateway>"
```

_VSCode_
The best method to work with environment settings here for command line is to put environment settings in a .env file and reference it with the vscode `launch.json` file.

The device connection string must be put in the .env file as the task runner is not sending the connection string at runtime..

.env file format
```bash
# These are for Device
DEVICE="<device_name>"
APPINSIGHTS_INSTRUMENTATIONKEY = "<metrics_key>"
PROTOCOL="<protocol>"

# These are for DPS Provisioning
DPS_HOST="global.azure-devices-provisioning.net"
ID_SCOPE="<id_scope>"
EDGE_GATEWAY="<edge_gateway>"


## THIS MUST BE SET TO FEED IN THE CONNECTION STRING
DEVICE_CONNECTION_STRING="<connection_string>"
```


If using Visual Studio then the environment variables are pulled out of the `Properties/launchSettings.json`

## LocalHost Device Simulation

Windows Powershell
```powershell
# Setup the Environment Variables in .env.ps1
$Env:GROUP = "iot-resources"
$Env:DEVICE="device"
$Env:HUB = (az iot hub list --resource-group $GROUP --query [].name -otsv)
$Env:HUB_CONNECTION_STRING = (az iot hub show-connection-string --hub-name $HUB)


# Option A:  Self register a Device with either Symmetric Key or a self signed x509 Certificate
./task.ps1 -run device        # Create Device with Symetric Key
./task.ps1 -run device:x509   # Create Device With x509

# Run the Device
$Env:DEVICE_CONNECTION_STRING= (az iot hub device-identity show-connection-string --hub-name $HUB --device-id $DEVICE -otsv)
./task.ps1

# Monitor the Device in a seperate terminal session
./task.ps1 -run monitor

# Remove the Device
./task.ps1 -run clean
```

```bash
# Setup the Environment Variables
export GROUP="iot-resources"
export DEVICE="device"
export HUB=$(az iot hub list --resource-group $GROUP --query [].name -otsv)
export HUB_CONNECTION_STRING=$(az iot hub show-connection-string --hub-name $HUB)

# Install
npm install

# Option A:  Self register a Device with either Symmetric Key or a self signed x509 Certificate
npm run device            # Create Device with Symetric Key
npm run device:x509       # Create Device With x509

# Run the Device
DEVICE_CONNECTION_STRING=$(az iot hub device-identity show-connection-string --hub-name $HUB --device-id $DEVICE -otsv) npm start

# Monitor the Device in a seperate terminal session
npm run monitor

# Remove the Device
npm run clean
```
