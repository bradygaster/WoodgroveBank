using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orleans.Hosting
{
    public abstract class AzureSiloClientBuilder
    {
        private AzureSiloClientBuilder? _successor;

        public void SetNextBuilder(AzureSiloClientBuilder successor)
        {
            _successor = successor;
        }

        public virtual void Build(IClientBuilder clientBuilder, IConfiguration configuration)
        {
            if (_successor != null)
            {
                _successor.Build(clientBuilder, configuration);
            }
        }
    }
}
