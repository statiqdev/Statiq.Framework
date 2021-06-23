using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Statiq.Common;

namespace Statiq.Lunr
{
    /// <summary>
    /// Generates a JavaScript-based search index from the input documents.
    /// </summary>
    /// <remarks>
    /// This module generates a search index that can be imported into the JavaScript <a href="http://lunrjs.com/">Lunr.js</a> search engine.
    /// </remarks>
    /// <example>
    /// The client-side JavaScript code for importing the search index should look something like this (assuming you have an HTML <c>input</c>
    /// with an ID of <c>#search</c> and a <c>div</c> with an ID of <c>#search-results</c>):
    /// <code>
    /// function runSearch(query) {
    ///     $("#search-results").empty();
    ///     if (query.length &lt; 2)
    ///     {
    ///         return;
    ///     }
    ///     var results = searchModule.search(query);
    ///     var listHtml = "&lt;ul&gt;";
    ///     listHtml += "&lt;li&gt;&lt;strong&gt;Search Results&lt;/strong&gt;&lt;/li&gt;";
    ///     if (results.length == 0)
    ///     {
    ///         listHtml += "&lt;li&gt;No results found&lt;/li&gt;";
    ///     }
    ///     else
    ///     {
    ///         for (var i = 0; i &lt; results.length; ++i)
    ///         {
    ///             var res = results[i];
    ///             listHtml += "&lt;li&gt;&lt;a href='" + res.url + "'&gt;" + res.title + "&lt;/a&gt;&lt;/li&gt;";
    ///         }
    ///     }
    ///     listHtml += "&lt;/ul&gt;";
    ///     $("#search-results").append(listHtml);
    /// }
    ///
    /// $(document).ready(function() {
    ///     $("#search").on('input propertychange paste', function() {
    ///         runSearch($("#search").val());
    ///     });
    /// });
    /// </code>
    /// </example>
    /// <metadata cref="LunrKeys.LunrIndexItem" usage="Input" />
    /// <metadata cref="LunrKeys.HideFromSearchIndex" usage="Input" />
    /// <category>Content</category>
    public class GenerateLunrIndex : Module
    {
        public static readonly NormalizedPath DefaultDestinationPath = new NormalizedPath("searchindex.js");

        private static readonly Regex StripHtmlAndSpecialChars = new Regex(@"<[^>]+>|&[a-zA-Z]{2,};|&#\d+;|[^a-zA-Z-#]", RegexOptions.Compiled);
        private readonly Config<ILunrIndexItem> _getSearchIndexItem;
        private NormalizedPath _stopwordsPath;
        private bool _enableStemming;
        private NormalizedPath _destination = DefaultDestinationPath;
        private bool _includeHost = false;
        private Func<StringBuilder, IExecutionContext, string> _script = (builder, _) => builder.ToString();

        /// <summary>
        /// Creates the search index by looking for a <see cref="LunrKeys.LunrIndexItem"/> metadata key in each input document.
        /// If no <see cref="LunrKeys.LunrIndexItem"/> metadata key is present in the document, default values for the document
        /// will be used to generate a search index item.
        /// </summary>
        /// <param name="stopwordsPath">A file to use that contains a set of stopwords.</param>
        /// <param name="enableStemming">If set to <c>true</c>, stemming is enabled.</param>
        public GenerateLunrIndex(in NormalizedPath stopwordsPath = default, bool enableStemming = false)
            : this(Config.FromDocument(doc => doc.Get<ILunrIndexItem>(LunrKeys.LunrIndexItem) ?? new LunrIndexDocItem(doc)), stopwordsPath, enableStemming)
        {
        }

        /// <summary>
        /// Creates the search index by looking for a specified metadata key in each input document that contains a <see cref="ILunrIndexItem"/> instance.
        /// If the corresponding metadata key is not available or does not contain a <see cref="ILunrIndexItem"/> instance, the document will be
        /// omitted from the search index.
        /// </summary>
        /// <param name="searchIndexItemMetadataKey">The metadata key that contains the <c>SearchIndexItem</c> instance.</param>
        /// <param name="stopwordsPath">A file to use that contains a set of stopwords.</param>
        /// <param name="enableStemming">If set to <c>true</c>, stemming is enabled.</param>
        public GenerateLunrIndex(string searchIndexItemMetadataKey, in NormalizedPath stopwordsPath = default, bool enableStemming = false)
            : this(Config.FromDocument(doc => doc.Get<ILunrIndexItem>(searchIndexItemMetadataKey)), stopwordsPath, enableStemming)
        {
        }

