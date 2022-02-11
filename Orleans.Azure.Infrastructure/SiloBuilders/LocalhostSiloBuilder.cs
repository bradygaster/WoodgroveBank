using Microsoft.Extensions.Configuration;

namespace Orleans.Hosting
{
    internal class LocalhostSiloBuilder : AzureSiloBuilder
    {
        public override void Build(ISiloBuilder siloBuilder, IConfiguration configuration)
        {
            if (string.IsNullOrEmpty(configuration.GetValue<string>(EnvironmentVariables.AzureStorageConnectionString)) &&
                string.IsNullOrEmpty(configuration.GetValue<string>(EnvironmentVariables.OrleansSiloPort)) &&
                string.IsNullOrEmpty(configuration.GetValue<string>(EnvironmentVariables.OrleansGatewayPort)) &&
                string.IsNullOrEmpty(configuration.GetValue<string>(EnvironmentVariables.WebAppsPrivateIPAddress)) &&
                string.IsNullOrEmpty(configuration.GetValue<string>(EnvironmentVariables.WebAppsPrivatePorts)) &&
                string.IsNullOrEmpty(configuration.GetValue<string>(EnvironmentVariables.KubernetesPodName)) &&
                string.IsNullOrEmpty(configuration.GetValue<string>(EnvironmentVariables.KubernetesPodNamespace)) &&
                string.IsNullOrEmpty(configuration.GetValue<string>(EnvironmentVariables.KubernetesPodIPAddress)))
                // check for other clustering configurations, and if none are found...)
            {
                siloBuilder.UseLocalhostClustering();
            }

            base.Build(siloBuilder, configuration);
        }
    }
}