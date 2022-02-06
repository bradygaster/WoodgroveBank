using Microsoft.Extensions.Configuration;
using Orleans.Configuration;
using System.Net;

namespace Orleans.Hosting
{
    public class ConfiguredEndpointsSiloBuilder : AzureSiloBuilder
    {
        public override void Build(ISiloBuilder siloBuilder, IConfiguration configuration)
        {
            /*
                The ConfiguredEndpointsBuilder's functionality will be
                automatically deactivated if this silo is running in 
                an Azure Web Apps S1 or greater with a regional vnet.
            
                In that scenario, the WebAppsVirtualNetworkEndpointBuilder 
                configures the silo's endpoints.
            */ 

            if (string.IsNullOrEmpty(configuration.GetValue<string>(EnvironmentVariables.WebAppsPrivateIPAddress)) &&
                string.IsNullOrEmpty(configuration.GetValue<string>(EnvironmentVariables.WebAppsPrivatePorts)))
            {
                int siloPort = Defaults.SiloPort;
                int gatewayPort = Defaults.GatewayPort;

                if (!string.IsNullOrEmpty(configuration.GetValue<string>(EnvironmentVariables.OrleansSiloPort)) &&
                    !string.IsNullOrEmpty(configuration.GetValue<string>(EnvironmentVariables.OrleansGatewayPort)))
                {
                    siloPort = configuration.GetValue<int>(EnvironmentVariables.OrleansSiloPort);
                    gatewayPort = configuration.GetValue<int>(EnvironmentVariables.OrleansGatewayPort);
                }

                siloBuilder.Configure<EndpointOptions>(options =>
                {
                    options.SiloPort = siloPort;
                    options.GatewayPort = gatewayPort;

                    var siloHostEntry = Dns.GetHostEntry(Environment.MachineName);
                    options.AdvertisedIPAddress = siloHostEntry.AddressList[0];
                });
            }

            base.Build(siloBuilder, configuration);
        }
    }
}
