using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Statiq.Common;

namespace Statiq.Lunr
{
    public class GenerateLunrIndex : Module
    {
        // TODO: Allow position metadata flag

        private readonly bool _includeHost = false;

        // The key in the search metadata object to use for the ref
        private readonly string _refKey = "id";

        // The keys in the search metadata objects to use for fields
        private readonly Dictionary<string, FieldType> _fieldKeys = new Dictionary<string, FieldType>
        {
            { "link", FieldType.EagerLoad },
            { "title", FieldType.Searchable | FieldType.EagerLoad },
            { "content", FieldType.Searchable }
        };

        // A search metadata object - only keys in _fieldKeys will be used, all others will be ignored
        // Clone the input document by default so any additional metadata it contains can be added to the field keys without problems
        private readonly Config<IEnumerable<IMetadata>> _getSearchMetadata;

        public GenerateLunrIndex()
        {
            _getSearchMetadata = Config.FromDocument(async doc =>
                (IEnumerable<IMetadata>)new IMetadata[]
                {
                    doc.Clone(new MetadataItems
                    {
                        { "id", await doc.GetCacheCodeAsync() },
                        { "link", doc.GetLink(_includeHost) },
                        { "title", doc.GetTitle() },
                        { "content", await doc.GetContentStringAsync() }
                    })
                });
        }

        protected override async Task<IEnumerable<IDocument>> ExecuteContextAsync(IExecutionContext context)
        {
            global::Lunr.Index searchIndex = await global::Lunr.Index.Build(async indexBuilder =>
            {
                // Add fields
                string camelCaseRefKey = _refKey.ToLowerCamelCase();
                indexBuilder.ReferenceField = camelCaseRefKey;
                foreach (KeyValuePair<string, FieldType> fieldKey in _fieldKeys.Where(x => x.Value.HasFlag(FieldType.Searchable)))
                {
                    indexBuilder.AddField(fieldKey.Key.ToLowerCamelCase());
                }

                // Iterate the input documents
                Dictionary<string, Dictionary<string, object>> eagerDictionaries = new Dictionary<string, Dictionary<string, object>>();
                Dictionary<string, Dictionary<string, object>> lazyDictionaries = new Dictionary<string, Dictionary<string, object>>();
                foreach (IDocument input in context.Inputs)
                {
                    IEnumerable<IMetadata> searchMetadataItems = await _getSearchMetadata.GetValueAsync(input, context);
                    if (searchMetadataItems is object)
                    {
                        foreach (IMetadata searchMetadata in searchMetadataItems)
                        {
                            // Create the search document and data dictionaries
                            global::Lunr.Document searchDocument = new global::Lunr.Document();
                            Dictionary<string, object> eagerDictionary = new Dictionary<string, object>();
                            Dictionary<string, object> lazyDictionary = new Dictionary<string, object>();

                            // Get the reference value and only add a search item if we have one
                            // TODO: Skip if the hide from search flag is set
                            string refValue = searchMetadata.GetString(_refKey);
                            if (!refValue.IsNullOrEmpty())
                            {
                                // Add the reference value
                                searchDocument.Add(camelCaseRefKey, refValue);
                                eagerDictionary.Add(camelCaseRefKey, refValue);
                                lazyDictionary.Add(camelCaseRefKey, refValue);

                                // Iterate fields and populate the search document and data dictionaries
                                foreach (KeyValuePair<string, FieldType> fieldKey in _fieldKeys)
                                {
                                    string camelCaseKey = fieldKey.Key.ToLowerCamelCase();
                                    object searchValue = searchMetadata.Get(fieldKey.Key);
                                    if (searchValue is object)
                                    {
                                        // Add to the search document
                                        if (fieldKey.Value.HasFlag(FieldType.Searchable))
                                        {
                                            // TODO: Test different types of search values from document metadata like int, bool, int[], string[], etc.
                                            searchDocument.Add(camelCaseKey, searchValue);
                                        }

                                        // Add to the data dictionaries
                                        // TODO: Test different types of search values and JSON deserialization
                                        if (fieldKey.Value.HasFlag(FieldType.EagerLoad))
                                        {
                                            eagerDictionary.Add(camelCaseKey, searchValue);
                                        }
                                        if (fieldKey.Value.HasFlag(FieldType.LazyLoad))
                                        {
                                            lazyDictionary.Add(camelCaseKey, searchValue);
                                        }
                                    }
                                }

                                // Add the search document and data dictionaries
                                await indexBuilder.Add(searchDocument, cancellationToken: context.CancellationToken);
                                eagerDictionaries.Add(refValue, eagerDictionary);
                                lazyDictionaries.Add(refValue, lazyDictionary);
                            }
                        }

                        // TODO: Create output documents
                    }
                }
            });

            return null;
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
