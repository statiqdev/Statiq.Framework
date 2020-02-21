using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Statiq.Common
{
    public static class IMetadataGetExtensions
    {
        /// <summary>
        /// Gets the value for the specified key. This method never throws an exception. It will return the specified
        /// default value or null if the key is not found or is <c>null</c>.
        /// </summary>
        /// <param name="metadata">The metadata instance.</param>
        /// <param name="key">The key of the value to get.</param>
        /// <param name="defaultValue">The default value to use if the key is not found or is <c>null</c>.</param>
        /// <returns>The value for the specified key or the specified default value.</returns>
        public static object Get(
            this IMetadata metadata,
            string key,
            object defaultValue = null) =>
            metadata.TryGetValue(key, out object value) ? value : defaultValue;

        /// <summary>
        /// Gets the value for the specified key converted to the specified type.
        /// This method never throws an exception. It will return default(T) if the key is not found,
        /// is <c>null</c>, or the value cannot be converted to T.
        /// </summary>
        /// <typeparam name="T">The desired return type.</typeparam>
        /// <param name="metadata">The metadata instance.</param>
        /// <param name="key">The key of the value to get.</param>
        /// <returns>The value for the specified key converted to type T or default(T) if the key is not found, is <n>null</n>, or cannot be converted to type T.</returns>
        public static T Get<T>(this IMetadata metadata, string key) => metadata.TryGetValue(key, out T value) ? value : default;

        /// <summary>
        /// Gets the value for the specified key. This method never throws an exception. It will return the specified
        /// default value if the key is not found or is <c>null</c>.
        /// </summary>
        /// <typeparam name="T">The desired return type.</typeparam>
        /// <param name="metadata">The metadata instance.</param>
        /// <param name="key">The key of the value to get.</param>
        /// <param name="defaultValue">The default value to use if the key is not found, is <c>null</c>, or cannot be converted to type T.</param>
        /// <returns>The value for the specified key converted to type T or the specified default value.</returns>
        public static T Get<T>(this IMetadata metadata, string key, T defaultValue) => metadata.TryGetValue(key, out T value) ? value : defaultValue;

        /// <summary>
        /// Gets the raw value for the specified key. This method will not materialize <see cref="IMetadataValue"/>
        /// values the way other get methods will. A <see cref="KeyNotFoundException"/> will be thrown
        /// for missing keys.
        /// </summary>
        /// <param name="metadata">The metadata instance.</param>
        /// <param name="key">The key of the value to get.</param>
        /// <returns>The raw value for the specified key.</returns>
        public static object GetRaw(this IMetadata metadata, string key) => metadata.TryGetRaw(key, out object value) ? value : throw new KeyNotFoundException(nameof(key));

        /// <summary>
        /// Gets a new filtered <see cref="IMetadata"/> containing only the specified keys and their values. If a key is not present in the current
        /// metadata, it will be ignored and will not be copied to the new metadata object.
        /// </summary>
        /// <param name="metadata">The metadata instance.</param>
        /// <param name="keys">The keys to include in the new filtered metadata object.</param>
        /// <returns>A new filtered <see cref="IMetadata"/> containing the specified keys and their values.</returns>
        public static IMetadata FilterMetadata(this IMetadata metadata, params string[] keys) => new FilteredMetadata(metadata, keys);

        /// <summary>
        /// Gets an enumerable that enumerates raw key-value pairs
        /// (I.e., the values have not been expanded similar to <see cref="IMetadata.TryGetRaw(string, out object)"/>).
        /// </summary>
        /// <param name="metadata">The metadata instance.</param>
        /// <returns>An enumerable over raw key-value pairs.</returns>
        public static IEnumerable<KeyValuePair<string, object>> GetRawEnumerable(this IMetadata metadata) =>
            new RawMetadataEnumerable(metadata ?? throw new ArgumentNullException(nameof(metadata)));
    }
}
