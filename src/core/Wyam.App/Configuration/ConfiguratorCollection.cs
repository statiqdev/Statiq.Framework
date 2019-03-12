using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Wyam.Common.Configuration;

namespace Wyam.App.Configuration
{
    internal class ConfiguratorCollection : IConfiguratorCollection
    {
        private readonly Dictionary<Type, List<object>> _configurators = new Dictionary<Type, List<object>>();

        public void Add<T, TConfigurator>()
            where T : class
            where TConfigurator : class, IConfigurator<T> =>
            Get<T>().Add(Activator.CreateInstance<TConfigurator>());

        public void Add<T>(Action<T> action)
            where T : class =>
            Add(new DelegateConfigurator<T>(action));

        public void Add<T>(IConfigurator<T> configurator)
            where T : class =>
            Get<T>().Add(configurator);

        public IList<IConfigurator<T>> Get<T>()
            where T : class
        {
            if (!_configurators.TryGetValue(typeof(T), out List<object> list))
            {
                list = new List<object>();
                _configurators.Add(typeof(T), list);
            }
            return new ConfiguratorList<T>(list);
        }

        public bool TryGet<T>(out IList<IConfigurator<T>> configurators)
            where T : class
        {
            if (_configurators.TryGetValue(typeof(T), out List<object> list))
            {
                configurators = new ConfiguratorList<T>(list);
                return true;
            }
            configurators = null;
            return false;
        }
    }
}
