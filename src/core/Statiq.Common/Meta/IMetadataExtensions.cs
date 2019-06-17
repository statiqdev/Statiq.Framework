namespace Statiq.Common.Meta
{
    public static class IMetadataExtensions
    {
        /// <summary>
        /// Gets the value for the specified key. This method never throws an exception. It will return the specified
        /// default value or null if the key is not found or is <c>null</c>.
        /// </summary>
        /// <param name="metadata">The metadata.</param>
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
        /// <param name="metadata">The metadata.</param>
        /// <param name="key">The key of the value to get.</param>
        /// <returns>The value for the specified key converted to type T or default(T) if the key is not found, is <n>null</n>, or cannot be converted to type T.</returns>
        public static T Get<T>(this IMetadata metadata, string key) =>
            metadata.TryGetValue(key, out T value) ? value : default;

        /// <summary>
        /// Gets the value for the specified key. This method never throws an exception. It will return the specified
        /// default value if the key is not found or is <c>null</c>.
        /// </summary>
        /// <typeparam name="T">The desired return type.</typeparam>
        /// <param name="metadata">The metadata.</param>
        /// <param name="key">The key of the value to get.</param>
        /// <param name="defaultValue">The default value to use if the key is not found, is <c>null</c>, or cannot be converted to type T.</param>
        /// <returns>The value for the specified key converted to type T or the specified default value.</returns>
        public static T Get<T>(this IMetadata metadata, string key, T defaultValue) =>
            metadata.TryGetValue(key, out T value) ? value : defaultValue;
    }
}
