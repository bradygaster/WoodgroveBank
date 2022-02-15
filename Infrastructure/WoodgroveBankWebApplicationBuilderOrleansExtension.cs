using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;

namespace WoodgroveBank.Infrastructure
{
    public static class WoodgroveBankWebApplicationBuilderOrleansExtension
    {
        public static WebApplicationBuilder AddWoodvilleBankSilo(this WebApplicationBuilder webApplicationBuilder, bool useDashboard = false)
        {
            var storageConnectionString = webApplicationBuilder.Configuration.GetValue<string>(EnvironmentVariables.AzureStorageConnectionString);
            webApplicationBuilder.AddOrleansSilo(siloBuilder =>
            {
                siloBuilder
                    .AddAzureTableGrainStorage(name: Strings.OrleansPersistenceNames.AccountTransactionsStore, options => options.ConfigureTableServiceClient(storageConnectionString))
                    .AddAzureTableGrainStorage(name: Strings.OrleansPersistenceNames.AccountsStore, options => options.ConfigureTableServiceClient(storageConnectionString))
                    .AddAzureTableGrainStorage(name: Strings.OrleansPersistenceNames.AccountStore, options => options.ConfigureTableServiceClient(storageConnectionString))
                    .AddAzureTableGrainStorage(name: Strings.OrleansPersistenceNames.CustomerStore, options => options.ConfigureTableServiceClient(storageConnectionString))
                    .AddAzureTableGrainStorage(name: Strings.OrleansPersistenceNames.CustomerAccountsStore, options => options.ConfigureTableServiceClient(storageConnectionString))
                    .AddAzureTableGrainStorage(name: Strings.OrleansPersistenceNames.CustomersStore, options => options.ConfigureTableServiceClient(storageConnectionString))
                    .AddAzureTableGrainStorage(name: Strings.OrleansPersistenceNames.TransactionsStore, options => options.ConfigureTableServiceClient(storageConnectionString))
                    ;

                if(useDashboard)
                {
                    siloBuilder
                        .ConfigureApplicationParts(parts => parts.AddFromApplicationBaseDirectory())
                        .UseDashboard(options => {
                            options.HostSelf = true;
                            options.BasePath = "/dashboard";
                        });
                }
                
            });

            return webApplicationBuilder;
        }
    }
}