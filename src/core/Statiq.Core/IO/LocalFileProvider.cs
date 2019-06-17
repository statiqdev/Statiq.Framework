using System;
using System.Threading.Tasks;
using Polly;
using Statiq.Common.IO;

namespace Statiq.Core.IO
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

        public Task<IFile> GetFileAsync(FilePath path) =>
            Task.FromResult<IFile>(new LocalFile(path));

        public Task<IDirectory> GetDirectoryAsync(DirectoryPath path) =>
            Task.FromResult<IDirectory>(new LocalDirectory(path));
    }
}