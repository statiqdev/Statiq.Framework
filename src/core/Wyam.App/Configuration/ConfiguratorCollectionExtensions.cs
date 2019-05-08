using System.Collections.Generic;
using Wyam.Common.Configuration;

namespace Wyam.App.Configuration
{
    public static class ConfiguratorCollectionExtensions
    {
        public static void Configure<TConfigurable>(this IConfiguratorCollection configuratorCollection, TConfigurable configurable)
            where TConfigurable : IConfigurable
        {
            if (configuratorCollection.TryGet(out IList<IConfigurator<TConfigurable>> configurators))
            {
                foreach (IConfigurator<TConfigurable> configurator in configurators)
                {
                    configurator?.Configure(configurable);
                }
            }
        }
    }
}
