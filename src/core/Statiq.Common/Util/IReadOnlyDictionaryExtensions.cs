using System;
using System.Collections.Generic;
using System.Linq;

namespace Statiq.Common
{
    public static class IReadOnlyDictionaryExtensions
    {
        /// <summary>
        /// Verifies that a dictionary contains all requires keys.
        /// An <see cref="ArgumentException"/> will be thrown if the
        /// specified keys are not all present in the dictionary.
        /// </summary>
        /// <typeparam name="TKey">The type of keys.</typeparam>
        /// <typeparam name="TValue">The type of values.</typeparam>
        /// <param name="dictionary">The dictionary to verify.</param>
        /// <param name="keys">The keys that must be present in the dictionary.</param>
        public static void RequireKeys<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> dictionary, params TKey[] keys)
        {
            if (!keys.All(x => dictionary.ContainsKey(x)))
            {
                throw new ArgumentException("Dictionary does not contain all required keys");
            }
        }
    }
}
