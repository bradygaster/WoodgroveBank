using Microsoft.Extensions.Configuration;
using System.Net;

namespace Orleans.Hosting
{
    public class AzureAppServiceSiloBuilder : AzureSiloBuilder
    {
        public override void Build(ISiloBuilder siloBuilder, IConfiguration configuration)
        {
            // make sure they haven't wired themselves up to work in K8S
            if (string.IsNullOrEmpty(configuration.GetValue<string>(EnvironmentVariables.KubernetesPodName)) &&
                string.IsNullOrEmpty(configuration.GetValue<string>(EnvironmentVariables.KubernetesPodNamespace)) &&
                string.IsNullOrEmpty(configuration.GetValue<string>(EnvironmentVariables.KubernetesPodIPAddress)))
            {
                // are the app service-injected environment variables we need there?
                if (configuration.GetValue<string>(EnvironmentVariables.WebAppsPrivateIPAddress) != null &&
                    configuration.GetValue<string>(EnvironmentVariables.WebAppsPrivatePorts) != null)
                {
                    // presume the app is running in Web Apps on App Service and start up
                    IPAddress endpointAddress = IPAddress.Parse(configuration.GetValue<string>(EnvironmentVariables.WebAppsPrivateIPAddress));

                    var strPorts = configuration.GetValue<string>(EnvironmentVariables.WebAppsPrivatePorts).Split(',');

                    if (strPorts.Length < 2) throw new Exception("Insufficient private ports configured.");

                    int siloPort = int.Parse(strPorts[0]);
                    int gatewayPort = int.Parse(strPorts[1]);

                    siloBuilder.ConfigureEndpoints(endpointAddress, siloPort, gatewayPort);
                }
            }

            base.Build(siloBuilder, configuration);
        }
    }
}
