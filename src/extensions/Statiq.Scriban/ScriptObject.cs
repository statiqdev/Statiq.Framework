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

        private readonly ImmutableHashSet<string> _members;
        private readonly Dictionary<string, object> _locals;

        public ScriptObject(IDocument document, IEnumerable<KeyValuePair<string, object>> locals = null)
        {
            _document = document;
            _members = document.Keys
                .Select(StandardMemberRenamer.Rename)
                .ToImmutableHashSet(StringComparer.OrdinalIgnoreCase);

            _locals = locals?.ToDictionary(x => x.Key, x => x.Value) ?? new Dictionary<string, object>();
        }

        public IEnumerable<string> GetMembers() => _locals.Keys.Concat(_members);

        public bool Contains(string member) => _locals.ContainsKey(member) || _members.Contains(member);

        public bool TryGetValue(TemplateContext context, SourceSpan span, string member, out object value)
            => _locals.TryGetValue(member, out value) || _document.TryGetValue(member, out value);

        public bool CanWrite(string member) => !_members.Contains(member);

        public void SetValue(TemplateContext context, SourceSpan span, string member, object value, bool readOnly)
            => _locals[member] = value; // TODO: We are ignoring readOnly.

        public bool Remove(string member) => !_members.Contains(member) && _locals.Remove(member);

        public void SetReadOnly(string member, bool readOnly)
        {
            // TODO: We are ignoring readOnly.
        }

        public IScriptObject Clone(bool deep) => new ScriptObject(_document.Clone(_document.ContentProvider), _locals);

        public int Count => _locals.Count + _members.Count;

        public bool IsReadOnly
        {
            get => true;
            set => throw new NotImplementedException();
        }
    }
}