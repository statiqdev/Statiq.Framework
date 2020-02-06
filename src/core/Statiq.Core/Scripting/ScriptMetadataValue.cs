using System;
using System.Collections.Generic;
using System.Linq;
using Statiq.Common;

namespace Statiq.Core
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
            _code = code;
            _executionState = executionState;
        }

        public object Get(IMetadata metadata)
        {
            if (_cachedMetadataKeys?.SetEquals(metadata.Keys) != true)
            {
                // Compilation cache miss: not cached or the metadata keys are different
                string[] keys = metadata.Keys.ToArray();
                _cachedMetadataKeys = new HashSet<string>(keys);
                byte[] rawAssembly = ScriptHelper.Compile(_code, keys, _executionState);
                _cachedScriptType = ScriptHelper.Load(rawAssembly);
            }
            return ScriptHelper.EvaluateAsync(_cachedScriptType, metadata, _executionState).Result;
        }
    }
}
