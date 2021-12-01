#!/bin/bash
set -e

if [ -z "$1" ]; then
  echo "No resource group name provided"
  exit 1
else
  rg=$1
fi

if [ -z "$2" ]; then
  echo "No container registry provided"
  exit 2
else
  containerRegistry=$2
fi

if [ -z "$3" ]; then
  echo "No container image URI provided"
  exit 3
else
  containerImage=$3
fi

if [ -z "$4" ]; then
  echo "No tenantId provided"
  exit 4
else
  tenantId=$4
fi

if [ -z "$5" ]; then
  echo "No applicationId provided"
  exit 5
else
  applicationId=$5
fi

if [ -z "$6" ]; then
  echo "No applicationSecret provided"
  exit 6
else
  applicationSecret=$6
fi

if [ -z "$7" ]; then
  echo "No webHookApiKey provided"
  exit 7
else
  webHookApiKey=$7
fi

if [ -z "$8" ]; then
  echo "No KeyVault URL provided"
  exit 8
else
  keyVaultUrl=$8
fi

if [ -z "$9" ]; then
  echo "No Container App Environment ID provided"
  exit 9
else
  containerAppEnvironmentId=$9
fi

if [ -z "${10}" ]; then
  echo "No IoT Hub Connection String provided"
  exit 10
else
  iotHubConnectionString=${10}
fi

az deployment group create -n app-deployment -g ${rg} \
  --template-file ./modules/secretDeliveryApp.bicep \
  -p containerRegistry=${containerRegistry} \
     containerImage=${containerImage} \
     containerPort=80 \
     containerAppEnvironmentId=${containerAppEnvironmentId} \
     tenantId=${tenantId} \
     applicationId=${applicationId} \
     applicationSecret=${applicationSecret} \
     webHookApiKey=${webHookApiKey} \
     keyVaultUrl=${keyVaultUrl} \
     iotHubConnectionString=${iotHubConnectionString}

echo "DONE!"