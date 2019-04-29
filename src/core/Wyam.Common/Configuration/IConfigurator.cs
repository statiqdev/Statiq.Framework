namespace Wyam.Common.Configuration
{
    public interface IConfigurator<TConfigurable>
        where TConfigurable : class
    {
        void Configure(TConfigurable configurable);
    }
}
