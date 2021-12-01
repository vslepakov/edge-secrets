param location string = resourceGroup().location

param containerRegistry string
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

resource iotHub 'Microsoft.Devices/IotHubs@2021-07-01' = {
  name: '${uniqueString(resourceGroup().id)}-iothub'
  location: location
  sku: {
    capacity: 1
    name: 'S1'
  }
  properties: {}
}

module secretDeliveryApp 'modules/secretDeliveryApp.bicep' = {
  name: 'secretDeliveryApp'
  params: {
    name: 'secret-delivery-app'
    location: location
    containerRegistry: containerRegistry
    containerImage: containerImage
    containerPort: containerPort
    tenantId: tenantId
    applicationId: applicationId
    applicationSecret: applicationSecret
    containerAppEnvironmentId: k8senv.outputs.k8senvId
    webHookApiKey: webHookApiKey
    keyVaultUrl: 'https://${keyVault.name}${environment().suffixes.keyvaultDns}'
    iotHubConnectionString: 'HostName=${iotHub.name}.azure-devices.net;SharedAccessKeyName=${listKeys(iotHub.id, '2020-04-01').value[1].keyName};SharedAccessKey=${listKeys(iotHub.id, '2020-04-01').value[1].primaryKey}'
  }
}

resource eventGridTopic 'Microsoft.EventGrid/systemTopics@2021-06-01-preview' = {
  name: '${uniqueString(resourceGroup().id)}-iothubtopic'
  location: location
  properties: {
    source: iotHub.id
    topicType: 'Microsoft.Devices.IotHubs'
  }
}

resource symbolicname 'Microsoft.EventGrid/systemTopics/eventSubscriptions@2021-06-01-preview' = {
  name: '${uniqueString(resourceGroup().id)}-iothubtopic-subscription'
  dependsOn: [
    secretDeliveryApp
  ]
  parent: eventGridTopic
  properties: {
    destination: {
      endpointType: 'WebHook'
      properties: {
        // Securing access using an API Key for now. Later use AAD.
        deliveryAttributeMappings: [
          {
            name: 'X-API-KEY'
            type: 'Static'
            properties: {
              value: webHookApiKey
              isSecret: true
            }
          }
        ]
        endpointUrl: 'https://${secretDeliveryApp.outputs.fqdn}/events'
        maxEventsPerBatch: 5
        preferredBatchSizeInKilobytes: 64
      }
    }
    eventDeliverySchema: 'CloudEventSchemaV1_0'
    filter: {
      advancedFilters: [
        {
          operatorType: 'IsNotNull'
          key: 'data.properties.secret-request-id'
        }
      ]
      enableAdvancedFilteringOnArrays: true
      includedEventTypes: [
        'Microsoft.Devices.DeviceTelemetry'
      ]
    }
    retryPolicy: {
      eventTimeToLiveInMinutes: 5
      maxDeliveryAttempts: 10
    }
  }
}

output fqdn string = secretDeliveryApp.outputs.fqdn
