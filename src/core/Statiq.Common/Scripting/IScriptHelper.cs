using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Statiq.Common;

namespace Statiq.Common
{
    public interface IScriptHelper
    {
        public const string ScriptStringPrefix = "=>";

        /// <summary>
        /// Compiles, caches, and evaluates a script.
        /// </summary>
        /// <remarks>
        /// The script compilation is cached, so as long as the script remains the same
        /// it will not need to be recompiled.
        /// </remarks>
        /// <param name="code">The code to compile.</param>
        /// <param name="metadata">
        /// The metadata used to construct the script. Metadata items are exposed a properties with
        /// the name of the key and can be used directly in the script.
        /// </param>
        /// <returns>Raw assembly bytes.</returns>
        Task<object> EvaluateAsync(string code, IMetadata metadata);

        /// <summary>
        /// Checks if the string is a "script" string (it starts with <c>=></c>)
        /// and trims the string if it is.
        /// </summary>
        /// <param name="str">The candidate string.</param>
        /// <param name="script">The trimmed script.</param>
        /// <returns><c>true</c> if the candidate string is a script string, <c>false</c> otherwise.</returns>
        public static bool TryGetScriptString(string str, out string script)
        {
            script = str;
            if (!string.IsNullOrWhiteSpace(str))
            {
                script = str.TrimStart();
                if (script.StartsWith(ScriptStringPrefix))
                {
                    script = script.Substring(2);
                    if (!string.IsNullOrWhiteSpace(script))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
