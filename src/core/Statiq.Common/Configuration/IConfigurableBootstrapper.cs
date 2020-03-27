namespace Statiq.Common
{
    public interface IConfigurableBootstrapper : IConfigurable
    {
        /// <summary>
        /// A catalog of all classes in all assemblies loaded by the current application context.
        /// </summary>
        ClassCatalog ClassCatalog { get; }

        /// <summary>
        /// A collection of all configurators to be run on the bootstrapper.
        /// </summary>
        IConfiguratorCollection Configurators { get; }
    }
}
