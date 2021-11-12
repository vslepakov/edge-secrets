param prefix string = 'secret-manager'
param location string = resourceGroup().location

param containerImage string
param containerPort int

param tenantId string
param objectId string
param applicationId string

@secure()
param applicationSecret string

resource iotHub 'Microsoft.Devices/IotHubs@2021-07-01' = {
  name: '${prefix}-${uniqueString(resourceGroup().id)}-iothub'
  location: location
  sku: {
    capacity: 1
    name: 'S1'
  }
  properties: {
    routing: {
      routes: [
        {
          condition: 'true' // TODO only route specific messages for secret requests
          endpointNames: [
            'eventgrid'
          ]
          isEnabled: true
          name: 'RouteToEventGrid'
          source: 'DeviceMessages'
        }
      ]
    }
  }
}

resource keyVault 'Microsoft.KeyVault/vaults@2021-06-01-preview' = {
  name: '${prefix}-keyvault'
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
    sku: {
      family: 'A'
      name: 'standard'
    }
    tenantId: tenantId
  }
}

resource loganalyticsWs 'Microsoft.OperationalInsights/workspaces@2020-03-01-preview' = {
  name: '${prefix}-${uniqueString(resourceGroup().id)}-loganalyticsws'
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
  name: '${prefix}-${uniqueString(resourceGroup().id)}-k8senv'
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
        name: 'TEST_ENV'
        value: 'TEST_VALUE'
      }
    ]
    useExternalIngress: true
    containerAppEnvironmentId: k8sEnv.id
  }
}

output fqdn string = containerApp.outputs.fqdn
