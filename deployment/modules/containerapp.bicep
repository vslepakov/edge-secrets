param location string
param name string
param containerAppEnvironmentId string

// Container Image ref
param containerImage string
param envVars array = []

// Networking
param useExternalIngress bool = false
param containerPort int

// TODO use in Dapr to KeyVault
param tenantId string
param applicationId string
@secure()
param applicationSecret string

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
          name: 'container-registry-password'
          value: 'todo'
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
          env: envVars
          resources: {
            cpu: 1
            memory: '2Gi'
          }
        }
      ]
      scale: {
        minReplicas: 1
      }
    }
  }
}

output fqdn string = containerApp.properties.configuration.ingress.fqdn
