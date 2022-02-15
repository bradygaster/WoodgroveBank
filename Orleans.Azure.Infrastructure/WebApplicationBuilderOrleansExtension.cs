using Orleans.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Hosting
{
    public static class WebApplicationBuilderOrleansExtension
    {
        public static WebApplicationBuilder AddOrleansSilo(this WebApplicationBuilder webApplicationBuilder, Action<ISiloBuilder>? siloBuilderHook = null)
        {
            webApplicationBuilder.Host.UseOrleans(siloBuilder =>
            {
                siloBuilder.HostSiloInAzure(webApplicationBuilder.Configuration);
                if(siloBuilderHook != null)
                {
                    siloBuilderHook(siloBuilder);
                }
            });

            return webApplicationBuilder;
        }
    }
}
