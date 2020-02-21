using System;
using System.Collections.Generic;

namespace Statiq.Common
{
    /// <summary>
    /// Contains a set of metadata with flexible runtime conversion methods. Metadata keys are case-insensitive.
    /// </summary>
    public interface IMetadata : IReadOnlyDictionary<string, object>
    {
        /// <summary>
        /// Tries to get the raw value for the specified key. This method will not materialize <see cref="IMetadataValue"/>
        /// values the way <see cref="TryGetValue{T}(string, out T)"/> and other get methods will. A <see cref="KeyNotFoundException"/> will be thrown
        /// for missing keys.
        /// </summary>
        /// <param name="key">The key of the value to get.</param>
        /// <param name="value">The value of the key if it was found.</param>
        /// <returns><c>true</c> if the key was found, <c>false</c> otherwise.</returns>
        bool TryGetRaw(string key, out object value);

        /// <summary>
        /// Tries to get the value for the specified key.
        /// </summary>
        /// <typeparam name="TValue">The desired return type.</typeparam>
        /// <param name="key">The key of the value to get. If the key is <c>null</c>, this will return the default value.</param>
        /// <param name="value">The value of the key if it was found and could be converted to the desired return type.</param>
        /// <returns><c>true</c> if the key was found and the value could be converted to the desired return type, <c>false</c> otherwise.</returns>
        bool TryGetValue<TValue>(string key, out TValue value);

        /// <summary>
        /// Enumerates raw key-value pairs
        /// (I.e., the values have not been expanded similar to <see cref="TryGetRaw(string, out object)"/>).
        /// </summary>
        /// <returns>An enumerator over raw key-value pairs.</returns>
        IEnumerator<KeyValuePair<string, object>> GetRawEnumerator();
    }
}
