using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using JavaScriptEngineSwitcher.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Statiq.Common;

namespace Statiq.Core
{
    // Wraps the actual service providers and provides some engine-specific services
    internal class EngineServiceProvider : IServiceProvider
    {
        private static readonly Dictionary<Type, Func<Engine, object>> ServiceMapping =
            new Dictionary<Type, Func<Engine, object>>
            {
                { typeof(IReadOnlyFileSystem), x => x.FileSystem },
                { typeof(IReadOnlySettings), x => x.Settings },
                { typeof(IReadOnlyShortcodeCollection), x => x.Shortcodes },
                { typeof(IMemoryStreamFactory), x => x.MemoryStreamFactory },
                { typeof(INamespacesCollection), x => x.Namespaces },
                { typeof(IRawAssemblyCollection), x => x.DynamicAssemblies }
            };

        private readonly Engine _engine;
        private readonly IServiceProvider _services;

        public EngineServiceProvider(Engine engine, IServiceProvider services)
        {
            _engine = engine ?? throw new ArgumentNullException();
            _services = services ?? throw new ArgumentNullException();
        }

        public object GetService(Type serviceType) =>
            ServiceMapping.TryGetValue(serviceType, out Func<Engine, object> func)
                ? func(_engine)
                : _services.GetService(serviceType);
    }
}
