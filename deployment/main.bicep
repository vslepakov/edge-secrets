param location string = resourceGroup().location

param containerImage string
param containerPort int

param tenantId string
param objectId string
param applicationId string

@secure()
param applicationSecret string

// Securing access using an API Key for now. Later use AAD
@secure()
param webHookApiKey string

resource keyVault 'Microsoft.KeyVault/vaults@2021-06-01-preview' = {
  name: 'a${uniqueString(resourceGroup().id)}-kv'
  location: location
  properties: {
    accessPolicies: [
      {
        tenantId: tenantId
        objectId: objectId
        permissions: {
          secrets: [
            'get'
            'list'
          ]
        }
      }
    ]
    createMode: 'default'
    enableSoftDelete: false
    sku: {
      family: 'A'
      name: 'standard'
    }
    tenantId: tenantId
  }
}

module k8senv 'modules/k8senv.bicep' = {
  name: 'k8senv'
  params: {
    location: location
  }
}

module secretDeliveryApp 'modules/secretDeliveryApp.bicep' = {
  name: 'secretDeliveryApp'
  params: {
    name: 'secret-delivery-app'
    location: location
    containerImage: containerImage
    containerPort: containerPort
    tenantId: tenantId
    applicationId: applicationId
    applicationSecret: applicationSecret
    containerAppEnvironmentId: k8senv.outputs.k8senvId
    webHookApiKey: webHookApiKey
    keyVaultUrl: 'https://${keyVault.name}${environment().suffixes.keyvaultDns}'
  }
}

module iotHub 'modules/iothub_with_eventgrid_webhook.bicep' = {
  name: 'iothub-with-eventgrid'
  dependsOn: [
    secretDeliveryApp
  ]
  params: {
    location: location
    webHookApiKey: webHookApiKey
    webHookUrl: 'https://${secretDeliveryApp.outputs.fqdn}/events'
  }
}

output fqdn string = secretDeliveryApp.outputs.fqdn
