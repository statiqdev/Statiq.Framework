using System;
using System.Collections.Generic;

namespace Wyam.Common.Configuration
{
    public interface IConfiguratorCollection
    {
        void Add<TConfigurable, TConfigurator>()
            where TConfigurable : IConfigurable
            where TConfigurator : IConfigurator<TConfigurable>;

        void Add<TConfigurable>(Action<TConfigurable> action)
            where TConfigurable : IConfigurable;

        void Add<TConfigurable>(IConfigurator<TConfigurable> configurator)
            where TConfigurable : IConfigurable;

        IList<IConfigurator<TConfigurable>> Get<TConfigurable>()
            where TConfigurable : IConfigurable;

        bool TryGet<TConfigurable>(out IList<IConfigurator<TConfigurable>> configurators)
            where TConfigurable : IConfigurable;
    }
}
