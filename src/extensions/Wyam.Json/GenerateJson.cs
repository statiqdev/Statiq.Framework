using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Wyam.Common.Configuration;
using Wyam.Common.Documents;
using Wyam.Common.Meta;
using Wyam.Common.Modules;
using Wyam.Common.Execution;
using Wyam.Common.Tracing;
using Newtonsoft.Json.Serialization;
using Wyam.Common;
using Wyam.Common.IO;

namespace Wyam.Json
{
    /// <summary>
    /// Converts objects stored in metadata or elsewhere to JSON.
    /// </summary>
    /// <remarks>
    /// Generates JSON for a specified object (which can come from document metadata or elsewhere)
    /// and stores it as new content for each input document or in each document's metadata.
    /// </remarks>
    /// <category>Content</category>
    public class GenerateJson : IModule
    {
        private readonly DocumentConfig<object> _data;
        private readonly string _destinationKey;
        private bool _indenting = true;
        private bool _camelCase = false;
        private Action<JsonSerializerSettings> _settings = null;

        /// <summary>
        /// The object stored in metadata at the specified key is converted to JSON, which then either
        /// replaces the content of each input document or is stored in the specified metadata key.
        /// </summary>
        /// <param name="sourceKey">The metadata key of the object to convert to JSON.</param>
        /// <param name="destinationKey">The metadata key where the JSON should be stored (or <c>null</c>
        /// to replace the content of each input document).</param>
        public GenerateJson(DocumentConfig<string> sourceKey, string destinationKey = null)
        {
            if (sourceKey == null)
            {
                throw new ArgumentNullException(nameof(sourceKey));
            }
            _destinationKey = destinationKey;
            _data = Config.FromDocument(async (doc, ctx) => doc.Get(await sourceKey.GetValueAsync(doc, ctx)));
        }

        /// <summary>
        /// The object returned by the specified delegate is converted to JSON, which then either
        /// replaces the content of each input document or is stored in the specified metadata key.
        /// </summary>
        /// <param name="data">A delegate that returns the object to convert to JSON.</param>
        /// <param name="destinationKey">The metadata key where the JSON should be stored (or <c>null</c>
        /// to replace the content of each input document).</param>
        public GenerateJson(DocumentConfig<object> data, string destinationKey = null)
        {
            _destinationKey = destinationKey;
            _data = data ?? throw new ArgumentNullException(nameof(data));
        }

        /// <summary>
        /// Allows you to specify metadata keys for each input document that should be serialized as properties in a JSON object.
        /// </summary>
        /// <param name="keys">The metadata keys to serialize as properties.</param>
        /// <param name="destinationKey">The metadata key where the JSON should be stored (or <c>null</c>
        /// to replace the content of each input document).</param>
        public GenerateJson(DocumentConfig<IEnumerable<string>> keys, string destinationKey = null)
        {
            if (keys == null)
            {
                throw new ArgumentNullException(nameof(keys));
            }
            _destinationKey = destinationKey;
            _data = Config.FromDocument(async (doc, ctx) => (await keys.GetValueAsync(doc, ctx)).Where(k => doc.ContainsKey(k)).ToDictionary(k => k, k => doc[k]));
        }

        /// <summary>
        /// Specifies whether the generated JSON should be indented.
        /// </summary>
        /// <param name="indenting">If set to <c>true</c>, the JSON is indented.</param>
        /// <returns>The current module instance.</returns>
        public GenerateJson WithIndenting(bool indenting = true)
        {
            _indenting = indenting;
            return this;
        }

        /// <summary>
        /// Specifies whether the generated JSON should use a camel case naming strategy for property names.
        /// The default behavior is not to generate camel case property names.
        /// </summary>
        /// <param name="camelCase">If set to <c>true</c>, camel case property names are generated.</param>
        /// <returns>The current module instance.</returns>
        public GenerateJson WithCamelCase(bool camelCase = true)
        {
            _camelCase = camelCase;
            return this;
        }

        /// <summary>
        /// Allows changing the JSON serializer settings.
        /// </summary>
        /// <param name="settings">An action that manipulates the serializer settings.</param>
        /// <returns>The current module instance.</returns>
        public GenerateJson WithSettings(Action<JsonSerializerSettings> settings)
        {
            _settings = settings;
            return this;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<IDocument>> ExecuteAsync(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            // Don't use the built-in exception tracing so that we can return the original document on error
            return await inputs.ParallelSelectAsync(GenerateJsonAsync);

            async Task<IDocument> GenerateJsonAsync(IDocument input)
            {
                try
                {
                    object data = await _data.GetValueAsync(input, context);
                    if (data != null)
                    {
                        JsonSerializerSettings settings = new JsonSerializerSettings()
                        {
                            Formatting = _indenting ? Formatting.Indented : Formatting.None
                        };
                        if (_camelCase)
                        {
                            settings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                        }
                        _settings?.Invoke(settings);
                        string result = JsonConvert.SerializeObject(data, settings);
                        if (string.IsNullOrEmpty(_destinationKey))
                        {
                            return context.GetDocument(input, await context.GetContentProviderAsync(result));
                        }
                        return context.GetDocument(input, new MetadataItems
                        {
                            { _destinationKey, result }
                        });
                    }
                }
                catch (Exception ex)
                {
                    Trace.Error("Error serializing JSON for {0}: {1}", input.Source.ToDisplayString(), ex.ToString());
                }
                return input;
            }
        }
    }
}
