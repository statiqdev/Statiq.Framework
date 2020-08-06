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
        // Built-in properties we want to expose but aren't available via metadata.
        private static readonly string[] PropertyNames =
        {
            nameof(IDocument.Id),
            nameof(IDocument.Count),
            nameof(IDocument.Keys),
            nameof(IDocument.Values)
        };

        private readonly IDocument _document;

        private readonly ImmutableDictionary<string, string> _properties;
        private readonly ImmutableDictionary<string, string> _metadata;
        private readonly Dictionary<string, object> _locals;

        public ScriptObject(IDocument document, IEnumerable<KeyValuePair<string, object>> locals = null)
        {
            _document = document;
            _properties = PropertyNames
                .ToImmutableDictionary(StandardMemberRenamer.Rename);
            _metadata = document.Keys
                .ToImmutableDictionary(StandardMemberRenamer.Rename);

            _locals = locals?.ToDictionary(x => x.Key, x => x.Value) ?? new Dictionary<string, object>();
        }

        public IEnumerable<string> GetMembers() => _locals.Keys
            .Concat(_metadata.Keys)
            .Concat(_properties.Keys)
            .Distinct();

        public bool Contains(string member)
            => _locals.ContainsKey(member) || _metadata.ContainsKey(member) || _properties.ContainsKey(member);

        public bool TryGetValue(TemplateContext context, SourceSpan span, string member, out object value)
        {
            if (_locals.TryGetValue(member, out value))
            {
                return true;
            }

            if (_metadata.TryGetValue(member, out string metadataName))
            {
                return _document.TryGetValue(metadataName, out value);
            }

            if (_properties.TryGetValue(member, out string propertyName))
            {
                value = propertyName switch
                {
                    nameof(IDocument.Id) => _document.Id,
                    nameof(IDocument.Count) => _document.Count,
                    nameof(IDocument.Keys) => _document.Keys.Select(StandardMemberRenamer.Rename),
                    nameof(IDocument.Values) => _document.Values,
                    _ => null
                };

                return true;
            }

            return false;
        }

        public bool CanWrite(string member) => !_metadata.ContainsKey(member) && !_properties.ContainsKey(member);

        public void SetValue(TemplateContext context, SourceSpan span, string member, object value, bool readOnly)
            => _locals[member] = value; // TODO: We are ignoring readOnly.

        public bool Remove(string member) => _locals.Remove(member);

        public void SetReadOnly(string member, bool readOnly)
        {
            // TODO: We are ignoring readOnly.
        }

        public IScriptObject Clone(bool deep) => new ScriptObject(_document.Clone(_document.ContentProvider), _locals);

        public int Count => _locals.Count + _metadata.Count;

        public bool IsReadOnly { get; set; }
    }
}