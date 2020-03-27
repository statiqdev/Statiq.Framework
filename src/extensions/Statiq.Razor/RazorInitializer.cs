using Statiq.Common;

namespace Statiq.Razor
{
    public class RazorInitializer : IInitializer
    {
        public void Configure(IConfigurableBootstrapper configurable) =>
            configurable.Configurators.Add<ConfigurableServices>(x => x.Services.AddRazor(null));
    }
}