using Orleans.Configuration;

namespace Microsoft.AspNetCore.Builder
{
    public static class OrleansWebApplicationBuilderExtensions
    {
        public static WebApplicationBuilder AsOrleansSilo(this WebApplicationBuilder builder, 
            Action<ISiloBuilder>? siloBuilderCallback = null)
        {
            builder.AddKeyedAzureTableClient("clustering");
            builder.AddKeyedAzureBlobClient("grainState");
            builder.UseOrleans(silo =>
            {
                silo.Configure<ClusterMembershipOptions>(o =>
                {
                    o.IAmAliveTablePublishTimeout = TimeSpan.FromSeconds(30);
                    o.NumMissedTableIAmAliveLimit = 4;
                });

                if (siloBuilderCallback is not null)
                {
                    siloBuilderCallback(silo);
                }
            });

            return builder;
        }

        public static WebApplicationBuilder AsOrleansClient(this WebApplicationBuilder builder)
        {
            builder.AddKeyedAzureTableClient("clustering");
            builder.UseOrleansClient(client =>
            {
                client.Configure<GatewayOptions>(o =>
                {
                    o.GatewayListRefreshPeriod = TimeSpan.FromSeconds(30);
                });
            });

            return builder;
        }
    }
}