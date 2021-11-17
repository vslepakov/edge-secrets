param location string = resourceGroup().location
param name string = 'secret-delivery-app'
param containerAppEnvironmentId string

// Container Image ref
param containerImage string

// Networking
param useExternalIngress bool = false
param containerPort int

param keyVaultUrl string

param tenantId string
param applicationId string

@secure()
param applicationSecret string

@secure()
param webHookApiKey string

// TODO add Dapr
resource containerApp 'Microsoft.Web/containerApps@2021-03-01' = {
  name: name
  kind: 'containerapp'
  location: location
  properties: {
    kubeEnvironmentId: containerAppEnvironmentId
    configuration: {
      activeRevisionsMode: 'single'
      secrets: [
        {
          name: 'webhook-api-key'
          value: webHookApiKey
        }
        {
          name: 'application-clientid'
          value: applicationId
        }
        {
          name: 'application-client-secret'
          value: applicationSecret
        }
      ]
      registries: []
      ingress: {
        external: useExternalIngress
        targetPort: containerPort
      }
    }
    template: {
      containers: [
        {
          image: containerImage
          name: name
          env: [
            {
              name: 'X-API-KEY'
              secretref: 'webhook-api-key'
            }
            {
              name: 'AZURE_CLIENT_ID'
              secretref: 'application-clientid'
            }
            {
              name: 'AZURE_CLIENT_SECRET'
              secretref: 'application-client-secret'
            }
            {
              name: 'AZURE_TENANT_ID'
              value: tenantId
            }
            {
              name: 'AZURE_KEYVAULT_URL'
              value: keyVaultUrl
            }
          ]
          resources: {
            cpu: 1
            memory: '2Gi'
          }
        }
      ]
      scale: {
        minReplicas: 1
        rules: [
          {
            name: 'http-rule'
            http: {
              metadata: {
                concurrentRequests: '50'
              }
            }
          }
        ]
      }
    }
  }
}

output fqdn string = containerApp.properties.configuration.ingress.fqdn
