using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Wyam.Common.Meta;

namespace Wyam.Common.Meta
{
    /// <summary>
    /// This wraps an <see cref="IMetadata"/> implementation and provides strongly-typed access with type conversion.
    /// Only values that can be converted to the requested type are considered part of the dictionary.
    /// </summary>
    /// <remarks>This class is generally used as part of <see cref="IMetadata"/> implementations.</remarks>
    /// <typeparam name="T">The type to convert metadata values to.</typeparam>
    public class MetadataAs<T> : IMetadata<T>
    {
        private readonly IMetadata _metadata;
        private readonly IMetadataTypeConverter _typeConverter;

        public MetadataAs(IMetadata metadata, IMetadataTypeConverter typeConverter)
        {
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _typeConverter = typeConverter ?? throw new ArgumentNullException(nameof(typeConverter));
        }

        public IEnumerator<KeyValuePair<string, T>> GetEnumerator()
        {
            return _metadata
                .Select(x => _typeConverter.TryConvert(x.Value, out T value)
                    ? new KeyValuePair<string, T>?(new KeyValuePair<string, T>(x.Key, value))
                    : null)
                .Where(x => x.HasValue)
                .Select(x => x.Value)
                .GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public int Count => _metadata.Count(x => _typeConverter.TryConvert(x.Value, out T value));

        public bool ContainsKey(string key) => TryGetValue(key, out _);

        public T this[string key]
        {
            get
            {
                if (!TryGetValue(key, out T value))
                {
                    throw new KeyNotFoundException("The key " + key + " was not found in metadata, use Get() to provide a default value.");
                }
                return value;
            }
        }

        public IEnumerable<string> Keys => this.Select(x => x.Key);

        public IEnumerable<T> Values => this.Select(x => x.Value);

        public T Get(string key) => TryGetValue(key, out T value) ? value : default;

        public T Get(string key, T defaultValue) => TryGetValue(key, out T value) ? value : defaultValue;

        public bool TryGetValue(string key, out T value) => _metadata.TryGetValue(key, out value);
    }
}
