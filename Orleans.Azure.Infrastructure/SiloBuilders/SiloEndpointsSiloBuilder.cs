﻿using Microsoft.Extensions.Configuration;
using Orleans.Configuration;
using System.Net;

namespace Orleans.Hosting
{
    public class SiloEndpointsSiloBuilder : AzureSiloBuilder
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

                    if(!IsLocalIpAddress(Environment.MachineName))
                    {
                        var siloHostEntry = Dns.GetHostEntry(Environment.MachineName);
                        options.AdvertisedIPAddress = siloHostEntry.AddressList[0];
                    }
                    else
                    {
                        options.AdvertisedIPAddress = IPAddress.Loopback;
                    }
                });
            }

            base.Build(siloBuilder, configuration);
        }

        public static bool IsLocalIpAddress(string host)
        {
            try
            {
                IPAddress[] hostIPs = Dns.GetHostAddresses(host);
                IPAddress[] localIPs = Dns.GetHostAddresses(Dns.GetHostName());

                foreach (IPAddress hostIP in hostIPs)
                {
                    if (IPAddress.IsLoopback(hostIP)) return true;
                    foreach (IPAddress localIP in localIPs)
                    {
                        if (hostIP.Equals(localIP)) return true;
                    }
                }
            }
            catch { }
            return false;
        }
    }
}