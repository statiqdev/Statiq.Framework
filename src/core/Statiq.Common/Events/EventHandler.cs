using System;
using System.Threading.Tasks;

namespace Statiq.Common
{
    /// <summary>
    /// An asynchronous event handler.
    /// </summary>
    /// <typeparam name="TEventArgs">The type of <see cref="EventArgs"/></typeparam>
    /// <param name="args">Event arguments.</param>
    public delegate void EventHandler<TEventArgs>(TEventArgs args);
}
