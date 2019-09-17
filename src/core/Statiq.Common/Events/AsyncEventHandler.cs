using System;
using System.Threading.Tasks;

namespace Statiq.Common
{
    /// <summary>
    /// An asynchronous event handler.
    /// </summary>
    /// <typeparam name="TEventArgs">The type of <see cref="EventArgs"/></typeparam>
    /// <param name="sender">The sender of the event.</param>
    /// <param name="args">Event arguments.</param>
    /// <returns>A task whose completion signals handling is finished.</returns>
    public delegate Task AsyncEventHandler<TEventArgs>(object sender, TEventArgs args);
}
