using System;
using System.IO;
using System.Threading.Tasks;
using Wyam.Common.IO;

namespace Wyam.Core.IO
{
    // Initially based on code from Cake (http://cakebuild.net/)
    internal class LocalFile : IFile
    {
        private readonly FileInfo _file;

        public FilePath Path { get; }

        NormalizedPath IFileSystemEntry.Path => Path;

        public LocalFile(FilePath path)
        {
            _ = path ?? throw new ArgumentNullException(nameof(path));
            if (path.IsRelative)
            {
                throw new ArgumentException("Path must be absolute", nameof(path));
            }

            Path = path;
            _file = new FileInfo(Path.FullPath);
        }

        public Task<IDirectory> GetDirectoryAsync() => Task.FromResult<IDirectory>(new LocalDirectory(Path.Directory));

        public Task<bool> GetExistsAsync() => Task.FromResult(_file.Exists);

        public Task<long> GetLengthAsync() => Task.FromResult(_file.Length);

        public async Task CopyToAsync(IFile destination, bool overwrite = true, bool createDirectory = true)
        {
            _ = destination ?? throw new ArgumentNullException(nameof(destination));

            // Create the directory
            if (createDirectory)
            {
                IDirectory directory = await destination.GetDirectoryAsync();
                await directory.CreateAsync();
            }

            // Use the file system APIs if destination is also in the file system
            if (destination is LocalFile)
            {
                LocalFileProvider.RetryPolicy.Execute(() => _file.CopyTo(destination.Path.FullPath, overwrite));
            }
            else
            {
                // Otherwise use streams to perform the copy
                using (Stream sourceStream = await OpenReadAsync())
                {
                    using (Stream destinationStream = await destination.OpenWriteAsync())
                    {
                        await sourceStream.CopyToAsync(destinationStream);
                    }
                }
            }
        }

        public async Task MoveToAsync(IFile destination)
        {
            _ = destination ?? throw new ArgumentNullException(nameof(destination));

            // Use the file system APIs if destination is also in the file system
            if (destination is LocalFile)
            {
                LocalFileProvider.RetryPolicy.Execute(() => _file.MoveTo(destination.Path.FullPath));
            }
            else
            {
                // Otherwise use streams to perform the move
                using (Stream sourceStream = await OpenReadAsync())
                {
                    using (Stream destinationStream = await destination.OpenWriteAsync())
                    {
                        await sourceStream.CopyToAsync(destinationStream);
                    }
                }
                await DeleteAsync();
            }
        }

        public Task DeleteAsync()
        {
            LocalFileProvider.RetryPolicy.Execute(() => _file.Delete());
            return Task.CompletedTask;
        }

        public async Task<string> ReadAllTextAsync() =>
            await LocalFileProvider.AsyncRetryPolicy.ExecuteAsync(() => File.ReadAllTextAsync(_file.FullName));

        public async Task WriteAllTextAsync(string contents, bool createDirectory = true)
        {
            if (createDirectory)
            {
                await CreateDirectoryAsync();
            }

            await LocalFileProvider.AsyncRetryPolicy.ExecuteAsync(() => File.WriteAllTextAsync(_file.FullName, contents));
        }

        public Task<Stream> OpenReadAsync() =>
            Task.FromResult<Stream>(LocalFileProvider.RetryPolicy.Execute(() => _file.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite)));

        public async Task<Stream> OpenWriteAsync(bool createDirectory = true)
        {
            if (createDirectory)
            {
                await CreateDirectoryAsync();
            }
            return LocalFileProvider.RetryPolicy.Execute(() => _file.Open(FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite));
        }

        public async Task<Stream> OpenAppendAsync(bool createDirectory = true)
        {
            if (createDirectory)
            {
                await CreateDirectoryAsync();
            }
            return LocalFileProvider.RetryPolicy.Execute(() => _file.Open(FileMode.Append, FileAccess.Write, FileShare.ReadWrite));
        }

        public async Task<Stream> OpenAsync(bool createDirectory = true)
        {
            if (createDirectory)
            {
                await CreateDirectoryAsync();
            }
            return LocalFileProvider.RetryPolicy.Execute(() => _file.Open(FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite));
        }

        private async Task CreateDirectoryAsync()
        {
            IDirectory directory = await GetDirectoryAsync();
            await directory.CreateAsync();
        }
    }
}
