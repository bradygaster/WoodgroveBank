using './simulations.module.bicep'

param orleans_cluster_id_value = '{{ parameter "orleans_cluster_id" }}'
param orleans_service_id_value = '{{ parameter "orleans_service_id" }}'
param outputs_azure_container_apps_environment_default_domain = '{{ .Env.AZURE_CONTAINER_APPS_ENVIRONMENT_DEFAULT_DOMAIN }}'
param outputs_azure_container_apps_environment_id = '{{ .Env.AZURE_CONTAINER_APPS_ENVIRONMENT_ID }}'
param outputs_azure_container_registry_endpoint = '{{ .Env.AZURE_CONTAINER_REGISTRY_ENDPOINT }}'
param outputs_azure_container_registry_managed_identity_id = '{{ .Env.AZURE_CONTAINER_REGISTRY_MANAGED_IDENTITY_ID }}'
param outputs_managed_identity_client_id = '{{ .Env.MANAGED_IDENTITY_CLIENT_ID }}'
param simulations_containerimage = '{{ .Image }}'
param simulations_containerport = '{{ targetPortOrDefault 8080 }}'
param storage_outputs_tableendpoint = '{{ .Env.STORAGE_TABLEENDPOINT }}'
