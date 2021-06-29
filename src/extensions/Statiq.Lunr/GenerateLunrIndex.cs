using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Statiq.Common;

namespace Statiq.Lunr
{
    public class GenerateLunrIndex : Module
    {
        public static readonly NormalizedPath DefaultScriptPath = new NormalizedPath("search.js");
        public static readonly string DefaultClientName = "search";
        public static readonly string DefaultReferenceKey = "ref"; // Can't use "id" because IDocument.Id will override it

        // TODO: Allow position metadata flag
        // TODO: Strip HTML from default content field only when media type is HTML

        // The keys in the search metadata objects to use for fields
        private readonly Dictionary<string, FieldType> _fieldKeys = new Dictionary<string, FieldType>()
        {
            { "link", FieldType.Result },
            { "title", FieldType.Searchable | FieldType.Result },
            { "content", FieldType.Searchable }
        };

        // Includes the host in the default link field
        private bool _includeHostInLink;

        // The key in the search metadata object to use for the ref
        private string _referenceKey = DefaultReferenceKey;

        // A search metadata object - only keys in _fieldKeys will be used, all others will be ignored
        private Config<IEnumerable<IEnumerable<KeyValuePair<string, object>>>> _getSearchItems;

        private NormalizedPath _scriptPath = DefaultScriptPath;

        // The destination path of the index file, will be "[_scriptDestinationPath].gz" if null
        private NormalizedPath _indexPath = NormalizedPath.Null;

        // The destination path of the results file, will be "[_scriptDestinationPath].json" if null
        private NormalizedPath _resultsPath = NormalizedPath.Null;

        private Func<StringBuilder, IExecutionContext, string> _customizeScript;

        private string _clientName = DefaultClientName;

        private string _searchItemsKey = LunrKeys.SearchItems;

        /// <summary>
        /// Defines a search field and whether to include it in results.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The key corresponds to a key from the items returned from
        /// <see cref="WithSearchItems(Config{IEnumerable{IEnumerable{KeyValuePair{string, object}}}})"/>,
        /// or from the input documents by default.
        /// </para>
        /// <para>
        /// Including the field in the results (<see cref="FieldType.Result"/>) increases
        /// the size of the JSON file that contains result data, but allows using the value
        /// of the field from the client. Otherwise, just specifying <see cref="FieldType.Searchable"/>
        /// will allow searching the field value but not using it from the client. You can
        /// also specify both since <see cref="FieldType"/> is a <see cref="FlagsAttribute"/> enum.
        /// </para>
        /// </remarks>
        /// <param name="key">The key that holds the search value.</param>
        /// <param name="fieldType">The type of field.</param>
        /// <returns>The current module instance.</returns>
        public GenerateLunrIndex DefineField(string key, FieldType fieldType)
        {
            key.ThrowIfNullOrEmpty(nameof(key));
            _fieldKeys[key.ToLowerCamelCase()] = fieldType;
            return this;
        }

        /// <summary>
        /// Removes a field from the search index.
        /// </summary>
        /// <param name="key">The key to remove.</param>
        /// <returns>The current module instance.</returns>
        public GenerateLunrIndex RemoveField(string key)
        {
            key.ThrowIfNullOrEmpty(nameof(key));
            _fieldKeys.Remove(key.ToLowerCamelCase());
            return this;
        }

        /// <summary>
        /// Clears all fields from the search index.
        /// </summary>
        /// <returns>The current module instance.</returns>
        public GenerateLunrIndex ClearFields()
        {
            _fieldKeys.Clear();
            return this;
        }

        /// <summary>
        /// Indicates whether the host should be automatically included
        /// in generated links (the default is <c>false</c>).
        /// </summary>
        /// <param name="includeHostInLink"><c>true</c> to include the host.</param>
        /// <returns>The current module instance.</returns>
        public GenerateLunrIndex IncludeHostInLink(bool includeHostInLink = true)
        {
            _includeHostInLink = includeHostInLink;
            return this;
        }

        /// <summary>
        /// This allows you to customize the JavaScript file that this module creates.
        /// </summary>
        /// <param name="customizeScript">
        /// A script transformation function. The <see cref="StringBuilder"/> contains
        /// the generated script content. You can manipulate as appropriate and then return the final
        /// script as a <c>string</c>. If you return <c>null</c> then no script will be output (only
        /// the index file will be output).
        /// </param>
        /// <returns>The current module instance.</returns>
        public GenerateLunrIndex CustomizeScript(Func<StringBuilder, IExecutionContext, string> customizeScript)
        {
            _customizeScript = customizeScript;
            return this;
        }

