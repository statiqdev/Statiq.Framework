using System;
using System.Collections.Generic;
using Statiq.Common;

namespace Statiq.App
{
    public partial interface IBootstrapper
    {
        public IBootstrapper AddSettings(IEnumerable<KeyValuePair<string, string>> settings) =>
            ConfigureSettings(x => x.AddOrReplaceRange(settings));

        public IBootstrapper AddSetting(KeyValuePair<string, string> setting) =>
            ConfigureSettings(x => x[setting.Key] = setting.Value);

        public IBootstrapper AddSetting(string key, string value) =>
            ConfigureSettings(x => x[key] = value);
    }
}
