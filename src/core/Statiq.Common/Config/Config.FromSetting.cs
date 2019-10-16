using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Statiq.Common
{
    public static partial class Config
    {
        /// <summary>
        /// Creates a config value by getting the metadata value from the execution context of a specified key.
        /// </summary>
        /// <typeparam name="TValue">The type of config value.</typeparam>
        /// <param name="key">The metadata key to get the value from.</param>
        /// <returns>A config object.</returns>
        public static Config<TValue> FromSetting<TValue>(string key) =>
            new Config<TValue>((_, ctx) => Task.FromResult(ctx.Settings.Get<TValue>(key)), false);

        /// <summary>
        /// Creates a config value by getting the metadata value from the execution context of a specified key.
        /// </summary>
        /// <typeparam name="TValue">The type of config value.</typeparam>
        /// <param name="key">The metadata key to get the value from.</param>
        /// <param name="defaultValue">The default value to use if the key cannot be found, is null, or cannot be converted to <typeparamref name="TValue"/>.</param>
        /// <returns>A config object.</returns>
        public static Config<TValue> FromSetting<TValue>(string key, TValue defaultValue) =>
            new Config<TValue>((_, ctx) => Task.FromResult(ctx.Settings.Get(key, defaultValue)), false);

        /// <summary>
        /// Creates a config value by getting the metadata value from the execution context of a specified key.
        /// </summary>
        /// <param name="key">The metadata key to get the value from.</param>
        /// <param name="defaultValue">The default value to use if the key cannot be found or is null.</param>
        /// <returns>A config object.</returns>
        public static Config<object> FromSetting(string key, object defaultValue = null) =>
            new Config<object>((_, ctx) => Task.FromResult(ctx.Settings.Get(key, defaultValue)), false);

        /// <summary>
        /// Creates a config value from a delegate that uses the settings.
        /// </summary>
        /// <typeparam name="TValue">The type of config value.</typeparam>
        /// <param name="func">The delegate that produces the config value.</param>
        /// <returns>A config object.</returns>
        public static Config<TValue> FromSettings<TValue>(Func<ISettings, TValue> func)
        {
            _ = func ?? throw new ArgumentNullException(nameof(func));
            return new Config<TValue>((_, ctx) => Task.FromResult(func(ctx.Settings)), false);
        }

        /// <summary>
        /// Creates a config value from a delegate that uses the settings.
        /// </summary>
        /// <typeparam name="TValue">The type of config value.</typeparam>
        /// <param name="func">The delegate that produces the config value.</param>
        /// <returns>A config object.</returns>
        public static Config<TValue> FromSettings<TValue>(Func<ISettings, Task<TValue>> func)
        {
            _ = func ?? throw new ArgumentNullException(nameof(func));
            return new Config<TValue>((_, ctx) => func(ctx.Settings), false);
        }

        /// <summary>
        /// Creates a config value from an action that uses the settings and returns
        /// the default value of <typeparamref name="TValue"/>.
        /// </summary>
        /// <typeparam name="TValue">The type of config value.</typeparam>
        /// <param name="action">A delegate action to evaluate.</param>
        /// <returns>A config object.</returns>
        public static Config<TValue> FromSettings<TValue>(Action<ISettings> action) =>
            new Config<TValue>((__, ctx) =>
            {
                _ = action ?? throw new ArgumentNullException(nameof(action));
                action(ctx.Settings);
                return Task.FromResult(default(TValue));
            });

        /// <summary>
        /// Creates a config value from an action that uses the settings and returns
        /// the default value of <typeparamref name="TValue"/>.
        /// </summary>
        /// <typeparam name="TValue">The type of config value.</typeparam>
        /// <param name="action">A delegate action to evaluate.</param>
        /// <returns>A config object.</returns>
        public static Config<TValue> FromSettings<TValue>(Func<ISettings, Task> action) =>
            new Config<TValue>(async (__, ctx) =>
            {
                _ = action ?? throw new ArgumentNullException(nameof(action));
                await action(ctx.Settings);
                return default;
            });

        /// <summary>
        /// Creates a config value from an action that uses the settings and returns null.
        /// </summary>
        /// <param name="action">A delegate action to evaluate.</param>
        /// <returns>A config object.</returns>
        public static Config<object> FromSettings(Action<ISettings> action) =>
            FromSettings<object>(action);

        /// <summary>
        /// Creates a config value from an action that uses the settings and returns null.
        /// </summary>
        /// <param name="action">A delegate action to evaluate.</param>
        /// <returns>A config object.</returns>
        public static Config<object> FromSettings(Func<ISettings, Task> action) =>
            FromSettings<object>(action);
    }
}
