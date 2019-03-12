namespace Wyam.Common.Configuration
{
    public interface IConfigurator<T>
        where T : class
    {
        void Configure(T configurable);
    }
}
