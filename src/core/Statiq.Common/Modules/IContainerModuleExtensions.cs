using System;
using System.Collections.Generic;

namespace Statiq.Common
{
    public static class IContainerModuleExtensions
    {
        public static T WithChildren<T>(this T container, params IModule[] modules)
            where T : class, IContainerModule =>
            WithChildren(container, (IEnumerable<IModule>)modules);

        public static T WithChildren<T>(this T container, IEnumerable<IModule> modules)
            where T : class, IContainerModule
        {
            _ = container ?? throw new ArgumentNullException(nameof(container));
            container.Children.Add(modules);
            return container;
        }
    }
}