        /// <summary>
        /// Controls the output path of the script file (by default the
        /// destination of the script file is "search.js").
        /// </summary>
        /// <param name="scriptPath">The script path.</param>
        /// <returns>The current module instance.</returns>
        public GenerateLunrIndex WithScriptPath(in NormalizedPath scriptPath)
        {
            _scriptPath = scriptPath.ThrowIfNull(nameof(scriptPath));
            return this;
        }

        /// <summary>
        /// Controls the output path of the search index file (by default the
        /// destination of the search index file is the same as the script file with a ".gz" extension).
        /// </summary>
        /// <param name="indexPath">The search index path.</param>
        /// <returns>The current module instance.</returns>
        public GenerateLunrIndex WithIndexPath(in NormalizedPath indexPath)
        {
            _indexPath = indexPath;
            return this;
        }

        /// <summary>
        /// Controls the output path of the results file that holds search field values (I.e. <see cref="FieldType.Result"/>).
        /// </summary>
        /// <param name="resultsPath">The results file path.</param>
        /// <returns>The current module instance.</returns>
        public GenerateLunrIndex WithResultsPath(in NormalizedPath resultsPath)
        {
            _resultsPath = resultsPath;
            return this;
        }

        /// <summary>
        /// Use a custom delegate to get search items for each input document. Only the reference and search field keys
        /// will be retrieved from the returned item(s), all other values will be ignored (I.e. they won't be added to
        /// the search automatically).
        /// </summary>
        /// <param name="getSearchItems">A delegate that gets search items.</param>
        /// <returns>The current module instance.</returns>
        public GenerateLunrIndex WithSearchItems(Config<IEnumerable<IEnumerable<KeyValuePair<string, object>>>> getSearchItems)
        {
            _getSearchItems = getSearchItems;
            return this;
        }

        /// <summary>
        /// Sets an alternate reference key that will be used to get a unique identifier for each search metadata item.
        /// </summary>
        /// <param name="referenceKey">The reference key to use.</param>
        /// <returns>The current module instance.</returns>
        public GenerateLunrIndex WithReferenceKey(string referenceKey)
        {
            _referenceKey = referenceKey.ThrowIfNullOrEmpty(nameof(referenceKey));
            return this;
        }

        /// <summary>
        /// Sets a custom metadata key to use to get search items from each document if the document provides search items via metadata.
        /// </summary>
        /// <param name="searchItemsKey">The search items key to use.</param>
        /// <returns>The current module instance.</returns>
        public GenerateLunrIndex WithSearchItemsKey(string searchItemsKey)
        {
            _searchItemsKey = searchItemsKey.ThrowIfNullOrEmpty(nameof(searchItemsKey));
            return this;
        }

