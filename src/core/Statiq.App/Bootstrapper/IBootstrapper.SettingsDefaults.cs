using System;
using System.Collections.Generic;
using Statiq.Common;

namespace Statiq.App
{
    public partial interface IBootstrapper
    {
        public IBootstrapper AddSettings(IEnumerable<KeyValuePair<string, object>> settings) =>
            ConfigureSettings(x => x.AddRange(settings));

        public IBootstrapper AddSetting(KeyValuePair<string, object> setting) =>
            ConfigureSettings(x => x.Add(setting));

        public IBootstrapper AddSetting(string key, object value) =>
            ConfigureSettings(x => x.Add(key, value));
    }
}
