using System;
using Statiq.Common;
using Polly;
using ConcurrentCollections;

namespace Statiq.Core
{
    public class LocalFileProvider : IFileProvider
    {
        public LocalFileProvider(IReadOnlyFileSystem fileSystem, ConcurrentHashSet<IFile> writtenFiles = null)
        {
            FileSystem = fileSystem.ThrowIfNull(nameof(fileSystem));
            WrittenFiles = writtenFiles ?? new ConcurrentHashSet<IFile>();
        }

        public IReadOnlyFileSystem FileSystem { get; }

        public ConcurrentHashSet<IFile> WrittenFiles { get; }

        internal static Policy RetryPolicy { get; } =
            Policy
                .Handle<Exception>()
                .WaitAndRetry(3, _ => TimeSpan.FromMilliseconds(100));

        internal static AsyncPolicy AsyncRetryPolicy { get; } =
            Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(3, _ => TimeSpan.FromMilliseconds(100));

        public IFile GetFile(NormalizedPath path) => new LocalFile(this, path);

        public IDirectory GetDirectory(NormalizedPath path) => new LocalDirectory(this, path);
    }
}