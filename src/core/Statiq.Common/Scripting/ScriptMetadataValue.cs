using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Statiq.Common
{
    /// <summary>
    /// A lazily evaluated metadata value based on script code.
    /// </summary>
    public sealed class ScriptMetadataValue : IMetadataValue
    {
        private readonly string _key;
        private readonly string _originalPrefix;
        private readonly string _script;
        private readonly IExecutionState _executionState;

        private ScriptMetadataValue(string key, string originalPrefix, string script, IExecutionState executionState)
        {
            _key = key.ThrowIfNull(nameof(key));
            _originalPrefix = originalPrefix.ThrowIfNull(nameof(originalPrefix));
            _script = script.ThrowIfNull(nameof(script));
            _executionState = executionState.ThrowIfNull(nameof(executionState));
        }

        public object Get(string key, IMetadata metadata)
        {
            // Check if we're excluded from evaluation
            if (metadata is object
                && metadata.TryGetValue(Keys.ExcludeFromEvaluation, out object excludeObject)
                && ((excludeObject is bool excludeBool && excludeBool)
                    || metadata.GetList<string>(Keys.ExcludeFromEvaluation).Contains(_key, StringComparer.OrdinalIgnoreCase)))
            {
                return _originalPrefix + _script;
            }

            // Evaluate the script
            return _executionState.ScriptHelper.EvaluateAsync(_script, metadata).GetAwaiter().GetResult();
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
