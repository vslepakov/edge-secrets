#!/bin/bash

if [ -z "$1" ]; then
  echo "No resource group name provided"
  exit 1
else
  rg=$1
fi

# Configure defaults
if [ -z "$2" ]; then
  echo "No coontainer image URI provided"
  exit 2
else
  containerImage=$2
fi

az group create -n ${rg} -l northeurope

az deployment group create -n secret-manager-all -g ${rg} \
  --template-file ./main.bicep \
  -p containerImage=${containerImage} \
     containerPort=80

fqdn=$(az deployment group show -g ${rg} --query properties.outputs.fqdn.value \
  -n secret-manager-all -o tsv)

echo "Your app is deployed here: ${fqdn}"