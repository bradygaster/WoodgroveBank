using Microsoft.Extensions.Configuration;
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

            if (!IsRunningOnAzureAppService(configuration) && !IsRunningOnKubernetes(configuration))
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

                    if (!IsLocalIpAddress(Environment.MachineName))
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

            /// <summary>
            /// Reports back if the silo is running on Azure App Service, in a regional vnet-
            /// configured environment with multiple private ports.
            /// </summary>
            static bool IsRunningOnAzureAppService(IConfiguration configuration)
            {
                return !string.IsNullOrEmpty(configuration.GetValue<string>(EnvironmentVariables.WebAppsPrivateIPAddress)) &&
                       !string.IsNullOrEmpty(configuration.GetValue<string>(EnvironmentVariables.WebAppsPrivatePorts));
            }

            ///<summary>
            /// Reports back if the silo is running on Kubernetes.
            /// </summary>
            static bool IsRunningOnKubernetes(IConfiguration configuration)
            {
                return !string.IsNullOrEmpty(configuration.GetValue<string>(EnvironmentVariables.KubernetesPodName)) &&
                       !string.IsNullOrEmpty(configuration.GetValue<string>(EnvironmentVariables.KubernetesPodNamespace)) &&
                       !string.IsNullOrEmpty(configuration.GetValue<string>(EnvironmentVariables.KubernetesPodIPAddress));
            }
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
