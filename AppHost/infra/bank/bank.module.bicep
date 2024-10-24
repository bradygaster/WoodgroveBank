@description('The location for the resource(s) to be deployed.')
param location string = resourceGroup().location

param bank_containerport string

param storage_outputs_tableendpoint string

param orleans_cluster_id_value string

param orleans_service_id_value string

param storage_outputs_blobendpoint string

param outputs_azure_container_registry_managed_identity_id string

param outputs_managed_identity_client_id string

param outputs_azure_container_apps_environment_id string

param outputs_azure_container_registry_endpoint string

param bank_containerimage string

resource bank 'Microsoft.App/containerApps@2024-03-01' = {
  name: 'bank'
  location: location
  properties: {
    configuration: {
      activeRevisionsMode: 'Single'
      ingress: {
        external: true
        targetPort: bank_containerport
        transport: 'http'
        additionalPortMappings: [
          {
            external: false
            targetPort: 8000
          }
          {
            external: false
            targetPort: 8001
          }
        ]
      }
      registries: [
        {
          server: outputs_azure_container_registry_endpoint
          identity: outputs_azure_container_registry_managed_identity_id
        }
      ]
    }
    environmentId: outputs_azure_container_apps_environment_id
    template: {
      containers: [
        {
          image: bank_containerimage
          name: 'bank'
          env: [
            {
              name: 'OTEL_DOTNET_EXPERIMENTAL_OTLP_EMIT_EXCEPTION_LOG_ATTRIBUTES'
              value: 'true'
            }
            {
              name: 'OTEL_DOTNET_EXPERIMENTAL_OTLP_EMIT_EVENT_LOG_ATTRIBUTES'
              value: 'true'
            }
            {
              name: 'OTEL_DOTNET_EXPERIMENTAL_OTLP_RETRY'
              value: 'in_memory'
            }
            {
              name: 'ASPNETCORE_FORWARDEDHEADERS_ENABLED'
              value: 'true'
            }
            {
              name: 'HTTP_PORTS'
              value: bank_containerport
            }
            {
              name: 'Orleans__Clustering__ProviderType'
              value: 'AzureTableStorage'
            }
            {
              name: 'Orleans__Clustering__ServiceKey'
              value: 'clustering'
            }
            {
              name: 'ConnectionStrings__clustering'
              value: storage_outputs_tableendpoint
            }
            {
              name: 'Orleans__ClusterId'
              value: orleans_cluster_id_value
            }
            {
              name: 'Orleans__ServiceId'
              value: orleans_service_id_value
            }
            {
              name: 'Orleans__EnableDistributedTracing'
              value: 'true'
            }
            {
              name: 'Orleans__GrainStorage__grainState__ProviderType'
              value: 'AzureBlobStorage'
            }
            {
              name: 'Orleans__GrainStorage__grainState__ServiceKey'
              value: 'grainState'
            }
            {
              name: 'ConnectionStrings__grainState'
              value: storage_outputs_blobendpoint
            }
            {
              name: 'Orleans__Endpoints__SiloPort'
              value: '8000'
            }
            {
              name: 'Orleans__Endpoints__GatewayPort'
              value: '8001'
            }
            {
              name: 'AZURE_CLIENT_ID'
              value: outputs_managed_identity_client_id
            }
          ]
        }
      ]
      scale: {
        minReplicas: 1
        maxReplicas: 1
      }
    }
  }
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${outputs_azure_container_registry_managed_identity_id}': { }
    }
  }
}