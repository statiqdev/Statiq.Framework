using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.Extensions.Logging;
using Statiq.Common;

namespace Statiq.Core
{
    internal class ScriptFactoryCache
    {
        private readonly ConcurrentDictionary<HashSet<KeyValuePair<string, string>>, ScriptFactoryBase> _cachedScriptFactories =
            new ConcurrentDictionary<HashSet<KeyValuePair<string, string>>, ScriptFactoryBase>(HashSet<KeyValuePair<string, string>>.CreateSetComparer());

        private readonly ScriptHelper _scriptHelper;

        public ScriptFactoryCache(ScriptHelper scriptHelper)
        {
            _scriptHelper = scriptHelper;
        }

        public ScriptBase GetScript(string script, IMetadata metadata, IExecutionState executionState, IExecutionContext executionContext)
        {
            HashSet<KeyValuePair<string, string>> metadataProperties = ScriptHelper.GetMetadataProperties(metadata).ToHashSet(MetadataPropertyComparer.Instance);
            ScriptFactoryBase scriptFactory = _cachedScriptFactories.GetOrAdd(metadataProperties, x =>
            {
                executionContext?.LogDebug($"Script cache miss for script `{(script.Length > 20 ? (script.Substring(0, 19) + "...") : script)}`");
                byte[] rawAssembly = _scriptHelper.Compile(script, x);
                Type scriptFactoryType = ScriptHelper.LoadFactory(rawAssembly);
                return (ScriptFactoryBase)Activator.CreateInstance(scriptFactoryType);
            });
            return scriptFactory.GetScript(metadata, executionState, executionContext);
        }

        private class MetadataPropertyComparer : IEqualityComparer<KeyValuePair<string, string>>
        {
            public static readonly MetadataPropertyComparer Instance = new MetadataPropertyComparer();

            public bool Equals([AllowNull] KeyValuePair<string, string> x, [AllowNull] KeyValuePair<string, string> y) =>
                x.Key.Equals(y.Key, StringComparison.OrdinalIgnoreCase) && (x.Value?.Equals(y.Value) ?? y.Value == null);

            public int GetHashCode([DisallowNull] KeyValuePair<string, string> obj) =>
                HashCode.Combine(
                    StringComparer.OrdinalIgnoreCase.GetHashCode(obj.Key),
                    obj.Value?.GetHashCode() ?? 0);
        }
    }
}
