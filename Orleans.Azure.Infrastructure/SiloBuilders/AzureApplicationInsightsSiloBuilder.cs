using Microsoft.Extensions.Configuration;

namespace Orleans.Hosting
{
    public class AzureApplicationInsightsSiloBuilder : AzureSiloBuilder
    {
        public override void Build(ISiloBuilder siloBuilder, IConfiguration configuration)
        {
            if (!string.IsNullOrEmpty(configuration.GetValue<string>(EnvironmentVariables.ApplicationInsightsInstrumentationKey)))
            {
                siloBuilder.AddApplicationInsightsTelemetryConsumer(configuration.GetValue<string>(EnvironmentVariables.ApplicationInsightsInstrumentationKey));
            }

            base.Build(siloBuilder, configuration);
        }
    }
}
