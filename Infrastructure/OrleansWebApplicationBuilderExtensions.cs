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
                if(siloBuilderCallback is not null)
                {
                    siloBuilderCallback(silo);
                }
            });

            return builder;
        }

        public static WebApplicationBuilder AsOrleansClient(this WebApplicationBuilder builder)
        {
            builder.AddKeyedAzureTableClient("clustering");
            builder.UseOrleansClient();

            return builder;
        }
    }
}