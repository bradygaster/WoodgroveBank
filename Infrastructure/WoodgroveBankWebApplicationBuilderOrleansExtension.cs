﻿using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;

namespace WoodgroveBank.Infrastructure
{
    public static class WoodgroveBankWebApplicationBuilderOrleansExtension
    {
        public static WebApplicationBuilder AddWoodvilleBankSilo(this WebApplicationBuilder webApplicationBuilder, bool useDashboard = false)
        {
            var storageConnectionString = webApplicationBuilder.Configuration.GetValue<string>(EnvironmentVariables.AzureStorageConnectionString);
            webApplicationBuilder.Host.UseOrleans(siloBuilder => 
            {
                siloBuilder
                    .UseLocalhostClustering();

                siloBuilder
                    .Configure<ClusterMembershipOptions>(options => options.ValidateInitialConnectivity = false);

                siloBuilder
                    .AddAzureTableGrainStorage(name: Strings.OrleansPersistenceNames.CustomersStore, options => options.ConfigureTableServiceClient(storageConnectionString))
                    .AddAzureTableGrainStorage(name: Strings.OrleansPersistenceNames.TransactionsStore, options => options.ConfigureTableServiceClient(storageConnectionString))
                    .AddAzureTableGrainStorage(name: Strings.OrleansPersistenceNames.AccountsStore, options => options.ConfigureTableServiceClient(storageConnectionString))
                    .AddAzureTableGrainStorage(name: Strings.OrleansPersistenceNames.CustomerStore, options => options.ConfigureTableServiceClient(storageConnectionString));

                if(useDashboard)
                {
                    siloBuilder
                        .ConfigureApplicationParts(parts => parts.AddFromApplicationBaseDirectory())
                        .UseDashboard(options => { });
                }
                
            });

            return webApplicationBuilder;
        }
    }
}