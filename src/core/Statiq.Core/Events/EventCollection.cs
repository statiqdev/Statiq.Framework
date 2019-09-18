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
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Statiq.Common;

namespace Statiq.Core
{
    internal class EventCollection : IEventCollection
    {
        // Stores lists of AsyncEventHandler<TEventArgs> keyed by TEventArgs
        // Don't use a single Delegate with Delegate.Combine() because it
        // results in each subsequent delegate in the chain invoking all previous ones
        private readonly ConcurrentDictionary<Type, List<Delegate>> _events =
            new ConcurrentDictionary<Type, List<Delegate>>();

        /// <inheritdoc />
        public void Subscribe<TEvent>(AsyncEventHandler<TEvent> handler) =>
            _events.AddOrUpdate(
                typeof(TEvent),
                _ => new List<Delegate> { handler ?? throw new ArgumentNullException(nameof(handler)) },
                (_, handlers) =>
                {
                    handlers.Add(handler ?? throw new ArgumentNullException(nameof(handler)));
                    return handlers;
                });

        /// <inheritdoc />
        public async Task<bool> RaiseAsync<TEvent>(TEvent evt)
        {
            _ = evt ?? throw new ArgumentNullException(nameof(evt));
            if (_events.TryGetValue(typeof(TEvent), out List<Delegate> handlers))
            {
                foreach (Delegate handler in handlers)
                {
                    await ((AsyncEventHandler<TEvent>)handler)(evt);
                }
                return true;
            }
            return false;
        }
    }
}
