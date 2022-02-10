using Microsoft.Extensions.Configuration;

namespace Orleans.Hosting
{
    public class AzureTableStorageClusteringSiloBuilder : AzureSiloBuilder
    {
        public override void Build(ISiloBuilder siloBuilder, IConfiguration configuration)
        {
            var azureStorageConnectionString = string.Empty;

            if (!string.IsNullOrEmpty(configuration.GetValue<string>(EnvironmentVariables.AzureStorageConnectionString)))
            {
                azureStorageConnectionString = configuration.GetValue<string>(EnvironmentVariables.AzureStorageConnectionString);
            }

            if(!string.IsNullOrEmpty(azureStorageConnectionString))
            {
                siloBuilder
                    .UseAzureStorageClustering(storageOptions => storageOptions.ConfigureTableServiceClient(azureStorageConnectionString))
                    .AddAzureTableGrainStorageAsDefault(tableStorageOptions => 
                     {
                        tableStorageOptions.ConfigureTableServiceClient(azureStorageConnectionString);
                        tableStorageOptions.UseJson = true;
                    });
            }

            base.Build(siloBuilder, configuration);
        }
    }
}
