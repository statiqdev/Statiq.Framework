#nullable enable
using System;
using System.Collections;
using System.Collections.Immutable;
using System.Linq;
using Scriban.Runtime;
using Statiq.Common;

namespace Statiq.Scriban
{
    internal class DocumentDictionary : IDictionary
    {
        private readonly IDocument _document;

        private readonly ImmutableHashSet<string> _members;

        public DocumentDictionary(IDocument document)
        {
            _document = document;
            _members = document.Keys
                .Select(StandardMemberRenamer.Rename)
                .ToImmutableHashSet(StringComparer.OrdinalIgnoreCase);
        }

        public void Add(object key, object? value)
        {
        }

        public void Clear()
        {
        }

        public bool Contains(object key) => _members.Contains(key);

        public IDictionaryEnumerator GetEnumerator()
        {
            return ((IDictionary)_document
                .ToDictionary(x => StandardMemberRenamer.Rename(x.Key), x => x.Value))
                .GetEnumerator();
        }

        public void Remove(object key)
        {
        }

        public bool IsFixedSize => true;
        public bool IsReadOnly => true;

        public object? this[object key]
        {
            get => _document[key.ToString()];
            set => throw new NotImplementedException();
        }

        public ICollection Keys => _members;
        public ICollection Values => _document.Values.ToArray();

        IEnumerator IEnumerable.GetEnumerator() => _document.GetEnumerator();

        public void CopyTo(Array array, int index)
        {
        }

        public int Count => _members.Count;
        public bool IsSynchronized => false;
        public object SyncRoot { get; } = new object();
    }
}