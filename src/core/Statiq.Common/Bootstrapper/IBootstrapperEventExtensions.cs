namespace Statiq.Common
{
    public static class IBootstrapperEventExtensions
    {
        public static TBootstrapper SubscribeEvent<TBootstrapper, TEvent>(this TBootstrapper bootstrapper, AsyncEventHandler<TEvent> handler)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.ConfigureEngine(x => x.Events.Subscribe(handler));

        public static TBootstrapper SubscribeEvent<TBootstrapper, TEvent>(this TBootstrapper bootstrapper, EventHandler<TEvent> handler)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.ConfigureEngine(x => x.Events.Subscribe(handler));
    }
}
