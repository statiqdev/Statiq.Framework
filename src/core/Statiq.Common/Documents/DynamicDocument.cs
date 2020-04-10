using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Dynamic;
using System.Threading.Tasks;

namespace Statiq.Common
{
    internal class DynamicDocument : DynamicObject, IDocument
    {
        private readonly IDocument _document;

        public DynamicDocument(IDocument document)
        {
            _document = document;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result) =>
            _document.TryGetValue(binder.Name, out result);

        // IDocument

        public object this[string key] => _document[key];

        public Guid Id => _document.Id;

        public NormalizedPath Source => _document.Source;

        public NormalizedPath Destination => _document.Destination;

        public IContentProvider ContentProvider => _document.ContentProvider;

        public IEnumerable<string> Keys => _document.Keys;

        public IEnumerable<object> Values => _document.Values;

        public int Count => _document.Count;

        public IDocument Clone(
            NormalizedPath source,
            NormalizedPath destination,
            IEnumerable<KeyValuePair<string, object>> items,
            IContentProvider contentProvider = null) =>
            _document.Clone(source, destination, items, contentProvider);

        public bool ContainsKey(string key) => _document.ContainsKey(key);

        public Task<int> GetCacheHashCodeAsync() => _document.GetCacheHashCodeAsync();

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator() => _document.GetEnumerator();

        public IEnumerator<KeyValuePair<string, object>> GetRawEnumerator() => _document.GetRawEnumerator();

        public bool TryGetRaw(string key, out object value) => _document.TryGetRaw(key, out value);

        public bool TryGetValue(string key, [MaybeNullWhen(false)] out object value) => _document.TryGetValue(key, out value);

        IEnumerator IEnumerable.GetEnumerator() => _document.GetEnumerator();
    }
}
