using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Statiq.Common;

namespace Statiq.Core
{
    /// <summary>
    /// Caches documents between executions.
    /// </summary>
    /// <remarks>
    /// This module will execute child modules and cache the output documents. On following
    /// executions, the cached documents will be output if the input documents (as well as
    /// any other specified documents) match the original inputs (as defined by a hash of
    /// the document content and metadata). Using this module can greatly improve performance
    /// on re-execution after making changes in preview mode. In general, caching works best
    /// when there is a one-to-one relationship between input and output documents. Modules
    /// that aggregate documents into groups such as <see cref="CreateTree"/> should not be used as
    /// child modules of the cache module. Instead they should appear after the cache module
    /// and operate on the cached outputs.
    /// </remarks>
    /// <metadata cref="Keys.DisableCache" usage="Input" />
    /// <metadata cref="Keys.ResetCache" usage="Input" />
    /// <category>Control</category>
    public class CacheDocuments : ParentModule, IDisposable
    {
        private Dictionary<FilePath, CacheEntry> _cache = null;

        public CacheDocuments(params IModule[] modules)
            : base(modules)
        {
        }

        public void Dispose() => ResetCache();

        private void ResetCache() => _cache = null;

        /// <inheritdoc />
        public override async Task<IEnumerable<IDocument>> ExecuteAsync(IExecutionContext context)
        {
            // If we're disabling the cache, clear any existing entries and just execute children
            if (context.Settings.Bool(Keys.DisableCache))
            {
                ResetCache();
                return await context.ExecuteModulesAsync(Children, context.Inputs);
            }

            // If we're reseting the cache, reset it but then continue
            if (context.Settings.Bool(Keys.ResetCache))
            {
                ResetCache();
            }

            List<IDocument> misses = new List<IDocument>();
            List<IDocument> outputs = new List<IDocument>();

            // Need to track misses by their source and map it to the aggregate input document hash
            // Go ahead and calculate a single hash for all input documents since we'd end up having to
            // get each individual hash anyway, whether while checking for a complete hit or recording for the next time
            Dictionary<FilePath, int> missesBySource = new Dictionary<FilePath, int>();

            // Creating a new cache and swapping is the easiest way to expire old entries
            Dictionary<FilePath, CacheEntry> currentCache = _cache;
            _cache = new Dictionary<FilePath, CacheEntry>();

            // Check for hits and misses
            if (currentCache == null)
            {
                // If the current cache is null, this is the first run through
                misses.AddRange(context.Inputs);
                IEnumerable<IGrouping<FilePath, IDocument>> inputGroups = context.Inputs
                    .Where(input => input.Source != null)
                    .GroupBy(input => input.Source);
                foreach (IGrouping<FilePath, IDocument> inputGroup in inputGroups)
                {
                    missesBySource.Add(inputGroup.Key, await CombineCacheHashCodesAsync(inputGroup));
                }
            }
            else
            {
                // Note that due to cloning we could have multiple inputs and outputs with the same source
                // so we need to check all inputs with a given source and only consider a hit when they all match
                foreach (IGrouping<FilePath, IDocument> inputsBySource in context.Inputs.GroupBy(x => x.Source))
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
                                context.Logger.LogDebug($"Cache hit for {inputsBySource.Key}, using cached results");
                                _cache.Add(inputsBySource.Key, entry);
                                outputs.AddRange(entry.Documents);
                                continue;  // Go to the next source group since misses are dealt with below
                            }

                            // If a miss and we previously cached results, dispose cached results if we took over tracking
                            message = $"Cache miss for {inputsBySource.Key}, one or more documents have changed";
                        }

                        // Miss with non-null source: track source and input documents so we can cache for next pass
                        message = $"Cache miss for {inputsBySource.Key}, source not cached";
                        missesBySource.Add(inputsBySource.Key, inputHash);
                    }

                    // Miss, add inputs to execute
                    context.Logger.LogDebug(message ?? "Cache miss for null source, null sources are never cached");
                    misses.AddRange(inputsBySource);
                }
            }

            // Execute misses
            IReadOnlyList<IDocument> results = misses.Count == 0
                ? ImmutableArray<IDocument>.Empty
                : await context.ExecuteModulesAsync(Children, misses);
            Dictionary<FilePath, IGrouping<FilePath, IDocument>> resultsBySource =
                results.Where(x => x.Source != null).GroupBy(x => x.Source).ToDictionary(x => x.Key, x => x);
            outputs.AddRange(results);

            // Cache all miss sources, even if they resulted in an empty result document set
            foreach (KeyValuePair<FilePath, int> inputGroup in missesBySource)
            {
                // Did we get any results from this input source?
                IDocument[] cacheDocuments = null;
                if (resultsBySource.TryGetValue(inputGroup.Key, out IGrouping<FilePath, IDocument> sourceResults))
                {
                    cacheDocuments = sourceResults.ToArray();
                }

                // Add a cache entry
                _cache.Add(inputGroup.Key, new CacheEntry(inputGroup.Value, cacheDocuments));
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
            public CacheEntry(int inputHash, IDocument[] documents)
            {
                InputHash = inputHash;
                Documents = documents ?? Array.Empty<IDocument>();
            }

            public int InputHash { get; }

            public IDocument[] Documents { get; }
        }
    }
}
