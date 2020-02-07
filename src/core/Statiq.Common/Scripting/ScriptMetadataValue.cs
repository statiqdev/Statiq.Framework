using System;
using System.Collections.Generic;
using System.Linq;
using Statiq.Common;

namespace Statiq.Common
{
    /// <summary>
    /// A lazily evaluated metadata value based on script code.
    /// </summary>
    public class ScriptMetadataValue : IMetadataValue
    {
        private readonly string _code;
        private readonly IExecutionState _executionState;
        private HashSet<string> _cachedMetadataKeys;
        private Type _cachedScriptType;

        public ScriptMetadataValue(string code, IExecutionState executionState)
        {
            _code = code ?? throw new ArgumentNullException(nameof(code));
            _executionState = executionState ?? throw new ArgumentNullException(nameof(executionState));
        }

        public object Get(IMetadata metadata)
        {
            if (_cachedMetadataKeys?.SetEquals(metadata.Keys) != true)
            {
                // Compilation cache miss: not cached or the metadata keys are different
                string[] keys = metadata.Keys.ToArray();
                _cachedMetadataKeys = new HashSet<string>(keys);
                byte[] rawAssembly = _executionState.ScriptHelper.Compile(_code, keys);
                _cachedScriptType = _executionState.ScriptHelper.Load(rawAssembly);
            }
            return _executionState.ScriptHelper.EvaluateAsync(_cachedScriptType, metadata).Result;
        }

        public static bool TryGetMetadataValue(object value, IExecutionState executionState, out ScriptMetadataValue metadataValue)
        {
            metadataValue = default;
            if (value is string stringValue)
            {
                stringValue = stringValue.TrimStart();
                if (stringValue.StartsWith("=>"))
                {
                    stringValue = stringValue.Substring(2);
                    if (!string.IsNullOrWhiteSpace(stringValue))
                    {
                        metadataValue = new ScriptMetadataValue(stringValue, executionState);
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
