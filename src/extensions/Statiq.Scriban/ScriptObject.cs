using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Scriban;
using Scriban.Parsing;
using Scriban.Runtime;
using Statiq.Common;

namespace Statiq.Scriban
{
    internal class ScriptObject : IScriptObject
    {
        private readonly IDocument _document;

        private readonly ImmutableHashSet<string> _documentProperties;
        private readonly ImmutableHashSet<string> _members;

        public ScriptObject(IDocument document)
        {
            _document = document;
            _documentProperties = IDocument.Properties;
            _members = document.Keys
                .Concat(_documentProperties)
                .ToImmutableHashSet(StringComparer.OrdinalIgnoreCase);
        }

        public IEnumerable<string> GetMembers() => _members;

        public bool Contains(string member) => _members.Contains(member);

        public bool TryGetValue(TemplateContext context, SourceSpan span, string member, out object value)
        {
            return _document.TryGetValue(member, out value);
        }

        public bool CanWrite(string member) => false;

        public void SetValue(TemplateContext context, SourceSpan span, string member, object value, bool readOnly)
        {
        }

        public bool Remove(string member) => false;

        public void SetReadOnly(string member, bool readOnly)
        {
        }

        public IScriptObject Clone(bool deep) => new ScriptObject(_document.Clone(_document.ContentProvider));

        public int Count => _members.Count;

        public bool IsReadOnly
        {
            get => true;
            set => throw new NotImplementedException();
        }
    }
}