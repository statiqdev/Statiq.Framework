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

        private readonly LocalFileProvider _fileProvider;
        private readonly System.IO.FileInfo _file;

        public LocalFile(LocalFileProvider fileProvider, in NormalizedPath path)
        {
            _fileProvider = fileProvider.ThrowIfNull(nameof(fileProvider));

            path.ThrowIfNull(nameof(path));
            path.ThrowIfRelative(nameof(path));

            Path = path;
            _file = new System.IO.FileInfo(Path.FullPath);
        }

        /// <inheritdoc/>
        public NormalizedPath Path { get; }

        /// <inheritdoc/>
        NormalizedPath IFileSystemEntry.Path => Path;

        /// <inheritdoc/>
        public IDirectory Directory => _fileProvider.FileSystem.GetDirectory(Path.Parent);

        /// <inheritdoc/>
        public bool Exists => _file.Exists;

        /// <inheritdoc/>
        public long Length => _file.Length;

        /// <inheritdoc/>
        public string MediaType => Path.MediaType;

        /// <inheritdoc/>
        public DateTime LastWriteTime => _file.LastWriteTime;

        /// <inheritdoc/>
        public DateTime CreationTime => _file.CreationTime;

        /// <inheritdoc/>
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

        /// <inheritdoc/>
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

        /// <inheritdoc/>
        public void Delete() => LocalFileProvider.RetryPolicy.Execute(() => _file.Delete());

        /// <inheritdoc/>
        public async Task<string> ReadAllTextAsync(CancellationToken cancellationToken = default) =>
            await LocalFileProvider.AsyncRetryPolicy.ExecuteAsync(() => File.ReadAllTextAsync(_file.FullName, cancellationToken));

        /// <inheritdoc/>
        public async Task WriteAllTextAsync(string contents, bool createDirectory = true, CancellationToken cancellationToken = default)
        {
            if (createDirectory)
            {
                CreateDirectory();
            }
            _fileProvider.FileSystem.WriteTracker.AddWrite(Path, GetCacheHashCode());
            await LocalFileProvider.AsyncRetryPolicy.ExecuteAsync(() => File.WriteAllTextAsync(_file.FullName, contents, cancellationToken));
        }

        /// <inheritdoc/>
        // Assumes most file operations are going to be asynchronous and sequential
        public Stream OpenRead() => LocalFileProvider.RetryPolicy.Execute(() =>
            new FileStream(_file.FullName, FileMode.Open, FileAccess.Read, FileShare.Read, BufferSize, FileOptions.Asynchronous | FileOptions.SequentialScan));

        /// <inheritdoc/>
        public Stream OpenWrite(bool createDirectory = true)
        {
            if (createDirectory)
            {
                CreateDirectory();
            }

            // Assumes most file operations are going to be asynchronous and sequential
            return LocalFileProvider.RetryPolicy.Execute(() =>
                new WrittenFileStream(
                    new FileStream(_file.FullName, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None, BufferSize, FileOptions.Asynchronous | FileOptions.SequentialScan),
                    _fileProvider.FileSystem.WriteTracker,
                    this));
        }

        /// <inheritdoc/>
        public Stream OpenAppend(bool createDirectory = true)
        {
            if (createDirectory)
            {
                CreateDirectory();
            }

            // Assumes most file operations are going to be asynchronous and sequential
            return LocalFileProvider.RetryPolicy.Execute(() =>
                new WrittenFileStream(
                    new FileStream(_file.FullName, FileMode.Append, FileAccess.Write, FileShare.None, BufferSize, FileOptions.Asynchronous | FileOptions.SequentialScan),
                    _fileProvider.FileSystem.WriteTracker,
                    this));
        }

        /// <inheritdoc/>
        public Stream Open(bool createDirectory = true)
        {
            if (createDirectory)
            {
                CreateDirectory();
            }

            // Assumes most file operations are going to be asynchronous and sequential
            // We don't actually know if this is going to be used for writing, so use the WrittenFileStream just in case
            return LocalFileProvider.RetryPolicy.Execute(() =>
                new WrittenFileStream(
                    new FileStream(_file.FullName, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite, BufferSize, FileOptions.Asynchronous | FileOptions.SequentialScan),
                    _fileProvider.FileSystem.WriteTracker,
                    this));
        }

        /// <inheritdoc/>
        public TextReader OpenText() => LocalFileProvider.RetryPolicy.Execute(() => File.OpenText(_file.FullName));

        private void CreateDirectory() => Directory.Create();

        /// <inheritdoc/>
        public IContentProvider GetContentProvider() => GetContentProvider(MediaType);

        /// <inheritdoc/>
        public IContentProvider GetContentProvider(string mediaType) =>
            _file.Exists ? (IContentProvider)new FileContent(this, mediaType) : new NullContent(mediaType);

        public override string ToString() => Path.ToString();

        /// <inheritdoc/>
        public string ToDisplayString() => Path.ToDisplayString();

        /// <inheritdoc/>
        public Task<int> GetCacheHashCodeAsync() => Task.FromResult(GetCacheHashCode());

        internal int GetCacheHashCode()
        {
            HashCode hashCode = default;
            hashCode.Add(_file.FullName);
            if (_file.Exists)
            {
                hashCode.Add(_file.Length);
                hashCode.Add(_file.CreationTime);
                hashCode.Add(_file.LastWriteTime);
            }
            else
            {
                hashCode.Add(-1);
            }
            return hashCode.ToHashCode();
        }
    }
}
