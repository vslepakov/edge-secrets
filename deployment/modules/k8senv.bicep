param location string

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

output k8senvId string = k8sEnv.id
