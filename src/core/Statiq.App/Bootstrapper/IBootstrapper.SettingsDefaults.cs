using System;
using System.Collections.Generic;
using Statiq.Common;

namespace Statiq.App
{
    public partial interface IBootstrapper
    {
        public IBootstrapper AddSettings(IEnumerable<KeyValuePair<string, object>> settings) =>
            ConfigureSettings(x =>
            {
                foreach (KeyValuePair<string, object> setting in settings)
                {
                    x[setting.Key] = setting.Value;
                }
            });

        public IBootstrapper AddSetting(KeyValuePair<string, object> setting) =>
            ConfigureSettings(x => x[setting.Key] = setting.Value);

        public IBootstrapper AddSetting(string key, object value) =>
            ConfigureSettings(x => x[key] = value);
    }
}
