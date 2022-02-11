using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Orleans.Hosting
{
    public static class SiloBuilderExtensions
    {
        public static ISiloBuilder HostSiloInAzure(this ISiloBuilder siloBuilder, IConfiguration configuration)
        {
            // registry meta
            var clusterNameSiloBuilder = new ClusterNameSiloBuilder();
            var siloNameSiloBuilder = new SiloNameSiloBuilder();

            // storage
            var tableStorageBuilder = new AzureTableStorageClusteringSiloBuilder();

            // endpoints
            var azureAppServiceSiloBuilder = new AzureAppServiceSiloBuilder();
            var kubernetesSiloBuilder = new KubernetesSiloBuilder();
            var siloEndpointsBuilder = new SiloEndpointsSiloBuilder();

            // monitoring
            var appInsightsBuilder = new AzureApplicationInsightsSiloBuilder();

            // bail out to use localhost
            var localhostBuilder = new LocalhostSiloBuilder();

            // set up the chain of responsibility

            // name the cluster & service
            clusterNameSiloBuilder.SetNextBuilder(siloNameSiloBuilder);

            // name the silo
            siloNameSiloBuilder.SetNextBuilder(tableStorageBuilder);

            // wire up storage clustering if so configured
            tableStorageBuilder.SetNextBuilder(azureAppServiceSiloBuilder);

            // set up the endpoints according to Azure App Service, if detected
            azureAppServiceSiloBuilder.SetNextBuilder(kubernetesSiloBuilder);

            // set up the endpoints according to Kubernetes, if detected
            kubernetesSiloBuilder.SetNextBuilder(siloEndpointsBuilder);

            // set up the silo's endpoints (if not Kubernetes or Azure App Service)
            siloEndpointsBuilder.SetNextBuilder(appInsightsBuilder);

            // extras
            appInsightsBuilder.SetNextBuilder(localhostBuilder);

            // build the silo
            clusterNameSiloBuilder.Build(siloBuilder, configuration);

            return siloBuilder;
        }
    }
}
