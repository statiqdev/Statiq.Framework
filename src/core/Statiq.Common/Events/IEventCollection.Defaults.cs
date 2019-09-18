using System;
using System.Threading.Tasks;

namespace Statiq.Common
{
    public partial interface IEventCollection : IReadOnlyEventCollection
    {
        /// <summary>
        /// Subscribes a new handler for the given <typeparamref name="TEvent"/> type.
        /// </summary>
        /// <typeparam name="TEvent">The type of event to subscribe and handler for.</typeparam>
        /// <param name="handler">The handler to subscribe to the event.</param>
        public void Subscribe<TEvent>(EventHandler<TEvent> handler)
        {
            _ = handler ?? throw new ArgumentNullException(nameof(handler));
            Subscribe((AsyncEventHandler<TEvent>)(args =>
            {
                handler(args);
                return Task.CompletedTask;
            }));
        }
    }
}
