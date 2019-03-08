namespace Wyam.App.Configuration
{
    public interface IConfigurator<T>
        where T : class
    {
        void Configure(T item);
    }
}
