using System;

namespace Statiq.Common
{
    /// <summary>
    /// Contains global events and event handlers.
    /// </summary>
    public partial interface IEventCollection : IReadOnlyEventCollection
    {
        /// <summary>
        /// Subscribes a new handler for the given <typeparamref name="TEventArgs"/> type.
        /// </summary>
        /// <typeparam name="TEventArgs">The type of event arguments to subscribe and handler for.</typeparam>
        /// <param name="handler">The handler to subscribe to the event.</param>
        void Subscribe<TEventArgs>(AsyncEventHandler<TEventArgs> handler);
    }
}
