using System;
using System.Collections.Generic;
using System.Text;
using Wyam.Common.Configuration;

namespace Wyam.App.Configuration
{
    public static class ConfiguratorCollectionExtensions
    {
        public static void Configure<TConfigurable>(this IConfiguratorCollection configuratorCollection, TConfigurable configurable)
            where TConfigurable : class
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
