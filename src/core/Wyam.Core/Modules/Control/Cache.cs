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

namespace Wyam.Core.Modules.Control
{
    // Should not contain modules that aggregate documents like Tree - put those after the cacheable operation
    public class Cache : ContainerModule, IDisposable
    {
        private Dictionary<FilePath, CacheItem> _cache = null;

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
                foreach (CacheItem cacheItem in _cache.Values)
                {
                    cacheItem.Dispose();
                }
                _cache = null;
            }
        }

        /// <inheritdoc />
        public override async Task<IEnumerable<IDocument>> ExecuteAsync(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            List<IDocument> misses = new List<IDocument>();
            List<IDocument> outputs = new List<IDocument>();

            // Need to track misses by their source so we can calculate the input document hashes that resulted in outputs
            List<IGrouping<FilePath, IDocument>> missesBySource;

            // Creating a new cache and swapping is the easiest way to expire old entries
            Dictionary<FilePath, CacheItem> currentCache = _cache;
            _cache = new Dictionary<FilePath, CacheItem>();

            // Check for hits and misses
            if (currentCache == null)
            {
                // If the current cache is null, this is the first run through
                misses.AddRange(inputs);
                missesBySource = inputs
                    .Where(x => x.Source != null)
                    .GroupBy(x => x.Source)
                    .ToList();
            }
            else
            {
                // Note that due to cloning we could have multiple inputs and outputs with the same source
                // so we need to check all inputs with a given source and only consider a hit when they all match
                missesBySource = new List<IGrouping<FilePath, IDocument>>();
                foreach (IGrouping<FilePath, IDocument> inputsBySource in inputs.GroupBy(x => x.Source))
                {
                    // Documents with a null source are never cached
                    if (inputsBySource.Key != null)
                    {
                        // Get the cache entry for this source
                        if (currentCache.TryGetValue(inputsBySource.Key, out CacheItem cacheItem))
                        {
                            // Check that all inputs with this source matched the cached inputs
                            if (inputsBySource.Count() == cacheItem.InputHashes.Length)
                            {
                                // Iterate one at a time so we can break if any don't match
                                List<int> cachedHashes = cacheItem.InputHashes.ToList();
                                foreach (IDocument input in inputsBySource)
                                {
                                    // Easiest way to check since there might be duplicate hashes is to remove matching hashes one at a time
                                    int hash = await input.GetHashAsync();
                                    if (!cachedHashes.Remove(hash))
                                    {
                                        // Couldn't find a matching hash value so the whole group is a miss
                                        break;
                                    }
                                }

                                // If we removed all cached hashes then this source group is a cache hit
                                if (cachedHashes.Count == 0)
                                {
                                    _cache.Add(inputsBySource.Key, cacheItem);
                                    outputs.AddRange(cacheItem.ResultDocuments);
                                    continue;  // Go to the next source group since misses are dealt with below
                                }

                                // If a miss and we previously cached results, dispose cached results if we took over tracking
                                cacheItem.Dispose();
                            }
                        }

                        // Miss with non-null source: track source and input documents so we can cache for next pass
                        missesBySource.Add(inputsBySource);
                    }

                    // Miss: add inputs to execute
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
            foreach (IGrouping<FilePath, IDocument> inputGroup in missesBySource)
            {
                // These are the hashes of every input document with the same source as these output documents
                int[] inputHashes = await inputGroup.SelectAsync(x => x.GetHashAsync()).ToArrayAsync();

                // Did we get any results from this input source?
                if (resultsBySource.TryGetValue(inputGroup.Key, out IGrouping<FilePath, IDocument> sourceResults))
                {
                    // Note whether the result document was being tracked (and is now tracked/disposed by the cache)
                    (IDocument Document, bool Tracking)[] resultDocuments = sourceResults.Select(x => (x, context.Untrack(x))).ToArray();

                    // We also need to track any child documents in the metadata of results so they don't get disposed by the context
                    HashSet<IDocument> flattenedDocuments = new HashSet<IDocument>();
                    foreach (IDocument resultDocument in sourceResults)
                    {
                        resultDocument.Flatten(flattenedDocuments);
                    }
                    IDocument[] childDocuments = flattenedDocuments.Where(x => !sourceResults.Contains(x) && context.Untrack(x)).ToArray();

                    _cache.Add(inputGroup.Key, new CacheItem(inputHashes, resultDocuments, childDocuments));
                }
                else
                {
                    // No results for this source
                    _cache.Add(inputGroup.Key, new CacheItem(inputHashes, Array.Empty<(IDocument, bool)>(), Array.Empty<IDocument>()));
                }
            }

            return outputs;
        }

        public class CacheItem : IDisposable
        {
            private readonly (IDocument Document, bool Tracking)[] _resultDocuments;
            private readonly IDocument[] _childDocuments;

            public CacheItem(int[] inputHashes, (IDocument, bool)[] resultDocuments, IDocument[] childDocuments)
            {
                InputHashes = inputHashes;
                _resultDocuments = resultDocuments;
                _childDocuments = childDocuments;
            }

            public int[] InputHashes { get; }

            public IEnumerable<IDocument> ResultDocuments => _resultDocuments.Select(x => x.Document);

            public void Dispose() =>
                Parallel.ForEach(
                    _resultDocuments.Where(x => x.Tracking).Select(x => x.Document).Concat(_childDocuments),
                    document => document.Dispose());
        }
    }
}
