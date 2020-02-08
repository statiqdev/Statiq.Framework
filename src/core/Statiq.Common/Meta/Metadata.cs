using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Statiq.Common;

namespace Statiq.Common
{
    /// <summary>
    /// A linked list of metadata items.
    /// </summary>
    public class Metadata : IMetadata
    {
        private readonly IMetadata _previous;

        protected IDictionary<string, object> Dictionary { get; }

        /// <summary>
        /// Creates a new set of metadata.
        /// </summary>
        /// <param name="executionState">The current execution state.</param>
        /// <param name="previous">The previous set of metadata this one should extend.</param>
        /// <param name="items">The initial set of items. If null, no underlying dictionary will be created.</param>
        public Metadata(IExecutionState executionState, IMetadata previous, IEnumerable<KeyValuePair<string, object>> items = null)
            : this(executionState, items)
        {
            _previous = previous;
        }

        /// <summary>
        /// Creates a new set of metadata.
        /// </summary>
        /// <param name="executionState">The current execution state.</param>
        /// <param name="items">The initial set of items. If null, no underlying dictionary will be created.</param>
        public Metadata(IExecutionState executionState, IEnumerable<KeyValuePair<string, object>> items = null)
        {
            _ = executionState ?? throw new ArgumentNullException(nameof(executionState));

            if (items != null)
            {
                Dictionary = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
                foreach (KeyValuePair<string, object> item in items)
                {
                    if (ScriptMetadataValue.TryGetScriptMetadataValue(item.Key, item.Value, executionState, out ScriptMetadataValue metadataValue))
                    {
                        Dictionary[item.Key] = metadataValue;
                    }
                    else
                    {
                        Dictionary[item.Key] = item.Value;
                    }
                }
            }
        }

        /// <summary>
        /// Creates a new set of metadata without evaluating scripted metadata.
        /// </summary>
        /// <remarks>
        /// This is marked as <c>protected internal</c> to discourage public use. When at all possible, metadata
        /// should be created such that it evaluated scripted metadata. If a <see cref="IExecutionState"/> isn't
        /// available for use at the time the metadata is needed, then a derived type should be created for use.
        /// </remarks>
        /// <param name="previous">The previous set of metadata this one should extend.</param>
        /// <param name="items">The initial set of items. If null, no underlying dictionary will be created.</param>
        protected internal Metadata(IMetadata previous, IEnumerable<KeyValuePair<string, object>> items = null)
            : this(items)
        {
            _previous = previous;
        }

        /// <summary>
        /// Creates a new set of metadata without evaluating scripted metadata.
        /// </summary>
        /// <remarks>
        /// This is marked as <c>protected internal</c> to discourage public use. When at all possible, metadata
        /// should be created such that it evaluated scripted metadata. If a <see cref="IExecutionState"/> isn't
        /// available for use at the time the metadata is needed, then a derived type should be created for use.
        /// </remarks>
        /// <param name="items">The initial set of items. If null, no underlying dictionary will be created.</param>
        protected internal Metadata(IEnumerable<KeyValuePair<string, object>> items = null)
        {
            if (items != null)
            {
                Dictionary = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
                foreach (KeyValuePair<string, object> item in items)
                {
                    Dictionary[item.Key] = item.Value;
                }
            }
        }

        public bool ContainsKey(string key)
        {
            _ = key ?? throw new ArgumentNullException(nameof(key));
            return (Dictionary?.ContainsKey(key) ?? false) || (_previous?.ContainsKey(key) ?? false);
        }

        public bool TryGetRaw(string key, out object value)
        {
            _ = key ?? throw new ArgumentNullException(nameof(key));
            value = default;
            return (Dictionary?.TryGetValue(key, out value) ?? false) || (_previous?.TryGetRaw(key, out value) ?? false);
        }

        public bool TryGetValue<TValue>(string key, out TValue value)
        {
            value = default;
            if (key != null && TryGetRaw(key, out object rawValue))
            {
                return TypeHelper.TryExpandAndConvert(rawValue, this, out value);
            }
            return false;
        }

        public bool TryGetValue(string key, out object value) => TryGetValue<object>(key, out value);

        public object this[string key]
        {
            get
            {
                _ = key ?? throw new ArgumentNullException(nameof(key));
                if (!TryGetValue(key, out object value))
                {
                    throw new KeyNotFoundException("The key " + key + " was not found in metadata, use Get() to provide a default value.");
                }
                return value;
            }
        }

        // Enumerate the keys seperatly so we don't evaluate values
        public IEnumerable<string> Keys
        {
            get
            {
                if (Dictionary != null)
                {
                    foreach (string key in Dictionary.Keys)
                    {
                        yield return key;
                    }
                }
                if (_previous != null)
                {
                    foreach (string previousKey in _previous.Keys)
                    {
                        if (!Dictionary.ContainsKey(previousKey))
                        {
                            yield return previousKey;
                        }
                    }
                }
            }
        }

        public IEnumerable<object> Values => this.Select(x => x.Value);

        // The Select ensures LINQ optimizations won't turn this into a recursive call to Count
        public int Count => this.Select(_ => (object)null).Count();

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            if (Dictionary != null)
            {
                foreach (KeyValuePair<string, object> item in Dictionary)
                {
                    yield return TypeHelper.ExpandKeyValuePair(item, this);
                }
            }
            if (_previous != null)
            {
                foreach (KeyValuePair<string, object> previousItem in _previous)
                {
                    if (!Dictionary.ContainsKey(previousItem.Key))
                    {
                        yield return previousItem;
                    }
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}