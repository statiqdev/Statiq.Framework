using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Statiq.Common
{
    /// <summary>
    /// Contains global events and event handlers.
    /// </summary>
    public interface IReadOnlyEventCollection
    {
        /// <summary>
        /// Raises all handlers for a registered event type
        /// (as indicated by the <typeparamref name="TEventArgs"/> type).
        /// </summary>
        /// <typeparam name="TEventArgs">The type of event arguments to raise an event for.</typeparam>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="args">The event arguments.</param>
        /// <returns><c>true</c> if registered handlers were found, <c>false</c> otherwise.</returns>
        Task<bool> RaiseAsync<TEventArgs>(object sender, TEventArgs args);
    }
}
