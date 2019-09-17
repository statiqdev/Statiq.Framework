using System;
using System.Threading.Tasks;

namespace Statiq.Common
{
    public partial interface IEventCollection : IReadOnlyEventCollection
    {
        /// <summary>
        /// Subscribes a new handler for the given <typeparamref name="TEventArgs"/> type.
        /// </summary>
        /// <typeparam name="TEventArgs">The type of event arguments to subscribe and handler for.</typeparam>
        /// <param name="handler">The handler to subscribe to the event.</param>
        public void Subscribe<TEventArgs>(EventHandler<TEventArgs> handler)
        {
            _ = handler ?? throw new ArgumentNullException(nameof(handler));
            Subscribe((AsyncEventHandler<TEventArgs>)(args =>
            {
                handler(args);
                return Task.CompletedTask;
            }));
        }
    }
}
