using System;
using System.Collections.Generic;

namespace Wyam.Common.Configuration
{
    public interface IConfiguratorCollection
    {
        void Add<T, TConfigurator>()
            where T : class
            where TConfigurator : class, IConfigurator<T>;

        void Add<T>(Action<T> action)
            where T : class;

        void Add<T>(IConfigurator<T> configurator)
            where T : class;

        IList<IConfigurator<T>> Get<T>()
            where T : class;

        bool TryGet<T>(out IList<IConfigurator<T>> configurators)
            where T : class;
    }
}
