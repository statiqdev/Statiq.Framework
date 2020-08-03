using System;
using Statiq.Common;
using Polly;

namespace Statiq.Core
{
    public class LocalFileProvider : IFileProvider
    {
        private readonly IReadOnlyFileSystem _fileSystem;

        public LocalFileProvider(IReadOnlyFileSystem fileSystem)
        {
            _fileSystem = fileSystem.ThrowIfNull(nameof(fileSystem));
        }

        internal static Policy RetryPolicy { get; } =
            Policy
                .Handle<Exception>()
                .WaitAndRetry(3, _ => TimeSpan.FromMilliseconds(100));

        internal static AsyncPolicy AsyncRetryPolicy { get; } =
            Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(3, _ => TimeSpan.FromMilliseconds(100));

        public IFile GetFile(NormalizedPath path) => new LocalFile(_fileSystem, path);

        public IDirectory GetDirectory(NormalizedPath path) => new LocalDirectory(_fileSystem, path);
    }
}