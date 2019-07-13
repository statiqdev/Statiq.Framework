namespace Statiq.Common
{
    public interface IConfigurableBootstrapper : IConfigurable
    {
        IClassCatalog ClassCatalog { get; }

        IConfiguratorCollection Configurators { get; }
    }
}
