using System.Collections.Generic;
using Wyam.Common.Configuration;
using Wyam.Common.Execution;

namespace Wyam.Common.Configuration
{
    public interface IConfigurableBootstrapper : IConfigurable
    {
        IClassCatalog ClassCatalog { get; }

        IConfiguratorCollection Configurators { get; }
    }
}
