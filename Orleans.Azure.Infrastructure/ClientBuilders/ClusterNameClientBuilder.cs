using Microsoft.Extensions.Configuration;
using Orleans.Configuration;

namespace Orleans.Hosting
{
    public class ClusterNameClientBuilder : AzureSiloClientBuilder
    {
        public override void Build(IClientBuilder clientBuilder, IConfiguration configuration)
        {
            var clusterId = string.IsNullOrEmpty(configuration.GetValue<string>(EnvironmentVariables.OrleansClusterName)) ? Defaults.ClusterName : configuration.GetValue<string>(EnvironmentVariables.OrleansClusterName);
            var serviceId = string.IsNullOrEmpty(configuration.GetValue<string>(EnvironmentVariables.OrleansServiceName)) ? Defaults.ServiceName : configuration.GetValue<string>(EnvironmentVariables.OrleansServiceName);

            clientBuilder.Configure<ClusterOptions>(clusterOptions =>
            {
                clusterOptions.ClusterId = clusterId;
                clusterOptions.ServiceId = serviceId;
            });

            base.Build(clientBuilder, configuration);
        }
    }
}
