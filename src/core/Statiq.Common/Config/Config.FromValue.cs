using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Statiq.Common
{
    public static partial class Config
    {
        /// <summary>
        /// Creates a config value.
        /// </summary>
        /// <typeparam name="TValue">The type of config value.</typeparam>
        /// <param name="value">The config value.</param>
        /// <returns>A config object.</returns>
        public static Config<TValue> FromValue<TValue>(TValue value) =>
            new Config<TValue>((_, __) => Task.FromResult(value), false);

        /// <summary>
        /// Creates a config value.
        /// </summary>
        /// <typeparam name="TValue">The type of config value.</typeparam>
        /// <param name="value">The config value.</param>
        /// <returns>A config object.</returns>
        public static Config<TValue> FromValue<TValue>(Task<TValue> value)
        {
            _ = value ?? throw new ArgumentNullException(nameof(value));
            return new Config<TValue>((_, __) => value, false);
        }

        /// <summary>
        /// Creates an enumeration of config values.
        /// </summary>
        /// <typeparam name="TValue">The type of config value items.</typeparam>
        /// <param name="values">The config values.</param>
        /// <returns>A config object.</returns>
        public static Config<IEnumerable<TValue>> FromValues<TValue>(params TValue[] values) =>
            new Config<IEnumerable<TValue>>((_, __) => Task.FromResult<IEnumerable<TValue>>(values), false);
    }
}
