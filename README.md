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

### Environment Settings

Environment Settings act differently depending upon the Operating System and the IDE that is being used.


The best method to work with environment settings here is for command line to put environment settings in a .envrc file.  The `package.json` file is being used only as a convenient task runner but this helps just to execute the different actions and flip back and forth between different use cases of functionality.  For Windows Powershell the task.ps1 can be used as a manually created task runner for now.

The task runner is responsible to obtain the connection_string needed and pass it in at runtime.

.envrc file format
```bash
export GROUP="<resource_group>"
export HUB="<hub_name>"

export DEVICE="<device_name>"
export APPINSIGHTS_INSTRUMENTATIONKEY="<metrics_key>"
export PROTOCOL="MQTT"
export EDGE_GATEWAY="<dns_hostname>"
```

If using VSCode and the desire is to step into the debugger then the .env file is a best practice to use.  The .envrc file can not be used because the format includes export.  The vscode `launch.json` file includes the ability to point to the .env file where environment variables will be loaded from.

The device connection string must be put in the .env file as the task runner is not sending the connection string at runtime..

.env file format
```bash
DEVICE_CONNECTION_STRING="<device_connection_string>"

DEVICE="<device_name>"
APPINSIGHTS_INSTRUMENTATIONKEY="<metrics_key>"
PROTOCOL="MQTT"
EDGE_GATEWAY="<dns_hostname>"
```


If using Visual Studio then the environment variables are pulled out of the `Properties/launchSettings.json`


### Supported Use Cases

1. __Localhost Device Symmetric Key__

    _On a localhost register a device using Symmetric Key Authentication and send telemetry data_

1. __Localhost Device x509__

    _On a localhost register a device using x509 Certificate Authentication and send telemetry data_

