﻿using System;
using System.Collections.Generic;
using Wyam.Common;
using Wyam.Common.Configuration;

namespace Wyam.App
{
    public static class BootstrapperSettingsExtensions
    {
        public static IBootstrapper AddSettings(
            this IBootstrapper bootstrapper,
            Action<ISettings> action) =>
            bootstrapper.Configure<ISettings>(x => action(x));

        public static IBootstrapper AddSettings(
            this IBootstrapper bootstrapper,
            IEnumerable<KeyValuePair<string, object>> settings) =>
            bootstrapper.Configure<ISettings>(x => x.AddRange(settings));

        public static IBootstrapper AddSetting(
            this IBootstrapper bootstrapper,
            KeyValuePair<string, object> setting) =>
            bootstrapper.Configure<ISettings>(x => x.Add(setting));

        public static IBootstrapper AddSetting(
            this IBootstrapper bootstrapper,
            string key,
            object value) =>
            bootstrapper.Configure<ISettings>(x => x.Add(key, value));
    }
}
