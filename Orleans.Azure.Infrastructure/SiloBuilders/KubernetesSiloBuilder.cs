namespace Orleans.Hosting
{
    public class KubernetesSiloBuilder : AzureSiloBuilder
    {
        public override void Build(ISiloBuilder siloBuilder, IConfiguration configuration)
        {
            if (!string.IsNullOrEmpty(configuration.GetValue<string>(EnvironmentVariables.KubernetesPodName)) &&
                !string.IsNullOrEmpty(configuration.GetValue<string>(EnvironmentVariables.KubernetesPodNamespace)) &&
                !string.IsNullOrEmpty(configuration.GetValue<string>(EnvironmentVariables.KubernetesPodIPAddress)))
            {
                siloBuilder.UseKubernetesHosting();
            }

            base.Build(siloBuilder, configuration);
        }
    }
}
