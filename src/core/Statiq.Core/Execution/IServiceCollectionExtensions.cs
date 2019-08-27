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
    public static class IServiceCollectionExtensions
    {
        /// <summary>
        /// Adds required engine services to the service collection.
        /// </summary>
        /// <remarks>
        /// This method uses <c>.TryAdd...</c> methods so existing services will not be replaced.
        /// </remarks>
        /// <param name="serviceCollection">The service collection.</param>
        /// <returns>The service collection with required engine services.</returns>
        public static IServiceCollection AddRequiredEngineServices(this IServiceCollection serviceCollection) =>
            serviceCollection
                .AddLogging();
    }
}
