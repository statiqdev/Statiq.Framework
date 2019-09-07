using System;
using System.Collections.Generic;

namespace Statiq.Common
{
    public partial interface IConfiguratorCollection
    {
        public void Add<TConfigurable, TConfigurator>()
            where TConfigurable : IConfigurable
            where TConfigurator : IConfigurator<TConfigurable> =>
            Get<TConfigurable>().Add(Activator.CreateInstance<TConfigurator>());

        public void Add<TConfigurable>(Action<TConfigurable> action)
            where TConfigurable : IConfigurable =>
            Add(new DelegateConfigurator<TConfigurable>(action));

        public void Add<TConfigurable>(IConfigurator<TConfigurable> configurator)
            where TConfigurable : IConfigurable =>
            Get<TConfigurable>().Add(configurator);

        /// <summary>
        /// Applies all applicable configurators in the collection to the provided configurable.
        /// </summary>
        /// <typeparam name="TConfigurable">The type of configurable object.</typeparam>
        /// <param name="configurable">The object to configure.</param>
        public void Configure<TConfigurable>(TConfigurable configurable)
            where TConfigurable : IConfigurable
        {
            if (TryGet(out IList<IConfigurator<TConfigurable>> configurators))
            {
                foreach (IConfigurator<TConfigurable> configurator in configurators)
                {
                    configurator?.Configure(configurable);
                }
            }
        }
    }
}
