namespace Statiq.Common
{
    public class ConfigurableInitialSettings : IConfigurable
    {
        public ConfigurableInitialSettings(ISettings settings)
        {
            Settings = settings;
        }

        public ISettings Settings { get; }
    }
}