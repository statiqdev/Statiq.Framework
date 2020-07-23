using System;
using System.Collections.Concurrent;

namespace Statiq.Common
{
    /// <summary>
    /// This class uses a delegate to get a metadata value. The result of the delegate
    /// will be cached and the cached value will be returned for subsequent calls to <see cref="Get"/>.
    /// </summary>
    public class CachedDelegateMetadataValue : DelegateMetadataValue
    {
        // Cache values by key and source metadata
        private readonly ConcurrentDictionary<(string, IMetadata), object> _cache =
            new ConcurrentDictionary<(string, IMetadata), object>();

        /// <summary>
        /// Initializes a new instance of the <see cref="CachedDelegateMetadataValue"/> class.
        /// The specified delegate should be thread-safe.
        /// </summary>
        /// <param name="value">The delegate that returns the metadata value.</param>
        public CachedDelegateMetadataValue(Func<IMetadata, object> value)
            : base(value)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CachedDelegateMetadataValue"/> class.
        /// The specified delegate should be thread-safe.
        /// </summary>
        /// <param name="value">The delegate that returns the metadata value.</param>
        public CachedDelegateMetadataValue(Func<string, IMetadata, object> value)
            : base(value)
        {
        }

        /// <summary>
        /// Lazily loads a metadata value and caches the value.
        /// </summary>
        /// <param name="key">The metadata key being requested.</param>
        /// <param name="metadata">The metadata object requesting the value.</param>
        /// <returns>The object to use as the value.</returns>
        public override object Get(string key, IMetadata metadata) =>
            _cache.GetOrAdd((key, metadata), _ => base.Get(key, metadata));
    }
}