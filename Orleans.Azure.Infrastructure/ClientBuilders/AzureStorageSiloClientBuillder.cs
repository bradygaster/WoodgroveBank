using Microsoft.Extensions.Configuration;

namespace Orleans.Hosting
{
    internal class AzureStorageSiloClientBuillder : AzureSiloClientBuilder
    {
        public override void Build(IClientBuilder clientBuilder, IConfiguration configuration)
        {
            if (!string.IsNullOrEmpty(configuration.GetValue<string>(EnvironmentVariables.AzureStorageConnectionString)))
            {
                clientBuilder.UseAzureStorageClustering(options => options.ConfigureTableServiceClient(configuration.GetValue<string>(EnvironmentVariables.AzureStorageConnectionString)));
            }

            base.Build(clientBuilder, configuration);
        }
    }
}
