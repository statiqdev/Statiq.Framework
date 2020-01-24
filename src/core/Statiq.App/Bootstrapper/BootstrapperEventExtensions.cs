using System;
using System.Collections.Generic;
using Statiq.Common;

namespace Statiq.App
{
    public static class BootstrapperEventExtensions
    {
        public static Bootstrapper SubscribeEvent<TEvent>(this Bootstrapper bootstrapper, AsyncEventHandler<TEvent> handler) =>
            bootstrapper.ConfigureEngine(x => x.Events.Subscribe(handler));

        public static Bootstrapper SubscribeEvent<TEvent>(this Bootstrapper bootstrapper, Common.EventHandler<TEvent> handler) =>
            bootstrapper.ConfigureEngine(x => x.Events.Subscribe(handler));
    }
}
