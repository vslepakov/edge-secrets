param location string
param webHookUrl string

@secure()
param webHookApiKey string

resource iotHub 'Microsoft.Devices/IotHubs@2021-07-01' = {
  name: '${uniqueString(resourceGroup().id)}-iothub'
  location: location
  sku: {
    capacity: 1
    name: 'S1'
  }
  properties: {}
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
  parent: eventGridTopic
  properties: {
    destination: {
      endpointType: 'WebHook'
      properties: {
        // Securing access using an API Key for now. Later use AAD.
        deliveryAttributeMappings: [
          {
            name: 'API-KEY'
            type: 'Static'
            properties: {
              value: webHookApiKey
              isSecret: true
            }
          }
        ]
        endpointUrl: webHookUrl
        maxEventsPerBatch: 5
        preferredBatchSizeInKilobytes: 64
      }
    }
    eventDeliverySchema: 'CloudEventSchemaV1_0'
    filter: {
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
