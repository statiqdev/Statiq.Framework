namespace Wyam.Common.Configuration
{
    public interface IConfigurableBootstrapper : IConfigurable
    {
        IClassCatalog ClassCatalog { get; }

        IConfiguratorCollection Configurators { get; }
    }
}
