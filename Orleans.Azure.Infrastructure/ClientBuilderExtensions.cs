using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Orleans.Hosting
{
    public static class ClientBuilderExtensions
    {
        public static IServiceCollection AddOrleansClusterClient(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<OrleansClusterClientHostedService>();
            services.AddHostedService<OrleansClusterClientHostedService>(svc => svc.GetRequiredService<OrleansClusterClientHostedService>());
            services.AddSingleton<IHostedService>(svc => svc.GetRequiredService<OrleansClusterClientHostedService>());
            services.AddSingleton<IClusterClient>(svc => svc.GetRequiredService<OrleansClusterClientHostedService>().Client);

            return services;
        }
    }
}
