using Orleans;
using Orleans.Hosting;

namespace WoodgroveBank.Infrastructure
{
    public static class WoodgroveBankWebApplicationBuilderOrleansExtension
    {
        public static WebApplicationBuilder AddWoodvilleBankSilo(this WebApplicationBuilder webApplicationBuilder)
        {
            var storageConnectionString = webApplicationBuilder.Configuration.GetValue<string>(EnvironmentVariables.AzureStorageConnectionString);
            webApplicationBuilder.AddOrleansSilo(siloBuilder =>
            {
                siloBuilder
                .AddAzureTableGrainStorage(name: Strings.OrleansPersistenceNames.CustomersStore, options => options.ConfigureTableServiceClient(storageConnectionString))
                .AddAzureTableGrainStorage(name: Strings.OrleansPersistenceNames.TransactionsStore, options => options.ConfigureTableServiceClient(storageConnectionString))
                .AddAzureTableGrainStorage(name: Strings.OrleansPersistenceNames.AccountsStore, options => options.ConfigureTableServiceClient(storageConnectionString))
                .ConfigureApplicationParts(parts => parts.AddFromApplicationBaseDirectory())
                .UseDashboard(options => { });
            });

            return webApplicationBuilder;
        }
    }
}