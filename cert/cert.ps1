<#
.SYNOPSIS
  Infrastructure as Code Component
.DESCRIPTION
  Install a Keyvault
.EXAMPLE
  .\install.ps1
  Version History
  v1.0   - Initial Release
#>
#Requires -Version 5.1

Param(
  [string] $GROUP = $env:GROUP,
  [string] $ORGANIZATION = "testonly",
  [string] $VAULT = (az keyvault list --resource-group $GROUP --query [].name -otsv),
  [string] $DEVICE = $env:DEVICE
)

if (Test-Path "$PSScriptRoot\.env.ps1") { . "$PSScriptRoot\.env.ps1" }

Write-Host "Removing Old Certificates" -ForegroundColor "yellow"
Write-Host "---------------------------------------------" -ForegroundColor "blue"
Remove-Item cert/*.pfx

Write-Host "Retrieving Required Certificates" -ForegroundColor "yellow"
Write-Host "---------------------------------------------" -ForegroundColor "blue"

# Download Root CA Certificate
az keyvault certificate download --name $ORGANIZATION-root-ca --vault-name $VAULT --file cert/root-ca.pem --encoding PEM
openssl pkcs12 -export -nokeys -in cert/root-ca.pem -out cert/root.pfx -password pass:password
# Remove-Item cert/root-ca.pem

# Download and extract PEM files for Device
az keyvault secret download --name $DEVICE --vault-name $VAULT --file cert/$DEVICE.pem --encoding base64
openssl pkcs12 -in cert/$DEVICE.pem -out cert/$DEVICE.cert.pem -nokeys -passin pass:
openssl pkcs12 -in cert/$DEVICE.pem -out cert/$DEVICE.key.pem -nodes -nocerts -passin pass:
openssl pkcs12 -export -in cert/$DEVICE.cert.pem -inkey cert/$DEVICE.key.pem -out cert/$DEVICE.pfx -password pass:password
Remove-Item cert/$DEVICE.pem, cert/$DEVICE.cert.pem, cert/$DEVICE.key.pem
