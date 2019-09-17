using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Statiq.Common;

namespace Statiq.Testing
{
    public class TestEventCollection : IEventCollection
    {
        // Stores lists of AsyncEventHandler<TEventArgs> keyed by TEventArgs
        // Don't use a single Delegate with Delegate.Combine() because it
        // results in each subsequent delegate in the chain invoking all previous ones
        private readonly ConcurrentDictionary<Type, List<Delegate>> _events =
            new ConcurrentDictionary<Type, List<Delegate>>();

        /// <inheritdoc />
        public void Subscribe<TEventArgs>(AsyncEventHandler<TEventArgs> handler) =>
            _events.AddOrUpdate(
                typeof(TEventArgs),
                _ => new List<Delegate> { handler ?? throw new ArgumentNullException(nameof(handler)) },
                (_, handlers) =>
                {
                    handlers.Add(handler ?? throw new ArgumentNullException(nameof(handler)));
                    return handlers;
                });

        /// <inheritdoc />
        public async Task<bool> RaiseAsync<TArgs>(object sender, TArgs args)
        {
            _ = args ?? throw new ArgumentNullException(nameof(args));
            if (_events.TryGetValue(typeof(TArgs), out List<Delegate> handlers))
            {
                foreach (Delegate handler in handlers)
                {
                    await ((AsyncEventHandler<TArgs>)handler)(sender, args);
                }
                return true;
            }
            return false;
        }
    }
}