        /// <summary>
        /// Creates the search index by looking for a specified metadata key in each input document that contains a <see cref="ILunrIndexItem"/> instance.
        /// If the corresponding metadata key is not available or does not contain a <see cref="ILunrIndexItem"/> instance, the document will be
        /// omitted from the search index or default values will be used depending on the value of <paramref name="useDefaultValues"/>.
        /// </summary>
        /// <param name="searchIndexItemMetadataKey">The metadata key that contains the <c>SearchIndexItem</c> instance.</param>
        /// <param name="useDefaultValues">
        /// <c>true</c> to use default values if the specified key is not present in a document or does not contain a <see cref="ILunrIndexItem"/> instance,
        /// <c>false</c> to omit the document if the specified key is not present in a document or does not contain a <see cref="ILunrIndexItem"/> instance.
        /// </param>
        /// <param name="stopwordsPath">A file to use that contains a set of stopwords.</param>
        /// <param name="enableStemming">If set to <c>true</c>, stemming is enabled.</param>
        public GenerateLunrIndex(string searchIndexItemMetadataKey, bool useDefaultValues, in NormalizedPath stopwordsPath = default, bool enableStemming = false)
            : this(Config.FromDocument(doc => doc.Get<ILunrIndexItem>(searchIndexItemMetadataKey) ?? (useDefaultValues ? new LunrIndexDocItem(doc) : null)), stopwordsPath, enableStemming)
        {
        }

        /// <summary>
        /// Creates the search index by using a delegate that returns a <see cref="ILunrIndexItem"/> instance for each input document.
        /// </summary>
        /// <param name="searchIndexItem">A delegate that should return a <c>ISearchIndexItem</c>.</param>
        /// <param name="stopwordsPath">A file to use that contains a set of stopwords.</param>
        /// <param name="enableStemming">If set to <c>true</c>, stemming is enabled.</param>
        public GenerateLunrIndex(Config<ILunrIndexItem> searchIndexItem, in NormalizedPath stopwordsPath = default, bool enableStemming = false)
        {
            _getSearchIndexItem = searchIndexItem.ThrowIfNull(nameof(searchIndexItem));
            _stopwordsPath = stopwordsPath;
            _enableStemming = enableStemming;
        }

        /// <summary>
        /// Indicates whether the host should be automatically included in generated links.
        /// </summary>
        /// <param name="includeHost"><c>true</c> to include the host.</param>
        /// <returns>The current module instance.</returns>
        public GenerateLunrIndex IncludeHost(bool includeHost = true)
        {
            _includeHost = includeHost;
            return this;
        }

        /// <summary>
        /// Sets the path to a stopwords file.
        /// </summary>
        /// <param name="stopwordsPath">A file to use that contains a set of stopwords.</param>
        /// <returns>The current module instance.</returns>
        public GenerateLunrIndex WithStopwordsPath(in NormalizedPath stopwordsPath)
        {
            _stopwordsPath = stopwordsPath;
            return this;
        }

        /// <summary>
        /// Controls whether stemming is turned on.
        /// </summary>
        /// <param name="enableStemming">If set to <c>true</c>, stemming is enabled.</param>
        /// <returns>The current module instance.</returns>
        public GenerateLunrIndex EnableStemming(bool enableStemming = true)
        {
            _enableStemming = enableStemming;
            return this;
        }

        /// <summary>
        /// Controls the output path of the result document (by default the
        /// destination of the result document is "searchIndex.js").
        /// </summary>
        /// <param name="destination">The destination path.</param>
        /// <returns>The current module instance.</returns>
        public GenerateLunrIndex WithDestination(in NormalizedPath destination)
        {
            _destination = destination;
            return this;
        }

