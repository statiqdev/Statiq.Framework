using Statiq.Common;

namespace Statiq.Razor
{
    public class RazorInitializer : IInitializer
    {
        public void Configure(IBootstrapper bootstrapper) =>
            bootstrapper.ConfigureServices(x => x.AddRazor(null));
    }
}