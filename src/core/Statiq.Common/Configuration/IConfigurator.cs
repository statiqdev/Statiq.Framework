namespace Statiq.Common.Configuration
{
    public interface IConfigurator<TConfigurable>
        where TConfigurable : IConfigurable
    {
        void Configure(TConfigurable configurable);
    }
}
