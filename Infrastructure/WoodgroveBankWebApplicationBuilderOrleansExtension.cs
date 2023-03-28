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
            var storageConnectionString = config.GetValue<string>(EnvironmentVariables.AzureStorageConnectionString);
            var clusterId = string.IsNullOrEmpty(config.GetValue<string>(EnvironmentVariables.OrleansClusterName))
                ? Defaults.ClusterName
                : config.GetValue<string>(EnvironmentVariables.OrleansClusterName);
            var serviceId = string.IsNullOrEmpty(config.GetValue<string>(EnvironmentVariables.OrleansServiceName))
                ? Defaults.ServiceName
                : config.GetValue<string>(EnvironmentVariables.OrleansServiceName);
            var siloName = string.IsNullOrEmpty(config.GetValue<string>(EnvironmentVariables.OrleansSiloName))
                ? Defaults.SiloName
                : config.GetValue<string>(EnvironmentVariables.OrleansSiloName);
            
            webApplicationBuilder.Host.UseOrleans(siloBuilder =>
            {
                // set up the cluster's id and service name
                siloBuilder.Configure<ClusterOptions>(clusterOptions =>
                {
                    clusterOptions.ClusterId = clusterId;
                    clusterOptions.ServiceId = serviceId;
                });

                // set up the silo name
                siloBuilder.Configure<SiloOptions>(options => options.SiloName = siloName);

                // use Kubernetes clustering
                siloBuilder.UseKubernetesHosting();

                // store persistent data in Azure Table Storage
                siloBuilder.AddAzureTableGrainStorageAsDefault(options =>
                {
                    options.ConfigureTableServiceClient(storageConnectionString);
                });

                // do any extra work the silo has requested
                if(siloAction != null)
                {
                    siloAction(siloBuilder);
                }
            });

            return webApplicationBuilder;
        }
    }
}