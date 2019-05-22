using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Wyam.Common;
using Wyam.Common.Documents;
using Wyam.Common.Execution;
using Wyam.Common.IO;
using Wyam.Common.Modules;
using Wyam.Common.Tracing;

namespace Wyam.Core.Modules.Control
{
    // Should not contain modules that aggregate documents like Tree - put those after the cacheable operation
    public class Cache : ContainerModule, IDisposable
    {
        private Dictionary<FilePath, CacheEntry> _cache = null;

        public Cache(params IModule[] modules)
            : this((IEnumerable<IModule>)modules)
        {
        }

        public Cache(IEnumerable<IModule> modules)
            : base(modules)
        {
        }

        public void Dispose()
        {
            // Dipose any documents we've been holding on to
            if (_cache != null)
            {
                foreach (IDocument document in _cache.Values.SelectMany(x => x.Documents.Where(doc => doc.Tracked).Select(doc => doc.Document)))
                {
                    document.Dispose();
                }
                _cache = null;
            }
        }

        /// <inheritdoc />
        public override async Task<IEnumerable<IDocument>> ExecuteAsync(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            List<IDocument> misses = new List<IDocument>();
            List<IDocument> outputs = new List<IDocument>();

            // Need to track misses by their source and map it to the aggregate input document hash
            // Go ahead and calculate a single hash for all input documents since we'd end up having to
            // get each individual hash anyway, whether while checking for a complete hit or recording for the next time
            Dictionary<FilePath, int> missesBySource = new Dictionary<FilePath, int>();

            // Creating a new cache and swapping is the easiest way to expire old entries
            Dictionary<FilePath, CacheEntry> currentCache = _cache;
            _cache = new Dictionary<FilePath, CacheEntry>();

            // Keep track of invalidated cache items but don't dispose their documents until we're done
            // in case some of the documents get used in other cache items
            List<CacheEntry> invalidated = new List<CacheEntry>();

            // Check for hits and misses
            if (currentCache == null)
            {
                // If the current cache is null, this is the first run through
                misses.AddRange(inputs);
                IEnumerable<IGrouping<FilePath, IDocument>> inputGroups = inputs
                    .Where(x => x.Source != null)
                    .GroupBy(x => x.Source);
                foreach (IGrouping<FilePath, IDocument> inputGroup in inputGroups)
                {
                    missesBySource.Add(inputGroup.Key, await CombineCacheHashCodesAsync(inputGroup));
                }
            }
            else
            {
                // Note that due to cloning we could have multiple inputs and outputs with the same source
                // so we need to check all inputs with a given source and only consider a hit when they all match
                foreach (IGrouping<FilePath, IDocument> inputsBySource in inputs.GroupBy(x => x.Source))
                {
                    string message = null;

                    // Documents with a null source are never cached
                    if (inputsBySource.Key != null)
                    {
                        // Get the input hash (we need it one way or the other to check if it's a hit or to record for next time)
                        int inputHash = await CombineCacheHashCodesAsync(inputsBySource);

                        // Get the cache entry for this source if there is one
                        if (currentCache.TryGetValue(inputsBySource.Key, out CacheEntry entry))
                        {
                            // If the aggregate hash matches then it's a hit
                            if (inputHash == entry.InputHash)
                            {
                                Trace.Verbose($"Cache hit for {inputsBySource.Key}, using cached results");
                                _cache.Add(inputsBySource.Key, entry);
                                outputs.AddRange(entry.Documents.Where(x => x.IsResult).Select(x => x.Document));
                                continue;  // Go to the next source group since misses are dealt with below
                            }

                            // If a miss and we previously cached results, dispose cached results if we took over tracking
                            message = $"Cache miss for {inputsBySource.Key}, one or more documents have changed";
                            invalidated.Add(entry);
                        }

                        // Miss with non-null source: track source and input documents so we can cache for next pass
                        message = $"Cache miss for {inputsBySource.Key}, source not cached";
                        missesBySource.Add(inputsBySource.Key, inputHash);
                    }

                    // Miss, add inputs to execute
                    Trace.Verbose(message ?? "Cache miss for null source, null sources are never cached");
                    misses.AddRange(inputsBySource);
                }
            }

            // Execute misses
            ImmutableArray<IDocument> results = misses.Count == 0
                ? ImmutableArray<IDocument>.Empty
                : await context.ExecuteAsync(Children, misses);
            Dictionary<FilePath, IGrouping<FilePath, IDocument>> resultsBySource =
                results.GroupBy(x => x.Source).ToDictionary(x => x.Key, x => x);
            outputs.AddRange(results);

            // Cache all miss sources, even if they resulted in an empty result document set
            foreach (KeyValuePair<FilePath, int> inputGroup in missesBySource)
            {
                // Did we get any results from this input source?
                CachedDocument[] cacheDocuments = null;
                if (resultsBySource.TryGetValue(inputGroup.Key, out IGrouping<FilePath, IDocument> sourceResults))
                {
                    // Note whether the result document was being tracked (and is now tracked/disposed by the cache)
                    IEnumerable<CachedDocument> resultDocuments = sourceResults.Select(x => new CachedDocument(x, context.Untrack(x), true)).ToArray();

                    // We also need to track any child documents in the metadata of results so they don't get disposed by the context
                    HashSet<IDocument> flattenedDocuments = new HashSet<IDocument>();
                    foreach (IDocument resultDocument in sourceResults)
                    {
                        resultDocument.Flatten(flattenedDocuments);
                    }
                    cacheDocuments = resultDocuments
                        .Concat(flattenedDocuments.Where(x => !sourceResults.Contains(x)).Select(x => new CachedDocument(x, context.Untrack(x), false)))
                        .ToArray();
                }

                // Add a cache entry
                _cache.Add(inputGroup.Key, new CacheEntry(inputGroup.Value, cacheDocuments));
            }

            // Dispose invalidated cache entries
            foreach (CacheEntry invalidatedEntry in invalidated)
            {
                foreach (IDocument disposingDocument in invalidatedEntry.Documents.Where(x => x.Tracked))
                {
                    // Try to find another existing cached document
                    CachedDocument cachedDocument = null;
                    foreach (CacheEntry cacheEntry in _cache.Values)
                    {
                        cachedDocument = Array.Find(cacheEntry.Documents, x => x.Document.Equals(disposingDocument));
                        if (cachedDocument != null)
                        {
                            break;
                        }
                    }

                    // If we found this document still in use, pass on tracking responsibility
                    // Otherwise dispose the document
                    if (cachedDocument != null)
                    {
                        cachedDocument.Tracked = true;
                    }
                    else
                    {
                        disposingDocument.Dispose();
                    }
                }
            }

            return outputs;
        }

        private static async Task<int> CombineCacheHashCodesAsync(IEnumerable<IDocument> documents)
        {
            HashCode hashCode = default;
            foreach (IDocument document in documents)
            {
                hashCode.Add(await document.GetCacheHashCodeAsync());
            }
            return hashCode.ToHashCode();
        }

        private class CacheEntry
        {
            public CacheEntry(int inputHash, CachedDocument[] documents)
            {
                InputHash = inputHash;
                Documents = documents ?? Array.Empty<CachedDocument>();
            }

            public int InputHash { get; }

            public CachedDocument[] Documents { get; }
        }

        private class CachedDocument
        {
            public CachedDocument(IDocument document, bool tracked, bool isResult)
            {
                Document = document;
                Tracked = tracked;
                IsResult = isResult;
            }

            public IDocument Document { get; }

            // Could get switched on if the tracking cache entry disposes but it's still in use in an active cache entry
            public bool Tracked { get; set; }

            // If false, this is a child document of the result
            public bool IsResult { get; }
        }
    }
}
