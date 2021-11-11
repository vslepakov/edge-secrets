param location string
param prefix string
param containerAppEnvironmentId string

// Container Image ref
param containerImage string

// Networking
param useExternalIngress bool = false
param containerPort int

param envVars array = []

resource containerApp 'Microsoft.Web/containerApps@2021-03-01' = {
  name: '${prefix}-${uniqueString(resourceGroup().id)}-containerapp'
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
          name: '${prefix}-app'
          env: envVars
          resources: {
            cpu: 1
            memory: '250Mb'
          }
        }
      ]
      scale: {
        minReplicas: 1
      }
    }
  }
}
