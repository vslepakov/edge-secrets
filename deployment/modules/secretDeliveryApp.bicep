param location string = resourceGroup().location
param name string = 'secret-delivery-app'
param containerAppEnvironmentId string

// Container Image ref
param containerRegistry string
param containerImage string
param containerPort int
param keyVaultUrl string
param tenantId string
param applicationId string

@secure()
param applicationSecret string

@secure()
param webHookApiKey string

@secure()
param iotHubConnectionString string

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
          name: 'iothub-connectionstring'
          value: iotHubConnectionString
        }
        {
          name: 'application-clientid'
          value: applicationId
        }
        {
          name: 'application-client-secret'
          value: applicationSecret
        }
        {
          name: 'container-registry-password'
          value: applicationSecret
        }
      ]
      registries: [
        {
          server: containerRegistry
          username: applicationId
          passwordSecretRef: 'container-registry-password'
        }
      ]
      ingress: {
        external: true
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
              name: 'IOT_HUB_CONNECTION_STRING'
              secretref: 'iothub-connectionstring'
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