        protected override async Task<IEnumerable<IDocument>> ExecuteContextAsync(IExecutionContext context)
        {
            Dictionary<string, Dictionary<string, object>> resultDictionaries = new Dictionary<string, Dictionary<string, object>>();
            Dictionary<string, Dictionary<string, object>> lazyDictionaries = new Dictionary<string, Dictionary<string, object>>();
            string camelCaseRefKey = _referenceKey.ToLowerCamelCase();
            bool addedFields = false;
            global::Lunr.Index searchIndex = await global::Lunr.Index.Build(async indexBuilder =>
            {
                // Iterate the input documents
                foreach (IDocument input in context.Inputs)
                {
                    // Omit documents that shouldn't be processed
                    if (!input.GetBool(LunrKeys.OmitFromSearch))
                    {
                        IEnumerable<IEnumerable<KeyValuePair<string, object>>> searchItems = _getSearchItems is object
                            ? await _getSearchItems.GetValueAsync(input, context)
                            : await DefaultGetSearchItemsAsync(input);
                        if (searchItems is object)
                        {
                            foreach (IEnumerable<KeyValuePair<string, object>> searchItem in searchItems)
                            {
                                if (searchItem is object)
                                {
                                    // Clone the original input document so any additional metadata it contains can be added to the field keys without problems
                                    IDocument searchDocument = input.Clone(searchItem);

                                    // Create the search document and data dictionaries
                                    global::Lunr.Document lunrDocument = new global::Lunr.Document();
                                    Dictionary<string, object> resultDictionary = new Dictionary<string, object>();

                                    // Get the reference value and only add a search item if we have one
                                    string refValue = searchDocument.GetString(_referenceKey);
                                    if (!refValue.IsNullOrEmpty())
                                    {
                                        // Add the reference value
                                        // We only need to add it to the search document, the eager document object is keyed by reference value and the lazy file name is the reference value
                                        lunrDocument.Add(camelCaseRefKey, refValue);

                                        // Iterate fields and populate the search document and data dictionaries
                                        bool hasResultField = false;
                                        foreach (KeyValuePair<string, FieldType> fieldKey in _fieldKeys.OrderBy(x => x.Key))
                                        {
                                            // Convert to either an array of strings or a single string
                                            object searchValue = searchDocument.Get(fieldKey.Key);
                                            if (!(searchValue is IEnumerable<string>))
                                            {
                                                searchValue = TypeHelper.Convert<string>(searchValue);
                                            }
                                            if (searchValue is object)
                                            {
                                                // Add to the search document
                                                if (fieldKey.Value.HasFlag(FieldType.Searchable))
                                                {
                                                    lunrDocument.Add(fieldKey.Key, searchValue);
                                                }

                                                // Add to the results dictionaries
                                                if (fieldKey.Value.HasFlag(FieldType.Result))
                                                {
                                                    resultDictionary.Add(fieldKey.Key, searchValue);
                                                    hasResultField = true;
                                                }
                                            }
                                        }

                                        // Add the fields if this is the first document (only add them when we know we have at least one document,
                                        // otherwise the search index build throws for an empty document set)
                                        if (!addedFields)
                                        {
                                            // Add fields
                                            indexBuilder.ReferenceField = camelCaseRefKey;
                                            foreach (KeyValuePair<string, FieldType> fieldKey in _fieldKeys.Where(x => x.Value.HasFlag(FieldType.Searchable)))
                                            {
                                                indexBuilder.AddField(fieldKey.Key);
                                            }
                                            addedFields = true;
                                        }

                                        // Add the search document and data dictionaries
                                        await indexBuilder.Add(lunrDocument, cancellationToken: context.CancellationToken);
                                        if (hasResultField)
                                        {
                                            resultDictionaries.Add(refValue, resultDictionary);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            });

            // Create output documents
            List<IDocument> outputs = new List<IDocument>();

            // Zip the index in a separate document
            byte[] indexBytes = Encoding.UTF8.GetBytes(searchIndex.ToJson());
            NormalizedPath indexPath = _indexPath.IsNullOrEmpty ? _scriptPath.ChangeExtension(".gz") : _indexPath;
            using (Stream indexStream = context.GetContentStream())
            {
                using (GZipStream zipStream = new GZipStream(indexStream, CompressionLevel.Optimal))
                {
                    await zipStream.WriteAsync(indexBytes, context.CancellationToken);
                }
                outputs.Add(context.CreateDocument(indexPath, context.GetContentProvider(indexStream, MediaTypes.Get(".gz"))));
            }

            // Output a result file (if we have any results)
            NormalizedPath resultsPath = _resultsPath.IsNullOrEmpty ? _scriptPath.ChangeExtension(".json") : _resultsPath;
            if (resultDictionaries.Count > 0)
            {
                outputs.Add(context.CreateDocument(resultsPath, context.GetContentProvider(System.Text.Json.JsonSerializer.Serialize(resultDictionaries), MediaTypes.Json)));
            }

            // Build the search JavaScript file, allowing for overriding the output
            StringBuilder scriptBuilder = new StringBuilder($@"const {_clientName} {{
    indexFile: '{context.GetLink(indexPath)}'");
            if (resultDictionaries.Count > 0)
            {
                scriptBuilder.Append($@",
    resultsFile: '{context.GetLink(resultsPath)}'");
            }
            scriptBuilder.Append(@"
}};");
            string script = _customizeScript is object ? _customizeScript.Invoke(scriptBuilder, context) : scriptBuilder.ToString();
            if (!script.IsNullOrEmpty())
            {
                // Only output the script if it wasn't overridden to null or empty
                outputs.Add(context.CreateDocument(_scriptPath, context.GetContentProvider(script, MediaTypes.JavaScript)));
            }

            return outputs;
        }

        private async Task<IEnumerable<IEnumerable<KeyValuePair<string, object>>>> DefaultGetSearchItemsAsync(IDocument input)
        {
            // Try to get search items from the metadata key
            IEnumerable<IEnumerable<KeyValuePair<string, object>>> searchItems = input.GetList<IEnumerable<KeyValuePair<string, object>>>(_searchItemsKey);
            if (searchItems is object)
            {
                return searchItems;
            }

            // Get the default search metadata for this input document
            Dictionary<string, object> searchItem = new Dictionary<string, object>
            {
                { "link", input.GetLink(_includeHostInLink) },
                { "title", input.GetTitle() },
                { "content", await input.GetContentStringAsync() }
            };
            if (input.GetString(_referenceKey) is null)
            {
                searchItem.Add(_referenceKey, await input.GetCacheCodeAsync());
            }
            return new IEnumerable<KeyValuePair<string, object>>[] { searchItem };
        }
    }
}
