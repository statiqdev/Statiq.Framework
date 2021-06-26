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
        public static readonly string DefaultRefKey = "ref"; // Can't use "id" because IDocument.Id will override it

        // TODO: Allow position metadata flag
        // TODO: Strip HTML from default content field only when media type is HTML

        // The keys in the search metadata objects to use for fields
        private readonly Dictionary<string, FieldType> _fieldKeys = new Dictionary<string, FieldType>()
        {
            { "link", FieldType.EagerLoad },
            { "title", FieldType.Searchable | FieldType.EagerLoad },
            { "content", FieldType.Searchable }
        };

        // Includes the host in the default link field
        private bool _includeHostInLink = false;

        // The key in the search metadata object to use for the ref
        private string _refKey = DefaultRefKey;

        // A search metadata object - only keys in _fieldKeys will be used, all others will be ignored
        private Config<IEnumerable<IMetadata>> _getSearchMetadata;

        private NormalizedPath _scriptPath = DefaultScriptPath;

        // The destination path of the index file, will be "[_scriptDestinationPath].gz" if null
        private NormalizedPath _indexPath = NormalizedPath.Null;

        private Func<StringBuilder, IExecutionContext, string> _customizeScript = null;

        private string _clientName = DefaultClientName;

        public GenerateLunrIndex()
        {
            // TODO: Get IMetadata from the specified key if it's available, only do fallback if not
            _getSearchMetadata = Config.FromDocument(async doc =>
                (IEnumerable<IMetadata>)new IMetadata[]
                {
                    doc.Clone(new MetadataItems
                    {
                        { _refKey, await doc.GetCacheCodeAsync() },
                        { "link", doc.GetLink(_includeHostInLink) },
                        { "title", doc.GetTitle() },
                        { "content", await doc.GetContentStringAsync() }
                    })
                });
        }

        public GenerateLunrIndex DefineField(string key, FieldType fieldType)
        {
            key.ThrowIfNullOrEmpty(nameof(key));
            _fieldKeys[key.ToLowerCamelCase()] = fieldType;
            return this;
        }

        public GenerateLunrIndex RemoveField(string key)
        {
            key.ThrowIfNullOrEmpty(nameof(key));
            _fieldKeys.Remove(key.ToLowerCamelCase());
            return this;
        }

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

        protected override async Task<IEnumerable<IDocument>> ExecuteContextAsync(IExecutionContext context)
        {
            Dictionary<string, Dictionary<string, object>> eagerDictionaries = new Dictionary<string, Dictionary<string, object>>();
            Dictionary<string, Dictionary<string, object>> lazyDictionaries = new Dictionary<string, Dictionary<string, object>>();
            string camelCaseRefKey = _refKey.ToLowerCamelCase();
            global::Lunr.Index searchIndex = await global::Lunr.Index.Build(async indexBuilder =>
            {
                // Add fields
                indexBuilder.ReferenceField = camelCaseRefKey;
                foreach (KeyValuePair<string, FieldType> fieldKey in _fieldKeys.Where(x => x.Value.HasFlag(FieldType.Searchable)))
                {
                    indexBuilder.AddField(fieldKey.Key);
                }

                // Iterate the input documents
                // TODO: Skip document if the hide from search flag is set
                foreach (IDocument input in context.Inputs)
                {
                    // Omit documents that shouldn't be processed
                    if (!input.GetBool(LunrKeys.OmitFromSearch))
                    {
                        // Get the search metadata for this input document
                        IEnumerable<IMetadata> searchMetadataItems = _getSearchMetadata is object
                            ? await _getSearchMetadata.GetValueAsync(input, context)
                            : new IMetadata[]
                            {
                                // Clone the original input document by default so any additional metadata it contains can be added to the field keys without problems
                                input.Clone(new MetadataItems
                                {
                                    { _refKey, await input.GetCacheCodeAsync() },
                                    { "link", input.GetLink(_includeHostInLink) },
                                    { "title", input.GetTitle() },
                                    { "content", await input.GetContentStringAsync() }
                                })
                            };
                        if (searchMetadataItems is object)
                        {
                            foreach (IMetadata searchMetadata in searchMetadataItems)
                            {
                                // Create the search document and data dictionaries
                                global::Lunr.Document searchDocument = new global::Lunr.Document();
                                Dictionary<string, object> eagerDictionary = new Dictionary<string, object>();
                                Dictionary<string, object> lazyDictionary = new Dictionary<string, object>();

                                // Get the reference value and only add a search item if we have one
                                string refValue = searchMetadata.GetString(_refKey);
                                if (!refValue.IsNullOrEmpty())
                                {
                                    // Add the reference value
                                    // We only need to add it to the search document, the eager document object is keyed by reference value and the lazy file name is the reference value
                                    searchDocument.Add(camelCaseRefKey, refValue);

                                    // Iterate fields and populate the search document and data dictionaries
                                    bool hasEagerData = false;
                                    bool hasLazyData = false;
                                    foreach (KeyValuePair<string, FieldType> fieldKey in _fieldKeys.OrderBy(x => x.Key))
                                    {
                                        object searchValue = searchMetadata.Get(fieldKey.Key);
                                        if (searchValue is object)
                                        {
                                            // Add to the search document
                                            if (fieldKey.Value.HasFlag(FieldType.Searchable))
                                            {
                                                // TODO: Test different types of search values from document metadata like int, bool, int[], string[], etc.
                                                searchDocument.Add(fieldKey.Key, searchValue);
                                            }

                                            // Add to the data dictionaries
                                            // TODO: Test different types of search values and JSON deserialization
                                            if (fieldKey.Value.HasFlag(FieldType.EagerLoad))
                                            {
                                                eagerDictionary.Add(fieldKey.Key, searchValue);
                                                hasEagerData = true;
                                            }
                                            if (fieldKey.Value.HasFlag(FieldType.LazyLoad))
                                            {
                                                lazyDictionary.Add(fieldKey.Key, searchValue);
                                                hasLazyData = true;
                                            }
                                        }
                                    }

                                    // Add the search document and data dictionaries
                                    await indexBuilder.Add(searchDocument, cancellationToken: context.CancellationToken);
                                    if (hasEagerData)
                                    {
                                        eagerDictionaries.Add(refValue, eagerDictionary);
                                    }
                                    if (hasLazyData)
                                    {
                                        lazyDictionaries.Add(refValue, lazyDictionary);
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

            // Build the search JavaScript file, allowing for overriding the output
            StringBuilder scriptBuilder = new StringBuilder();
            scriptBuilder.Append($@"const {_clientName} {{
    indexFile: '{context.GetLink(indexPath)}',
    documents: {System.Text.Json.JsonSerializer.Serialize(eagerDictionaries)}
}};");
            string script = _customizeScript is object ? _customizeScript.Invoke(scriptBuilder, context) : scriptBuilder.ToString();
            if (!script.IsNullOrEmpty())
            {
                // Only output the script if it wasn't overridden to null or empty
                outputs.Add(context.CreateDocument(_scriptPath, context.GetContentProvider(script, MediaTypes.JavaScript)));
            }

            return outputs;
        }
    }
    [Flags]
    public enum FieldType
    {
        /// <summary>
        /// The field is searchable.
        /// </summary>
        Searchable = 1,

        /// <summary>
        /// Field content will be lazily-loaded from an external JSON file.
        /// </summary>
        LazyLoad = 2,

        /// <summary>
        /// Field content will be eagerly loaded when the search index is initialized on the client.
        /// </summary>
        EagerLoad = 4
    }
}
