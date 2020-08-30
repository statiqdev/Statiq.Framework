using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Statiq.Common;

namespace Statiq.Core
{
    // Initially based on code from Cake (http://cakebuild.net/)
    internal class LocalFile : IFile
    {
        private const int BufferSize = 8192;

        private readonly IReadOnlyFileSystem _fileSystem;
        private readonly System.IO.FileInfo _file;

        public LocalFile(IReadOnlyFileSystem fileSystem, in NormalizedPath path)
        {
            _fileSystem = fileSystem.ThrowIfNull(nameof(fileSystem));

            path.ThrowIfNull(nameof(path));

            if (path.IsRelative)
            {
                throw new ArgumentException("Path must be absolute", nameof(path));
            }

            Path = path;
            _file = new System.IO.FileInfo(Path.FullPath);
        }

        public NormalizedPath Path { get; }

        NormalizedPath IFileSystemEntry.Path => Path;

        public IDirectory Directory => _fileSystem.GetDirectory(Path.Parent);

        public bool Exists => _file.Exists;

        public long Length => _file.Length;

        public string MediaType => Path.MediaType;

        public DateTime LastWriteTime => _file.LastWriteTime;

        public DateTime CreationTime => _file.CreationTime;

        public async Task CopyToAsync(IFile destination, bool overwrite = true, bool createDirectory = true, CancellationToken cancellationToken = default)
        {
            destination.ThrowIfNull(nameof(destination));

            // Create the directory
            if (createDirectory)
            {
                destination.Directory.Create();
            }

            // Use streams instead of System.IO since they're async
            if (!destination.Exists || overwrite)
            {
                using (Stream sourceStream = OpenRead())
                {
                    using (Stream destinationStream = destination.OpenWrite())
                    {
                        await sourceStream.CopyToAsync(destinationStream, cancellationToken);
                    }
                }
            }
        }

        public async Task MoveToAsync(IFile destination, CancellationToken cancellationToken = default)
        {
            destination.ThrowIfNull(nameof(destination));

            // Use streams instead of System.IO since they're async
            using (Stream sourceStream = OpenRead())
            {
                using (Stream destinationStream = destination.OpenWrite())
                {
                    await sourceStream.CopyToAsync(destinationStream, cancellationToken);
                }
            }
            Delete();
        }

        public void Delete() => LocalFileProvider.RetryPolicy.Execute(() => _file.Delete());

        public async Task<string> ReadAllTextAsync(CancellationToken cancellationToken = default) =>
            await LocalFileProvider.AsyncRetryPolicy.ExecuteAsync(() => File.ReadAllTextAsync(_file.FullName, cancellationToken));

        public async Task WriteAllTextAsync(string contents, bool createDirectory = true, CancellationToken cancellationToken = default)
        {
            if (createDirectory)
            {
                CreateDirectory();
            }

            await LocalFileProvider.AsyncRetryPolicy.ExecuteAsync(() => File.WriteAllTextAsync(_file.FullName, contents, cancellationToken));
        }

        public Stream OpenRead() => new FileStream(_file.FullName, FileMode.Open, FileAccess.Read, FileShare.Read, BufferSize, true);

        public Stream OpenWrite(bool createDirectory = true)
        {
            if (createDirectory)
            {
                CreateDirectory();
            }
            return new FileStream(_file.FullName, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None, BufferSize, true);
        }

        public Stream OpenAppend(bool createDirectory = true)
        {
            if (createDirectory)
            {
                CreateDirectory();
            }
            return new FileStream(_file.FullName, FileMode.Append, FileAccess.Write, FileShare.None, BufferSize, true);
        }

        public Stream Open(bool createDirectory = true)
        {
            if (createDirectory)
            {
                CreateDirectory();
            }
            return new FileStream(_file.FullName, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite, BufferSize, true);
        }

        private void CreateDirectory() => Directory.Create();

        public IContentProvider GetContentProvider() => GetContentProvider(MediaType);

        public IContentProvider GetContentProvider(string mediaType) =>
            _file.Exists ? (IContentProvider)new FileContent(this, mediaType) : new NullContent(mediaType);

        public override string ToString() => Path.ToString();

        public string ToDisplayString() => Path.ToDisplayString();
    }
}
