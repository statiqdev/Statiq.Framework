using System;
using System.Collections.Generic;
using Statiq.Common;

namespace Statiq.App
{
    public partial interface IBootstrapper
    {
        public IBootstrapper SubscribeEvent<TEventArgs>(AsyncEventHandler<TEventArgs> handler) =>
            Configure<IEngine>(x => x.Events.Subscribe(handler));

        public IBootstrapper SubscribeEvent<TEventArgs>(Common.EventHandler<TEventArgs> handler) =>
            Configure<IEngine>(x => x.Events.Subscribe(handler));
    }
}
