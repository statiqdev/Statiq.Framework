using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace Wyam.Common.Util
{
    /// <summary>
    /// Pools string values.
    /// </summary>
    public class StringPool
    {
        // Key = hash code, Value = matching weak references
        private readonly ConcurrentDictionary<int, List<WeakReference<string>>> _pool =
            new ConcurrentDictionary<int, List<WeakReference<string>>>();

        /// <summary>
        /// Returns the single instance of a given string.
        /// </summary>
        /// <param name="value">The string to pool.</param>
        /// <returns>A single instance of the pooled string.</returns>
        public string GetOrAdd(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return value;
            }

            _pool.AddOrUpdate(
                value.GetHashCode(),
                _ => new List<WeakReference<string>> { new WeakReference<string>(value) },
                (_, references) =>
                {
                    // Iterate backwards so we can safely remove entries
                    for (int c = references.Count - 1; c >= 0; c--)
                    {
                        if (!references[c].TryGetTarget(out string pooled))
                        {
                            references.RemoveAt(c);
                        }
                        else if (pooled.Equals(value))
                        {
                            value = pooled;
                            return references;
                        }
                    }

                    // Didn't find it so add a new one
                    references.Add(new WeakReference<string>(value));
                    return references;
                });

            return value;
        }
    }
}
