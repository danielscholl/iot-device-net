#!/usr/bin/env bash
#
#  Purpose: Create a Resource Group an Edge VM deployed to it
#  Usage:
#    get-cert.sh


###############################
## ARGUMENT INPUT            ##
###############################

usage() { echo "Usage: device-cert.sh " 1>&2; exit 1; }

if [ -f ./.envrc ]; then source ./.envrc; fi

if [ -z $1 ]; then
  GROUP="iot-resources"
else
  GROUP=$1
fi

if [ -z $2 ]; then
  VAULT=$(az keyvault list --resource-group $GROUP --query [].name -otsv)
else
  VAULT=$2
fi

if [ -z $ORGANIZATION ]; then
  ORGANIZATION="testonly"
fi

printf "\n"
tput setaf 2; echo "Removing Old Certificates" ; tput sgr0
tput setaf 3; echo "------------------------------------" ; tput sgr0
rm -f cert/*.pem

printf "\n"
tput setaf 2; echo "Retrieving Required Certificates" ; tput sgr0
tput setaf 3; echo "------------------------------------" ; tput sgr0

# Download Root CA Certificate
az keyvault certificate download --name ${ORGANIZATION}-root-ca --vault-name $VAULT --file cert/root-ca.pem --encoding PEM
openssl pkcs12 -export -nokeys -in cert/root-ca.pem -out cert/root.pfx -password pass:password
# rm cert/root-ca.pem

# Download and extract PEM files for Device
az keyvault secret download --name $DEVICE --vault-name $VAULT --file cert/$DEVICE.pem --encoding base64
openssl pkcs12 -in cert/$DEVICE.pem -out cert/$DEVICE.cert.pem -nokeys -passin pass:
openssl pkcs12 -in cert/$DEVICE.pem -out cert/$DEVICE.key.pem -nodes -nocerts -passin pass:
openssl pkcs12 -export -in cert/$DEVICE.cert.pem -inkey cert/$DEVICE.key.pem -out cert/$DEVICE.pfx -password pass:password
rm cert/$DEVICE.pem cert/$DEVICE.cert.pem cert/$DEVICE.key.pem
