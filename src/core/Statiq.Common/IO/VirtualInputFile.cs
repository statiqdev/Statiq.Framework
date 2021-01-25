using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Statiq.Common
{
    /// <summary>
    /// Wraps a file within a virtual directory so that crawling back up to the directory
    /// returns the original virtual directory.
    /// </summary>
    internal class VirtualInputFile : IFile
    {
        private readonly IFile _file;
        private readonly VirtualInputDirectory _directory;

        public VirtualInputFile(IFile file, VirtualInputDirectory directory)
        {
            _file = file ?? throw new ArgumentNullException(nameof(file));
            _directory = directory ?? throw new ArgumentNullException(nameof(file));
        }

        public NormalizedPath Path => _file.Path;

        public IDirectory Directory => _directory;

        public long Length => _file.Length;

        public string MediaType => _file.MediaType;

        public bool Exists => _file.Exists;

        public DateTime LastWriteTime => _file.LastWriteTime;

        public DateTime CreationTime => _file.CreationTime;

        public Task CopyToAsync(
            IFile destination,
            bool overwrite = true,
            bool createDirectory = true,
            CancellationToken cancellationToken = default) =>
            _file.CopyToAsync(destination, overwrite, createDirectory, cancellationToken);

        public void Delete() => _file.Delete();

        public IContentProvider GetContentProvider() => _file.GetContentProvider();

        public IContentProvider GetContentProvider(string mediaType) =>
            _file.GetContentProvider(mediaType);

        public Task MoveToAsync(IFile destination, CancellationToken cancellationToken = default) =>
            _file.MoveToAsync(destination, cancellationToken);

        public Stream Open(bool createDirectory = true) => _file.Open(createDirectory);

        public Stream OpenAppend(bool createDirectory = true) => _file.OpenAppend(createDirectory);

        public Stream OpenRead() => _file.OpenRead();

        public TextReader OpenText() => _file.OpenText();

        public Stream OpenWrite(bool createDirectory = true) => _file.OpenWrite(createDirectory);

        public Task<string> ReadAllTextAsync(CancellationToken cancellationToken = default) =>
            _file.ReadAllTextAsync(cancellationToken);

        public string ToDisplayString() => _file.ToDisplayString();

        public Task WriteAllTextAsync(
            string contents,
            bool createDirectory = true,
            CancellationToken cancellationToken = default) =>
            _file.WriteAllTextAsync(contents, createDirectory, cancellationToken);
    }
}
