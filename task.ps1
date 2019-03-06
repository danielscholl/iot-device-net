<#
.SYNOPSIS
  Install the Full Infrastructure As Code Solution
.DESCRIPTION
  This Script will install all the infrastructure needed for the solution.

  1. Resource Group


.EXAMPLE
  .\install.ps1
  Version History
  v1.0   - Initial Release
#>
#Requires -Version 5.1
#Requires -Module @{ModuleName='AzureRM.Resources'; ModuleVersion='5.0'}

param(
  [string] $run = "start"
)

$Env:ARM_TENANT_ID = [string]::Empty
$Env:ARM_SUBSCRIPTION_ID = [string]::Empty
$Env:ARM_CLIENT_ID = [string]::Empty
$Env:ARM_CLIENT_SECRET = [string]::Empty
$Env:GROUP = [string]::Empty
$Env:HUB = [string]::Empty
$Env:REGISTRY_SERVER = [string]::Empty
$Env:DEVICE = [string]::Empty
$Env:APPINSIGHTS_INSTRUMENTATIONKEY = [string]::Empty
$Env:PROTOCOL = [string]::Empty
$Env:DPS_HOST = [string]::Empty
$Env:ID_SCOPE = [string]::Empty
$Env:EDGE_GATEWAY = [string]::Empty
$Env:DEVICE_CONNECTION_STRING = [string]::Empty

# Load Project Environment Settings and Functions
if (Test-Path "$PSScriptRoot\.env.ps1") { . "$PSScriptRoot\.env.ps1" }

$ARM_TENANT_ID = $Env:ARM_TENANT_ID
$ARM_SUBSCRIPTION_ID = $Env:ARM_SUBSCRIPTION_ID
$ARM_CLIENT_ID = $Env:ARM_CLIENT_ID
$ARM_CLIENT_SECRET = $Env:ARM_CLIENT_SECRET
$HUB = $Env:HUB
$GROUP = $Env:GROUP
$REGISTRY_SERVER = $Env:REGISTRY_SERVER
$DEVICE = $Env:DEVICE
$APPINSIGHTS_INSTRUMENTATIONKEY = $Env:APPINSIGHTS_INSTRUMENTATIONKEY
$PROTOCOL = $Env:PROTOCOL
$DPS_HOST = $Env:DPS_HOST
$ID_SCOPE = $Env:ID_SCOPE
$EDGE_GATEWAY = $Env:EDGE_GATEWAY
$DEVICE_CONNECTION_STRING = $Env:DEVICE_CONNECTION_STRING


# Display Project Environment Settings
if ($run -eq "env") {
  Get-ChildItem Env:ARM* -ErrorAction SilentlyContinue
  Get-ChildItem Env:GROUP -ErrorAction SilentlyContinue
  Get-ChildItem Env:HUB -ErrorAction SilentlyContinue
  Get-ChildItem Env:REGISTRY_SERVER -ErrorAction SilentlyContinue
  Get-ChildItem Env:DEVICE -ErrorAction SilentlyContinue
  Get-ChildItem Env:APPINSIGHTS_INSTRUMENTATIONKEY -ErrorAction SilentlyContinue
  Get-ChildItem Env:PROTOCOL -ErrorAction SilentlyContinue
  Get-ChildItem Env:DPS_HOST -ErrorAction SilentlyContinue
  Get-ChildItem Env:ID_SCOPE -ErrorAction SilentlyContinue
  Get-ChildItem Env:EDGE_GATEWAY -ErrorAction SilentlyContinue
  Get-ChildItem Env:DEVICE_CONNECTION_STRING -ErrorAction SilentlyContinue
}

if ($run -eq "clean") {
  Write-Host "Cleaning up devices and certificates...." -ForegroundColor "cyan"
  az iot hub device-identity delete -d $DEVICE -n $HUB -ojsonc
  Remove-Item ./cert/*.pfx
  dotnet clean
}

if ($run -eq "device") {
  Write-Host "Creating Device...." -ForegroundColor "cyan"
  az iot hub device-identity create -d $DEVICE -n $HUB -ojsonc
}

if ($run -eq "device:x509") {
  Write-Host "Creating Device...." -ForegroundColor "cyan"
  az iot hub device-identity create -d $DEVICE -n $HUB  --am x509_thumbprint --output-dir cert -ojsonc
  openssl pkcs12 -export -in cert/$DEVICE-cert.pem -inkey cert/$DEVICE-key.pem -out cert/$DEVICE.pfx -password pass:password
  Remove-Item cert/*.pem
}

if ($run -eq "device:leaf") {
  Write-Host "Creating a Downstream Device...." -ForegroundColor "cyan"
  Remove-Item cert/*.pem
  ./cert/cert.ps1
  az iot hub device-identity create -d $DEVICE -n $HUB -ojsonc
  Remove-Item cert/device.pfx
}

if ($run -eq "docker") {
  Write-Host "Starting up Docker...." -ForegroundColor "cyan"
  docker build -t $REGISTRY_SERVER/iot-device-net:latest .
  docker run -it --name $DEVICE --mount source=$PSScriptRoot/cert,target=/usr/src/app/cert,type=bind -e PROTOCOL=$PROTOCOL -e EDGE_GATEWAY=$EDGE_GATEWAY -e APPINSIGHTS_INSTRUMENTATIONKEY=$APPINSIGHTS_INSTRUMENTATIONKEY -e DEVICE_CONNECTION_STRING=$(az iot hub device-identity show-connection-string --hub-name $HUB --device-id $DEVICE -otsv) $REGISTRY_SERVER/iot-device-net:latest
}

if ($run -eq "docker:stop") {
  Write-Host "Shutting down Docker...." -ForegroundColor "cyan"
  docker rm -f $DEVICE
}


if ($run -eq "monitor") {
  Write-Host "Monitor Device...." -ForegroundColor "cyan"
  az iot hub monitor-events --hub-name $HUB --device-id $DEVICE
}

if ($run -eq "start") {
  Write-Host "Starting Device...." -ForegroundColor "cyan"
  dotnet build
  $env:DEVICE_CONNECTION_STRING = (az iot hub device-identity show-connection-string --hub-name $HUB --device-id $DEVICE -otsv)
  dotnet run
}
