using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Orleans.Hosting
{
    public static class SiloBuilderExtensions
    {
        public static ISiloBuilder HostSiloInAzure(this ISiloBuilder siloBuilder, IConfiguration configuration)
        {
            // registry meta
            var clusterOptionsBuilder = new ClusterNameSiloBuilder();
            var siloOptionsBuilder = new SiloNameSiloBuilder();

            // storage
            var tableStorageBuilder = new AzureTableStorageClusteringSiloBuilder();

            // endpoints
            var webAppSiloBuilder = new AzureAppServiceSiloBuilder();
            var configuredEndpointsBuilder = new SiloEndpointsSiloBuilder();

            // monitoring
            var appInsightsBuilder = new AzureApplicationInsightsSiloBuilder();

            // bail out to use localhost
            var localhostBuilder = new LocalhostSiloBuilder();

            // set up the chain of responsibility

            // name the cluster & service
            clusterOptionsBuilder.SetNextBuilder(siloOptionsBuilder);

            // name the silo
            siloOptionsBuilder.SetNextBuilder(tableStorageBuilder);

            // wire up storage clustering if so configured
            tableStorageBuilder.SetNextBuilder(webAppSiloBuilder);

            // set up the endpoints according the Azure App Service, if detected
            webAppSiloBuilder.SetNextBuilder(configuredEndpointsBuilder);

            // set up the silo's endpoints (if not Kubernetes or Azure App Service)
            configuredEndpointsBuilder.SetNextBuilder(appInsightsBuilder);

            // extras
            appInsightsBuilder.SetNextBuilder(localhostBuilder);

            // build the silo
            clusterOptionsBuilder.Build(siloBuilder, configuration);

            return siloBuilder;
        }
    }
}
