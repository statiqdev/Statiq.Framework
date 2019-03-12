using System;
using System.Collections.Generic;
using System.Text;
using Wyam.Common.Configuration;

namespace Wyam.App.Configuration
{
    public static class ConfiguratorCollectionExtensions
    {
        public static void Configure<T>(this IConfiguratorCollection configuratorCollection, T configurable)
            where T : class
        {
            if (configuratorCollection.TryGet(out IList<IConfigurator<T>> configurators))
            {
                foreach (IConfigurator<T> configurator in configurators)
                {
                    configurator?.Configure(configurable);
                }
            }
        }
    }
}
