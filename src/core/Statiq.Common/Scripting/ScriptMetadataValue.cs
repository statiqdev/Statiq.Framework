using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.DotNet.PlatformAbstractions;
using Statiq.Common;

namespace Statiq.Common
{
    /// <summary>
    /// A lazily evaluated metadata value based on script code.
    /// </summary>
    public sealed class ScriptMetadataValue : IMetadataValue
    {
        private static readonly object Lock = new object();

        private readonly string _key;
        private readonly string _originalPrefix;
        private readonly string _script;
        private readonly IExecutionState _executionState;
        private readonly ConcurrentDictionary<HashSet<KeyValuePair<string, string>>, Type> _cachedScriptTypes =
            new ConcurrentDictionary<HashSet<KeyValuePair<string, string>>, Type>(HashSet<KeyValuePair<string, string>>.CreateSetComparer());

        private ScriptMetadataValue(string key, string originalPrefix, string script, IExecutionState executionState)
        {
            _key = key ?? throw new ArgumentNullException(nameof(key));
            _originalPrefix = originalPrefix ?? throw new ArgumentNullException(nameof(originalPrefix));
            _script = script ?? throw new ArgumentNullException(nameof(script));
            _executionState = executionState ?? throw new ArgumentNullException(nameof(executionState));
        }

        public object Get(string key, IMetadata metadata)
        {
            // Check if we're excluded from evaluation
            if (metadata.TryGetValue(Keys.ExcludeFromEvaluation, out object excludeObject)
                && ((excludeObject is bool excludeBool && excludeBool)
                    || metadata.GetList<string>(Keys.ExcludeFromEvaluation).Contains(_key, StringComparer.OrdinalIgnoreCase)))
            {
                return _originalPrefix + _script;
            }

            // Get (or compile) the script based on the current metadata
            HashSet<KeyValuePair<string, string>> metadataProperties = _executionState.ScriptHelper
                .GetMetadataProperties(metadata)
                .ToHashSet(MetadataPropertyComparer.Instance);
            Type cachedScriptType = _cachedScriptTypes.GetOrAdd(metadataProperties, x =>
            {
                byte[] rawAssembly = _executionState.ScriptHelper.Compile(_script, x);
                return _executionState.ScriptHelper.Load(rawAssembly);
            });

            // Evaluate the script
            return _executionState.ScriptHelper.EvaluateAsync(cachedScriptType, metadata).GetAwaiter().GetResult();
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

        public static bool TryGetScriptMetadataValue(string key, object value, IExecutionState executionState, out ScriptMetadataValue scriptMetadataValue)
        {
            scriptMetadataValue = default;
            if (value is string stringValue && IScriptHelper.TryGetScriptString(stringValue, out string script))
            {
                scriptMetadataValue = new ScriptMetadataValue(
                    key,
                    stringValue.Substring(0, stringValue.Length - script.Length),
                    script,
                    executionState);
                return true;
            }
            return false;
        }
    }
}
