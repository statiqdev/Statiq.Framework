using System;
using System.Collections.Generic;
using Statiq.Common;

namespace Statiq.App
{
    public static class BootstrapperSettingExtensions
    {
        public static Bootstrapper AddSettings(this Bootstrapper bootstrapper, IEnumerable<KeyValuePair<string, object>> settings) =>
            bootstrapper.ConfigureSettings(x => x.AddOrReplaceRange(settings));

        public static Bootstrapper AddSetting(this Bootstrapper bootstrapper, KeyValuePair<string, object> setting) =>
            bootstrapper.ConfigureSettings(x => x[setting.Key] = setting.Value);

        public static Bootstrapper AddSetting(this Bootstrapper bootstrapper, string key, object value) =>
            bootstrapper.ConfigureSettings(x => x[key] = value);

        public static Bootstrapper AddSettingsIfNonExisting(this Bootstrapper bootstrapper, IEnumerable<KeyValuePair<string, object>> settings) =>
            bootstrapper.ConfigureSettings(x => x.AddRangeIfNonExisting(settings));

        public static Bootstrapper AddSettingIfNonExisting(this Bootstrapper bootstrapper, KeyValuePair<string, object> setting) =>
            bootstrapper.ConfigureSettings(x => x.AddIfNonExisting(setting.Key, setting.Value));

        public static Bootstrapper AddSettingIfNonExisting(this Bootstrapper bootstrapper, string key, object value) =>
            bootstrapper.ConfigureSettings(x => x.AddIfNonExisting(key, value));
    }
}
