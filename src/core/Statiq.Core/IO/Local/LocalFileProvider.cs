using System;
using System.Collections.Concurrent;
using ConcurrentCollections;
using Polly;
using Statiq.Common;

namespace Statiq.Core
{
    public class LocalFileProvider : IFileProvider
    {
        public LocalFileProvider(IReadOnlyFileSystem fileSystem, ConcurrentDictionary<NormalizedPath, int> writtenFiles = null)
        {
            FileSystem = fileSystem.ThrowIfNull(nameof(fileSystem));
            WrittenFiles = writtenFiles ?? new ConcurrentDictionary<NormalizedPath, int>();
        }

        public IReadOnlyFileSystem FileSystem { get; }

        /// <summary>
        /// Keeps track of the files opened for writing and their post-write state
        /// hash for a given execution (reset by the engine before execution).
        /// </summary>
        public ConcurrentDictionary<NormalizedPath, int> WrittenFiles { get; }

        internal static Policy RetryPolicy { get; } =
            Policy
                .Handle<Exception>()
                .WaitAndRetry(5, retry => retry * TimeSpan.FromMilliseconds(100));

        internal static AsyncPolicy AsyncRetryPolicy { get; } =
            Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(5, retry => retry * TimeSpan.FromMilliseconds(100));

        /// <inheritdoc/>
        public IFile GetFile(NormalizedPath path) => new LocalFile(this, path);

        /// <inheritdoc/>
        public IDirectory GetDirectory(NormalizedPath path) => new LocalDirectory(this, path);
    }
}