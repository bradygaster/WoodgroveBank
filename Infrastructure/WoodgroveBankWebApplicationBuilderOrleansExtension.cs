using Orleans.Configuration;
using WoodgroveBank.Infrastructure;

namespace Microsoft.AspNetCore.Builder
{
    public static class WoodgroveBankWebApplicationBuilderOrleansExtension
    {
        public static WebApplicationBuilder AddWoodgroveBankSilo(this WebApplicationBuilder webApplicationBuilder, Action<ISiloBuilder>? siloAction = null)
        {
            // read from configuration or use default setting values
            var config = webApplicationBuilder.Configuration;
            var storageConnectionString = string.IsNullOrEmpty(config.GetValue<string>(EnvironmentVariables.AzureStorageConnectionString))
                ? Defaults.AzuriteConnectionString
                : config.GetValue<string>(EnvironmentVariables.AzureStorageConnectionString);
            var clusterId = string.IsNullOrEmpty(config.GetValue<string>(EnvironmentVariables.OrleansClusterName))
                ? Defaults.ClusterName
                : config.GetValue<string>(EnvironmentVariables.OrleansClusterName);
            var serviceId = string.IsNullOrEmpty(config.GetValue<string>(EnvironmentVariables.OrleansServiceName))
                ? Defaults.ServiceName
                : config.GetValue<string>(EnvironmentVariables.OrleansServiceName);
            var siloName = string.IsNullOrEmpty(config.GetValue<string>(EnvironmentVariables.OrleansSiloName))
                ? Defaults.SiloName
                : config.GetValue<string>(EnvironmentVariables.OrleansSiloName);
            var siloPort = string.IsNullOrEmpty(config.GetValue<string>(EnvironmentVariables.OrleansSiloPort))
                ? Defaults.SiloPort
                : config.GetValue<int>(EnvironmentVariables.OrleansSiloPort);
            var gatewayPort = string.IsNullOrEmpty(config.GetValue<string>(EnvironmentVariables.OrleansGatewayPort))
                ? Defaults.GatewayPort
                : config.GetValue<int>(EnvironmentVariables.OrleansGatewayPort);

            webApplicationBuilder.Host.UseOrleans(siloBuilder =>
            {                
#if DEBUG
                // set up the cluster's id and service name
                siloBuilder.Configure<ClusterOptions>(clusterOptions =>
                {
                    clusterOptions.ClusterId = clusterId;
                    clusterOptions.ServiceId = serviceId;
                });

                // set up the silo name
                siloBuilder.Configure<SiloOptions>(options => options.SiloName = siloName);

                // configure silo and gateway ports
                siloBuilder.ConfigureEndpoints(siloPort: siloPort, gatewayPort: gatewayPort);
#else
                // use Kubernetes hosting
                siloBuilder.UseKubernetesHosting();
#endif
                // use table storage for clustering
                siloBuilder.UseAzureStorageClustering(options =>
                    options.ConfigureTableServiceClient(storageConnectionString));

                // store persistent data in Azure Table Storage
                siloBuilder.AddAzureTableGrainStorageAsDefault(options =>
                {
                    options.ConfigureTableServiceClient(storageConnectionString);
                });

                // set up streaming
                siloBuilder.AddMemoryStreams("ADMIN")
                           .AddAzureTableGrainStorage("PubSubStore", options =>
                           {
                               options.ConfigureTableServiceClient(storageConnectionString);
                           });

                // do any extra work the silo has requested
                if (siloAction != null)
                {
                    siloAction(siloBuilder);
                }
            });

            return webApplicationBuilder;
        }
    }
}