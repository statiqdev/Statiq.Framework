using System;
using System.Collections.Generic;
using System.Text;

namespace Wyam.Common.Meta
{
    /// <summary>
    /// Contains a set of metadata converted to type <typeparamref name="T"/>.
    /// The conversion is designed to be flexible and several different methods of type
    /// conversion are tried. Only those values that can be converted to type <typeparamref name="T"/>
    /// are actually included in the dictionary.
    /// </summary>
    /// <typeparam name="T">The type all metadata values should be converted to.</typeparam>
    public interface IMetadata<T> : IReadOnlyDictionary<string, T>
    {
        /// <summary>Gets the value associated with the specified key converted to <typeparamref name="T"/>.</summary>
        /// <param name="key">The key of the value to get.</param>
        /// <returns>The converted value for the specified key or <c>default(T)</c> if not found.</returns>
        T Get(string key);

        /// <summary>Gets the value associated with the specified key converted to <typeparamref name="T"/>.</summary>
        /// <param name="key">The key of the value to get.</param>
        /// <param name="defaultValue">The default value to use if either the key is not found or the underlying type is not convertible.</param>
        /// <returns>The converted value for the specified key or <paramref name="defaultValue"/> if not found.</returns>
        T Get(string key, T defaultValue);
    }
}
