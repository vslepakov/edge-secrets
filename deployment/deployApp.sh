#!/bin/bash

if [ -z "$1" ]; then
  echo "No resource group name provided"
  exit 1
else
  rg=$1
fi

if [ -z "$2" ]; then
  echo "No container image URI provided"
  exit 2
else
  containerImage=$2
fi

if [ -z "$3" ]; then
  echo "No tenantId provided"
  exit 3
else
  tenantId=$3
fi

if [ -z "$4" ]; then
  echo "No applicationId provided"
  exit 4
else
  applicationId=$4
fi

if [ -z "$5" ]; then
  echo "No applicationSecret provided"
  exit 5
else
  applicationSecret=$5
fi

if [ -z "$6" ]; then
  echo "No webHookApiKey provided"
  exit 6
else
  webHookApiKey=$6
fi

az deployment group create -n app-deployment -g ${rg} \
  --template-file ./modules/containerapp.bicep \
  -p containerImage=${containerImage} \
     containerPort=80
     tenantId=${tenantId} \
     applicationId=${applicationId} \
     applicationSecret=${applicationSecret} \
     webHookApiKey=${webHookApiKey}

echo "DONE!"