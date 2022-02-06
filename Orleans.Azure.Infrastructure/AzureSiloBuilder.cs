using Microsoft.Extensions.Configuration;

namespace Orleans.Hosting
{
    public abstract class AzureSiloBuilder
    {
        private AzureSiloBuilder? _successor;

        public void SetNextBuilder(AzureSiloBuilder successor)
        {
            _successor = successor;
        }

        public virtual void Build(ISiloBuilder siloBuilder, IConfiguration configuration)
        {
            if(_successor != null)
            {
                _successor.Build(siloBuilder, configuration);
            }
        }
    }
}