        /// <summary>
        /// This allows you to customize the Lunr.js JavaScript that this module creates.
        /// </summary>
        /// <param name="script">A script transformation function. The <see cref="StringBuilder"/> contains
        /// the generated script content. You can manipulate as appropriate and then return the final
        /// script as a <c>string</c>.</param>
        /// <returns>The current module instance.</returns>
        public GenerateLunrIndex WithScript(Func<StringBuilder, IExecutionContext, string> script)
        {
            _script = script.ThrowIfNull(nameof(script));
            return this;
        }

        /// <inheritdoc />
        protected override async Task<IEnumerable<IDocument>> ExecuteContextAsync(IExecutionContext context)
        {
            ILunrIndexItem[] searchIndexItems =
                await context.Inputs
                    .Where(x => !x.GetBool(LunrKeys.HideFromSearchIndex))
                    .ToAsyncEnumerable()
                    .SelectAwait(async x => await _getSearchIndexItem.GetValueAsync(x, context))
                    .Where(x => x is object && !(x?.Title).IsNullOrEmpty())
                    .ToArrayAsync();

            string[] stopwords = await GetStopwordsAsync(context);
            StringBuilder scriptBuilder = await BuildScriptAsync(searchIndexItems, stopwords, context);
            string script = _script(scriptBuilder, context);

            return context.CreateDocument(_destination, context.GetContentProvider(script, MediaTypes.Get(".js"))).Yield();
        }

        private async Task<StringBuilder> BuildScriptAsync(IList<ILunrIndexItem> searchIndexItems, string[] stopwords, IExecutionContext context)
        {
            StringBuilder scriptBuilder = new StringBuilder($@"
var searchModule = function() {{
    var documents = [];
    var idMap = [];
    function a(a,b) {{ 
        documents.push(a);
        idMap.push(b); 
    }}
");

            for (int i = 0; i < searchIndexItems.Count; ++i)
            {
                ILunrIndexItem indexItem = searchIndexItems[i];

                // Get the URL and skip if not valid
                string url = indexItem.GetLink(context, _includeHost);
                if (string.IsNullOrEmpty(url))
                {
                    continue;
                }

                scriptBuilder.Append($@"
    a(
        {{
            id:{i},
            title:{CleanString(indexItem.Title, stopwords)},
            content:{CleanString(await indexItem.GetContentAsync(), stopwords)},
            description:{CleanString(indexItem.Description, stopwords)},
            tags:'{indexItem.Tags}'
        }},
        {{
            url:'{url}',
            title:{ToJsonString(indexItem.Title)},
            description:{ToJsonString(indexItem.Description)}
        }}
    );");
            }

            scriptBuilder.Append($@"
    var idx = lunr(function() {{
        this.field('title');
        this.field('content');
        this.field('description');
        this.field('tags');
        this.ref('id');

        this.pipeline.remove(lunr.stopWordFilter);
        {(_enableStemming ? string.Empty : "this.pipeline.remove(lunr.stemmer);")}
        documents.forEach(function (doc) {{ this.add(doc) }}, this)
    }});
");
            scriptBuilder.AppendLine($@"
    return {{
        search: function(q) {{
            return idx.search(q).map(function(i) {{
                return idMap[i.ref];
            }});
        }}
    }};
}}();");

            return scriptBuilder;
        }

        private static string CleanString(string input, string[] stopwords)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return "''";
            }

            string clean = StripHtmlAndSpecialChars.Replace(input, " ").Trim();
            clean = Regex.Replace(clean, @"\s{2,}", " ");
            clean = string.Join(" ", clean.Split(' ').Where(f => f.Length > 1 && !stopwords.Contains(f, StringComparer.InvariantCultureIgnoreCase)).ToArray());
            clean = ToJsonString(clean);

            return clean;
        }

        private static string ToJsonString(string s) => Newtonsoft.Json.JsonConvert.ToString(s);

        private async Task<string[]> GetStopwordsAsync(IExecutionContext context)
        {
            string[] stopwords = new string[0];

            if (!_stopwordsPath.IsNull)
            {
                IFile stopwordsFile = context.FileSystem.GetInputFile(_stopwordsPath);
                if (stopwordsFile.Exists)
                {
                    stopwords = (await stopwordsFile.ReadAllTextAsync())
                        .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(f => f.Trim().ToLowerInvariant())
                        .Where(f => f.Length > 1)
                        .ToArray();
                }
            }

            return stopwords;
        }
    }
}
