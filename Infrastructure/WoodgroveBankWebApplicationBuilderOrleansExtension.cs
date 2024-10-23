namespace Microsoft.AspNetCore.Builder
{
    public static class WoodgroveBankWebApplicationBuilderOrleansExtension
    {
        public static WebApplicationBuilder AddWoodgroveBankSilo(this WebApplicationBuilder builder, 
            Action<ISiloBuilder>? siloBuilderCallback = null)
        {
            builder.AddKeyedAzureTableClient("clustering");
            builder.AddKeyedAzureBlobClient("grainState");
            builder.AddKeyedAzureBlobClient("PubSubStore");
            builder.UseOrleans(silo =>
            {
                if(siloBuilderCallback is not null)
                {
                    siloBuilderCallback(silo);
                }
            });

            return builder;
        }
    }
}