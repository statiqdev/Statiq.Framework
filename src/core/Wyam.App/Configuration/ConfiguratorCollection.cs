using System;
using System.Collections.Generic;

namespace Wyam.App.Configuration
{
    public class ConfiguratorCollection<T> : List<IConfigurator<T>>
        where T : class
    {
        public void Add<TConfigurator>()
            where TConfigurator : class, IConfigurator<T> =>
            Add(Activator.CreateInstance<TConfigurator>());

        public void Add(Action<T> action) =>
            Add(new DelegateConfigurator<T>(action));

        internal void Configure(T item)
        {
            foreach (IConfigurator<T> configurator in this)
            {
                configurator?.Configure(item);
            }
        }
    }
}
