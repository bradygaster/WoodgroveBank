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
            var siloOptionsBuilder = new SiloNameBuilder();

            // storage
            var tableStorageBuilder = new TableStorageSiloBuilder();

            // endpoints
            var webAppSiloBuilder = new WebAppsVirtualNetworkEndpointsSiloBuilder();
            var configuredEndpointsBuilder = new ConfiguredEndpointsSiloBuilder();

            // monitoring
            var appInsightsBuilder = new AzureApplicationInsightsSiloBuilder();

            // bail out to use localhost
            var localhostBuilder = new LocalhostSiloBuilder();

            // set up the chain of responsibility
            clusterOptionsBuilder.SetNextBuilder(siloOptionsBuilder);
            siloOptionsBuilder.SetNextBuilder(tableStorageBuilder);
            tableStorageBuilder.SetNextBuilder(webAppSiloBuilder);
            webAppSiloBuilder.SetNextBuilder(configuredEndpointsBuilder);
            configuredEndpointsBuilder.SetNextBuilder(appInsightsBuilder);
            appInsightsBuilder.SetNextBuilder(localhostBuilder);

            // build the silo
            clusterOptionsBuilder.Build(siloBuilder, configuration);

            return siloBuilder;
        }
    }
}
