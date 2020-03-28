using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Statiq.Common;

namespace Statiq.Core
{
    /// <summary>
    /// Renders the shortcode content if the specified key equals a given value (or is true).
    /// </summary>
    /// <parameter name="Key">The key that contains the value.</parameter>
    /// <parameter name="Value">The value to compare (or <c>true</c> if not provided).</parameter>
    public class IfShortcode : DocumentShortcode
    {
        public override async Task<IDocument> ExecuteAsync(
            KeyValuePair<string, string>[] args,
            string content,
            IDocument document,
            IExecutionContext context)
        {
            IMetadataDictionary dictionary = args.ToDictionary(
                "Key",
                "Value");
            dictionary.RequireKeys("Key");

            object keyValue = document.Get(dictionary.GetString("Key"));
            if (dictionary.ContainsKey("Value"))
            {
                return TypeHelper.TryConvert(dictionary.Get("Value"), keyValue.GetType(), out object value)
                    && (keyValue?.Equals(value) ?? (keyValue == null && value == null))
                    ? await document.CloneAsync(content)
                    : null;
            }

            return TypeHelper.TryConvert(keyValue, out bool result) && result
                ? await document.CloneAsync(content)
                : null;
        }
    }
}