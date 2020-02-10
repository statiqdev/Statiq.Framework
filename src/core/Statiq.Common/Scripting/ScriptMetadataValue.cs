using System;
using System.Collections.Generic;
using System.Linq;
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
        private HashSet<string> _cachedMetadataKeys;
        private Type _cachedScriptType;

        private ScriptMetadataValue(string key, string originalPrefix, string script, IExecutionState executionState)
        {
            _key = key ?? throw new ArgumentNullException(nameof(key));
            _originalPrefix = originalPrefix ?? throw new ArgumentNullException(nameof(originalPrefix));
            _script = script ?? throw new ArgumentNullException(nameof(script));
            _executionState = executionState ?? throw new ArgumentNullException(nameof(executionState));
        }

        public object Get(IMetadata metadata)
        {
            // The metadata value could get resolved concurrently so we need to lock it while caching
            lock (Lock)
            {
                // Check if we're excluded from evaluation
                if (metadata.TryGetValue(Keys.ExcludeFromEvaluation, out object excludeObject)
                    && ((excludeObject is bool excludeBool && excludeBool)
                        || metadata.GetList<string>(Keys.ExcludeFromEvaluation).Contains(_key, StringComparer.OrdinalIgnoreCase)))
                {
                    return _originalPrefix + _script;
                }

                // Check if we've already cached a compilation for the current set of metadata keys
                if (_cachedMetadataKeys?.SetEquals(metadata.Keys) != true)
                {
                    // Compilation cache miss, not cached or the metadata keys are different
                    string[] keys = metadata.Keys.ToArray();
                    _cachedMetadataKeys = new HashSet<string>(keys);
                    byte[] rawAssembly = _executionState.ScriptHelper.Compile(_script, keys);
                    _cachedScriptType = _executionState.ScriptHelper.Load(rawAssembly);
                }
            }

            return _executionState.ScriptHelper.EvaluateAsync(_cachedScriptType, metadata).Result;
        }

        public static bool TryGetScriptMetadataValue(string key, object value, IExecutionState executionState, out ScriptMetadataValue scriptMetadataValue)
        {
            scriptMetadataValue = default;
            if (value is string script)
            {
                script = script.TrimStart();
                if (script.StartsWith("=>"))
                {
                    script = script.Substring(2);
                    if (!string.IsNullOrWhiteSpace(script))
                    {
                        scriptMetadataValue = new ScriptMetadataValue(
                            key,
                            ((string)value).Substring(0, ((string)value).Length - script.Length),
                            script,
                            executionState);
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
