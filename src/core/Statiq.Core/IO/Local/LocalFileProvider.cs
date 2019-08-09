using System;
using System.Threading.Tasks;
using Statiq.Common;
using Polly;

namespace Statiq.Core
{
    public class LocalFileProvider : IFileProvider
    {
        internal static Policy RetryPolicy { get; } =
            Policy
                .Handle<Exception>()
                .WaitAndRetry(3, _ => TimeSpan.FromMilliseconds(100));

        internal static AsyncPolicy AsyncRetryPolicy { get; } =
            Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(3, _ => TimeSpan.FromMilliseconds(100));

        public IFile GetFile(FilePath path) => new LocalFile(path);

        public IDirectory GetDirectory(DirectoryPath path) => new LocalDirectory(path);
    }
}