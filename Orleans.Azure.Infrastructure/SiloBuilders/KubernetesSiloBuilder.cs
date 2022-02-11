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
                // todo: add wire-up specific to the kubernetes hosting package's expectations
                siloBuilder.UseKubernetesHosting();
            }

            base.Build(siloBuilder, configuration);
        }
    }
}
