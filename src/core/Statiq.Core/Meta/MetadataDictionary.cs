using System;
using System.Collections.Generic;
using System.Linq;
using Statiq.Common.Meta;

namespace Statiq.Core.Meta
{
    internal class MetadataDictionary : Metadata, IMetadataDictionary
    {
        // Ensure items is not null when calling the base ctor so the dictionary gets instantiated
        public MetadataDictionary(IEnumerable<KeyValuePair<string, object>> items = null)
            : base(items ?? Array.Empty<KeyValuePair<string, object>>())
        {
        }

        public void Add(KeyValuePair<string, object> item) => Dictionary.Add(item);

        public void Clear() => Dictionary.Clear();

        public bool Contains(KeyValuePair<string, object> item) => Dictionary.Contains(item);

        public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
            => Dictionary.CopyTo(array, arrayIndex);

        public bool Remove(KeyValuePair<string, object> item) => Dictionary.Remove(item);

        public bool IsReadOnly { get; } = false;

        public void Add(string key, object value) => Dictionary.Add(key, value);

        public bool Remove(string key) => Dictionary.Remove(key);

        object IDictionary<string, object>.this[string key]
        {
            get { return Dictionary[key]; }
            set { Dictionary[key] = value; }
        }

        public new object this[string key]
        {
            get { return ((IDictionary<string, object>)this)[key]; }
            set { ((IDictionary<string, object>)this)[key] = value; }
        }

        ICollection<string> IDictionary<string, object>.Keys => Dictionary.Keys;

        ICollection<object> IDictionary<string, object>.Values => Dictionary.Values;

        ICollection<string> IMetadataDictionary.Keys => ((IDictionary<string, object>)this).Keys;

        ICollection<object> IMetadataDictionary.Values => ((IDictionary<string, object>)this).Values;
    }
}