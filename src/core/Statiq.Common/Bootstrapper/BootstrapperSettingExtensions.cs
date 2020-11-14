using System.Collections.Generic;

namespace Statiq.Common
{
    public static class BootstrapperSettingExtensions
    {
        public static TBootstrapper AddSettings<TBootstrapper>(this TBootstrapper bootstrapper, IEnumerable<KeyValuePair<string, object>> settings)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.ConfigureSettings(x => x.AddOrReplaceRange(settings));

        public static TBootstrapper AddSetting<TBootstrapper>(this TBootstrapper bootstrapper, KeyValuePair<string, object> setting)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.ConfigureSettings(x => x[setting.Key] = setting.Value);

        public static TBootstrapper AddSetting<TBootstrapper>(this TBootstrapper bootstrapper, string key, object value)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.ConfigureSettings(x => x[key] = value);

        public static TBootstrapper AddSettingsIfNonExisting<TBootstrapper>(this TBootstrapper bootstrapper, IEnumerable<KeyValuePair<string, object>> settings)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.ConfigureSettings(x => x.AddRangeIfNonExisting(settings));

        public static TBootstrapper AddSettingIfNonExisting<TBootstrapper>(this TBootstrapper bootstrapper, KeyValuePair<string, object> setting)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.ConfigureSettings(x => x.AddIfNonExisting(setting.Key, setting.Value));

        public static TBootstrapper AddSettingIfNonExisting<TBootstrapper>(this TBootstrapper bootstrapper, string key, object value)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.ConfigureSettings(x => x.AddIfNonExisting(key, value));
    }
}
