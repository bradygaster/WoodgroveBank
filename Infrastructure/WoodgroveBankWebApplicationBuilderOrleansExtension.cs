namespace Microsoft.AspNetCore.Builder
{
    public static class WoodgroveBankWebApplicationBuilderOrleansExtension
    {
        public static WebApplicationBuilder AddWoodgroveBankSilo(this WebApplicationBuilder builder)
        {
            builder.AddKeyedAzureTableClient("clustering");
            builder.AddKeyedAzureBlobClient("grainState");
            builder.AddKeyedAzureBlobClient("PubSubStore");
            builder.UseOrleans(silo =>
            {
                silo.AddMemoryStreams("BANK");
            });

            return builder;
        }
    }
}