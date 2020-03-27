namespace Statiq.Common
{
    /// <summary>
    /// Implement this interface to define an initializer that will get automatically
    /// instantiated and run by the bootstrapper at startup.
    /// </summary>
    public interface IInitializer : IConfigurator<IConfigurableBootstrapper>
    {
    }
}
