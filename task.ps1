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
  [string] $run = "none"
)

# Load Project Environment Settings and Functions
if (Test-Path "$PSScriptRoot\.env.ps1") { . "$PSScriptRoot\.env.ps1" }
if (Test-Path "$PSScriptRoot\scripts\functions.ps1") { . "$PSScriptRoot\scripts\functions.ps1" }

# Display Project Environment Settings
if ($run -eq "set") {
  [Environment]::SetEnvironmentVariable("APPINSIGHTS_INSTRUMENTATIONKEY", $Env:APPINSIGHTS_INSTRUMENTATIONKEY, "User")
  [Environment]::SetEnvironmentVariable("IOT_PROTOCOL", $Env:IOT_PROTOCOL, "User")
  [Environment]::SetEnvironmentVariable("IOT_DPS_HOST", $Env:IOT_DPS_HOST, "User")
  [Environment]::SetEnvironmentVariable("IOT_ID_SCOPE", $Env:IOT_ID_SCOPE, "User")
  [Environment]::SetEnvironmentVariable("IOT_DEVICE", $Env:IOT_DEVICE, "User")
  Get-ChildItem Env:ARM_*
  Get-ChildItem Env:AZURE_*
  Get-ChildItem Env:IOT_*
  Get-ChildItem Env:APPINSIGHTS*
}

if ($run -eq "unset") {
  [Environment]::SetEnvironmentVariable("APPINSIGHTS_INSTRUMENTATIONKEY", $Env:APPINSIGHTS_INSTRUMENTATIONKEY, "User")
  [Environment]::SetEnvironmentVariable("IOT_PROTOCOL", $null, "User")
  [Environment]::SetEnvironmentVariable("IOT_DPS_HOST", $null, "User")
  [Environment]::SetEnvironmentVariable("IOT_ID_SCOPE", $null, "User")
  [Environment]::SetEnvironmentVariable("IOT_DEVICE", $null, "User")
  Get-ChildItem Env:ARM_*
  Get-ChildItem Env:AZURE_*
  Get-ChildItem Env:IOT_*
  Get-ChildItem Env:APPINSIGHTS*
}

if ($run -eq "device") {
  Write-Host "Creating Device...." -ForegroundColor "cyan"
  az iot hub device-identity create -d $Env:IOT_DEVICE -n $Env:AZURE_HUB -ojsonc
}

if ($run -eq "clean") {
  Write-Host "Cleaning up devices and certificates...." -ForegroundColor "cyan"
  az iot hub device-identity delete -d $Env:IOT_DEVICE -n $Env:AZURE_HUB -ojsonc
  Remove-Item ./cert/*.pem
}

if ($run -eq "device:x509") {
  Write-Host "Creating Device...." -ForegroundColor "cyan"
  az iot hub device-identity create -d $Env:IOT_DEVICE -n $Env:AZURE_HUB  --am x509_thumbprint --output-dir cert -ojsonc
  openssl pkcs12 -export -in cert/$Env:IOT_DEVICE-cert.pem -inkey cert/$Env:IOT_DEVICE-key.pem -out cert/$Env:IOT_DEVICE.pfx -password pass:password
  Remove-Item cert/*.pem
}

if ($run -eq "monitor") {
  Write-Host "Monitor Device...." -ForegroundColor "cyan"
  az iot hub monitor-events --hub-name $Env:AZURE_HUB --device-id $Env:IOT_DEVICE
}

if ($run -eq "none") {
  Write-Host "Starting Device...." -ForegroundColor "cyan"
  $Env:DEVICE_CONNECTION_STRING = (az iot hub device-identity show-connection-string --hub-name $Env:AZURE_HUB  --device-id $Env:IOT_DEVICE -otsv)
  dotnet build
  dotnet run
}
