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
  echo "No objectId provided"
  exit 5
else
  objectId=$5
fi

if [ -z "$6" ]; then
  echo "No applicationId provided"
  exit 6
else
  applicationId=$6
fi

if [ -z "$7" ]; then
  echo "No applicationSecret provided"
  exit 7
else
  applicationSecret=$7
fi

if [ -z "$8" ]; then
  echo "No webHookApiKey provided"
  exit 8
else
  webHookApiKey=$8
fi

getRandomString() {
  sed "s/[^a-zA-Z0-9]//g" <<< $(openssl rand -base64 4) | tr '[:upper:]' '[:lower:]'
}

deploymentName=secret-manager-all-$(getRandomString)

az group create -n ${rg} -l northeurope

az deployment group create -n ${deploymentName} -g ${rg} \
  --template-file ./main.bicep \
  -p containerRegistry=${containerRegistry} \
     containerImage=${containerImage} \
     containerPort=80 \
     tenantId=${tenantId} \
     objectId=${objectId} \
     applicationId=${applicationId} \
     applicationSecret=${applicationSecret} \
     webHookApiKey=${webHookApiKey}

echo "Done!"