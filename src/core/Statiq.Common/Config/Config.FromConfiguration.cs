using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace Statiq.Common
{
    public static partial class Config
    {
        /// <summary>
        /// Creates a config value from a delegate that uses the configuration.
        /// </summary>
        /// <typeparam name="TValue">The type of config value.</typeparam>
        /// <param name="func">The delegate that produces the config value.</param>
        /// <returns>A config object.</returns>
        public static Config<TValue> FromConfiguration<TValue>(Func<IConfiguration, TValue> func)
        {
            _ = func ?? throw new ArgumentNullException(nameof(func));
            return new Config<TValue>((_, ctx) => Task.FromResult(func(ctx.Configuration)), false);
        }

        /// <summary>
        /// Creates a config value from a delegate that uses the configuration.
        /// </summary>
        /// <typeparam name="TValue">The type of config value.</typeparam>
        /// <param name="func">The delegate that produces the config value.</param>
        /// <returns>A config object.</returns>
        public static Config<TValue> FromConfiguration<TValue>(Func<IConfiguration, Task<TValue>> func)
        {
            _ = func ?? throw new ArgumentNullException(nameof(func));
            return new Config<TValue>((_, ctx) => func(ctx.Configuration), false);
        }

        /// <summary>
        /// Creates a config value from an action that uses the configuration and returns
        /// the default value of <typeparamref name="TValue"/>.
        /// </summary>
        /// <typeparam name="TValue">The type of config value.</typeparam>
        /// <param name="action">A delegate action to evaluate.</param>
        /// <returns>A config object.</returns>
        public static Config<TValue> FromConfiguration<TValue>(Action<IConfiguration> action) =>
            new Config<TValue>((__, ctx) =>
            {
                _ = action ?? throw new ArgumentNullException(nameof(action));
                action(ctx.Configuration);
                return Task.FromResult(default(TValue));
            });

        /// <summary>
        /// Creates a config value from an action that uses the configuration and returns
        /// the default value of <typeparamref name="TValue"/>.
        /// </summary>
        /// <typeparam name="TValue">The type of config value.</typeparam>
        /// <param name="action">A delegate action to evaluate.</param>
        /// <returns>A config object.</returns>
        public static Config<TValue> FromConfiguration<TValue>(Func<IConfiguration, Task> action) =>
            new Config<TValue>(async (__, ctx) =>
            {
                _ = action ?? throw new ArgumentNullException(nameof(action));
                await action(ctx.Configuration);
                return default;
            });

        /// <summary>
        /// Creates a config value from an action that uses the configuration and returns null.
        /// </summary>
        /// <param name="action">A delegate action to evaluate.</param>
        /// <returns>A config object.</returns>
        public static Config<object> FromConfiguration(Action<IConfiguration> action) =>
            FromConfiguration<object>(action);

        /// <summary>
        /// Creates a config value from an action that uses the configuration and returns null.
        /// </summary>
        /// <param name="action">A delegate action to evaluate.</param>
        /// <returns>A config object.</returns>
        public static Config<object> FromConfiguration(Func<IConfiguration, Task> action) =>
            FromConfiguration<object>(action);
    }
}
