using System;
using Statiq.CodeAnalysis.Scripting;
using Statiq.Common;

namespace Statiq.CodeAnalysis
{
    public static class IDocumentExtensions
    {
        public static string Interpolate(this IDocument document, string str, IExecutionContext context)
        {
            _ = document ?? throw new ArgumentNullException(nameof(document));
            _ = context ?? throw new ArgumentNullException(nameof(context));

            if (string.IsNullOrEmpty(str))
            {
                return string.Empty;
            }

            byte[] assembly = ScriptHelper.Compile($"return $\"{str}\";", document, context);
            return (string)ScriptHelper.EvaluateAsync(assembly, document, context).Result;
        }
    }
}
