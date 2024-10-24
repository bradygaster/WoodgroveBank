@description('The location for the resource(s) to be deployed.')
param location string = resourceGroup().location

param simulations_containerport string

param storage_outputs_tableendpoint string

param orleans_cluster_id_value string

param orleans_service_id_value string

param outputs_azure_container_apps_environment_default_domain string

param outputs_azure_container_registry_managed_identity_id string

param outputs_managed_identity_client_id string

param outputs_azure_container_apps_environment_id string

param outputs_azure_container_registry_endpoint string

param simulations_containerimage string

resource simulations 'Microsoft.App/containerApps@2024-03-01' = {
  name: 'simulations'
  location: location
  properties: {
    configuration: {
      activeRevisionsMode: 'Single'
      ingress: {
        external: false
        targetPort: simulations_containerport
        transport: 'http'
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
          image: simulations_containerimage
          name: 'simulations'
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
              value: simulations_containerport
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
              name: 'services__api__http__0'
              value: 'http://api.internal.${outputs_azure_container_apps_environment_default_domain}'
            }
            {
              name: 'services__api__https__0'
              value: 'https://api.internal.${outputs_azure_container_apps_environment_default_domain}'
            }
            {
              name: 'services__api__orleans-silo__0'
              value: 'tcp://api:8000'
            }
            {
              name: 'services__api__orleans-gateway__0'
              value: 'tcp://api:8001'
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