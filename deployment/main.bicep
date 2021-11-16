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
        applicationId: applicationId
        objectId: objectId
        permissions: {
          secrets: [
            'get'
            'list'
          ]
        }
        tenantId: tenantId
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

resource loganalyticsWs 'Microsoft.OperationalInsights/workspaces@2020-03-01-preview' = {
  name: '${uniqueString(resourceGroup().id)}-loganalyticsws'
  location: location
  properties: any({
    retentionInDays: 30
    features: {
      searchVersion: 1
    }
    sku: {
      name: 'PerGB2018'
    }
  })
}

resource k8sEnv 'Microsoft.Web/kubeEnvironments@2021-02-01' = {
  name: '${uniqueString(resourceGroup().id)}-k8senv'
  location: location
  properties: {
    type: 'managed'
    internalLoadBalancerEnabled: false
    appLogsConfiguration: {
      destination: 'log-analytics'
      logAnalyticsConfiguration: {
        customerId: loganalyticsWs.properties.customerId
        sharedKey: loganalyticsWs.listKeys().primarySharedKey
      }
    }
  }
}

module containerApp 'modules/containerapp.bicep' = {
  name: 'containerapp'
  params: {
    name: 'secret-manager-webapp'
    location: location
    containerImage: containerImage
    containerPort: containerPort
    tenantId: tenantId
    applicationId: applicationId
    applicationSecret: applicationSecret
    envVars: [
      {
        name: 'X-API-KEY'
        value: webHookApiKey
      }
    ]
    useExternalIngress: true
    containerAppEnvironmentId: k8sEnv.id
  }
}

module iotHub 'modules/iothub_with_eventgrid_webhook.bicep' = {
  name: 'iothub-with-eventgrid'
  dependsOn: [
    containerApp
  ]
  params: {
    location: location
    webHookApiKey: webHookApiKey
    webHookUrl: 'https://${containerApp.outputs.fqdn}/events'
  }
}

output fqdn string = containerApp.outputs.fqdn
