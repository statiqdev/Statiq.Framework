using System.Collections.Generic;

namespace Statiq.Common
{
    public partial interface IMetadata
    {
        /// <summary>
        /// Gets the value for the specified key. This method never throws an exception. It will return the specified
        /// default value or null if the key is not found or is <c>null</c>.
        /// </summary>
        /// <param name="key">The key of the value to get.</param>
        /// <param name="defaultValue">The default value to use if the key is not found or is <c>null</c>.</param>
        /// <returns>The value for the specified key or the specified default value.</returns>
        public object Get(
            string key,
            object defaultValue = null) =>
            TryGetValue(key, out object value) ? value : defaultValue;

        /// <summary>
        /// Gets the value for the specified key converted to the specified type.
        /// This method never throws an exception. It will return default(T) if the key is not found,
        /// is <c>null</c>, or the value cannot be converted to T.
        /// </summary>
        /// <typeparam name="T">The desired return type.</typeparam>
        /// <param name="key">The key of the value to get.</param>
        /// <returns>The value for the specified key converted to type T or default(T) if the key is not found, is <n>null</n>, or cannot be converted to type T.</returns>
        public T Get<T>(string key) => TryGetValue(key, out T value) ? value : default;

        /// <summary>
        /// Gets the value for the specified key. This method never throws an exception. It will return the specified
        /// default value if the key is not found or is <c>null</c>.
        /// </summary>
        /// <typeparam name="T">The desired return type.</typeparam>
        /// <param name="key">The key of the value to get.</param>
        /// <param name="defaultValue">The default value to use if the key is not found, is <c>null</c>, or cannot be converted to type T.</param>
        /// <returns>The value for the specified key converted to type T or the specified default value.</returns>
        public T Get<T>(string key, T defaultValue) => TryGetValue(key, out T value) ? value : defaultValue;

        /// <summary>
        /// Gets the raw value for the specified key. This method will not materialize <see cref="IMetadataValue"/>
        /// values the way other get methods will. A <see cref="KeyNotFoundException"/> will be thrown
        /// for missing keys.
        /// </summary>
        /// <param name="key">The key of the value to get.</param>
        /// <returns>The raw value for the specified key.</returns>
        public object GetRaw(string key) => TryGetRaw(key, out object value) ? value : throw new KeyNotFoundException(nameof(key));
    }
}
