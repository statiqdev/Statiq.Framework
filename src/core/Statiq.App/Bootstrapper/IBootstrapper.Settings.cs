using System;
using System.Collections.Generic;
using Statiq.Common;

namespace Statiq.App
{
    public partial interface IBootstrapper
    {
        public IBootstrapper AddSettings(Action<ISettings> action) =>
            Configure<ISettings>(x => action(x));

        public IBootstrapper AddSettings(IEnumerable<KeyValuePair<string, object>> settings) =>
            Configure<ISettings>(x => x.AddRange(settings));

        public IBootstrapper AddSetting(KeyValuePair<string, object> setting) =>
            Configure<ISettings>(x => x.Add(setting));

        public IBootstrapper AddSetting(string key, object value) =>
            Configure<ISettings>(x => x.Add(key, value));
    }
}
