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

az deployment group create -n app-deployment -g ${rg} \
  --template-file ./modules/containerapp.bicep \
  -p containerImage=${containerImage} \
     containerPort=80

echo "DONE!"