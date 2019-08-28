using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Statiq.Common;

namespace Statiq.Json
{
    /// <summary>
    /// Parses JSON content for each input document and stores the result in it's metadata.
    /// </summary>
    /// <remarks>
    /// Parses the content for each input document and then stores a dynamic object
    /// representing the JSON in metadata with the specified key. If no key is specified,
    /// then the dynamic object is not added. You can also flatten the JSON to add top-level items directly
    /// to the document metadata.
    /// </remarks>
    /// <category>Metadata</category>
    public class ParseJson : ParallelSyncModule
    {
        private readonly bool _flatten;
        private readonly string _key;

        /// <summary>
        /// The content of the input document is parsed as JSON. All root-level items are added to the input document's
        /// metadata. This is best for simple key-value JSON documents.
        /// </summary>
        public ParseJson()
        {
            _flatten = true;
        }

        /// <summary>
        /// The content of the input document is parsed as JSON. A dynamic object representing the JSON content
        /// is set as the value for the given metadata key. If flatten is true, all root-level items are also added
        /// to the input document's metadata.
        /// </summary>
        /// <param name="key">The metadata key in which to set the dynamic JSON object.</param>
        /// <param name="flatten">If set to <c>true</c>, all root-level items are also added to the input document's metadata.</param>
        public ParseJson(string key, bool flatten = false)
        {
            _key = key;
            _flatten = flatten;
        }

        protected override IEnumerable<IDocument> Execute(IDocument input, IExecutionContext context)
        {
            try
            {
                JsonSerializer serializer = new JsonSerializer();
                Dictionary<string, object> items = new Dictionary<string, object>();
                ExpandoObject json;
                using (TextReader contentReader = new StreamReader(input.GetStream()))
                {
                    using (JsonReader jsonReader = new JsonTextReader(contentReader))
                    {
                        json = serializer.Deserialize<ExpandoObject>(jsonReader);
                    }
                }
                if (json != null)
                {
                    if (!string.IsNullOrEmpty(_key))
                    {
                        items[_key] = json;
                    }
                    if (_flatten)
                    {
                        foreach (KeyValuePair<string, object> item in json)
                        {
                            items[item.Key] = item.Value;
                        }
                    }
                    return input.Clone(items).Yield();
                }
            }
            catch (Exception ex)
            {
                // Return original input on exception
                context.Logger.LogError($"Error processing JSON for {input.ToSafeDisplayString()}, returning original input document: {ex}");
            }
            return input.Yield();
        }
    }
}
