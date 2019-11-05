using System;
using System.IO;
using System.Threading.Tasks;
using Statiq.Common;

namespace Statiq.Core
{
    // Initially based on code from Cake (http://cakebuild.net/)
    internal class LocalFile : IFile
    {
        private readonly System.IO.FileInfo _file;

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
            _file = new System.IO.FileInfo(Path.FullPath);
        }

        public IDirectory Directory => new LocalDirectory(Path.Directory);

        public bool Exists => _file.Exists;

        public long Length => _file.Length;

        public string MediaType => MediaTypes.TryGet(Path.Extension, out string mediaType) ? mediaType : null;

        public async Task CopyToAsync(IFile destination, bool overwrite = true, bool createDirectory = true)
        {
            _ = destination ?? throw new ArgumentNullException(nameof(destination));

            // Create the directory
            if (createDirectory)
            {
                IDirectory directory = destination.Directory;
                directory.Create();
            }

            // Use the file system APIs if destination is also in the file system
            if (destination is LocalFile)
            {
                LocalFileProvider.RetryPolicy.Execute(() => _file.CopyTo(destination.Path.FullPath, overwrite));
            }
            else
            {
                // Otherwise use streams to perform the copy
                using (Stream sourceStream = OpenRead())
                {
                    using (Stream destinationStream = destination.OpenWrite())
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
                using (Stream sourceStream = OpenRead())
                {
                    using (Stream destinationStream = destination.OpenWrite())
                    {
                        await sourceStream.CopyToAsync(destinationStream);
                    }
                }
                Delete();
            }
        }

        public void Delete() => LocalFileProvider.RetryPolicy.Execute(() => _file.Delete());

        public async Task<string> ReadAllTextAsync() =>
            await LocalFileProvider.AsyncRetryPolicy.ExecuteAsync(() => File.ReadAllTextAsync(_file.FullName));

        public async Task WriteAllTextAsync(string contents, bool createDirectory = true)
        {
            if (createDirectory)
            {
                CreateDirectory();
            }

            await LocalFileProvider.AsyncRetryPolicy.ExecuteAsync(() => File.WriteAllTextAsync(_file.FullName, contents));
        }

        public Stream OpenRead() =>
            LocalFileProvider.RetryPolicy.Execute(() => _file.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite));

        public Stream OpenWrite(bool createDirectory = true)
        {
            if (createDirectory)
            {
                CreateDirectory();
            }
            return LocalFileProvider.RetryPolicy.Execute(() => _file.Open(FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite));
        }

        public Stream OpenAppend(bool createDirectory = true)
        {
            if (createDirectory)
            {
                CreateDirectory();
            }
            return LocalFileProvider.RetryPolicy.Execute(() => _file.Open(FileMode.Append, FileAccess.Write, FileShare.ReadWrite));
        }

        public Stream Open(bool createDirectory = true)
        {
            if (createDirectory)
            {
                CreateDirectory();
            }
            return LocalFileProvider.RetryPolicy.Execute(() => _file.Open(FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite));
        }

        private void CreateDirectory() => Directory.Create();

        public IContentProvider GetContentProvider() =>
            _file.Exists ? (IContentProvider)new FileContent(this, MediaType) : NullContent.Provider;

        public string ToDisplayString() => Path.ToDisplayString();
    }
}
