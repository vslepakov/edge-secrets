param prefix string = 'secret-manager'
param location string = resourceGroup().location

param containerImage string
param containerPort int

//resource iotHub 'Microsoft.Devices/IotHubs@2021-07-01' = {}

//resource keyVault 'Microsoft.KeyVault/vaults@2021-06-01-preview' = {}

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
    prefix: prefix
    location: location
    containerImage: containerImage
    containerPort: containerPort
    envVars: [
      {
        name: 'TEST_VAR_NAME'
        value: 'TEST_VAR_VALUE'
      }
    ]
    useExternalIngress: true
    containerAppEnvironmentId: k8sEnv.id
  }
}
output fqdn string = containerApp.outputs.fqdn
