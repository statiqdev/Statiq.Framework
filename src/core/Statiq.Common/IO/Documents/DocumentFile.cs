using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Statiq.Common
{
    internal class DocumentFile : IFile
    {
        private readonly DocumentFileProvider _fileProvider;
        private readonly IDocument _document;

        internal DocumentFile(DocumentFileProvider fileProvider, in NormalizedPath path)
        {
            _fileProvider = fileProvider.ThrowIfNull(nameof(fileProvider));
            path.ThrowIfNull(nameof(path));
            if (!fileProvider.Files.TryGetValue(path, out _document))
            {
                _document = null;
            }
            Path = path;
        }

        public NormalizedPath Path { get; }

        NormalizedPath IFileSystemEntry.Path => Path;

        public IContentProvider GetContentProvider() =>
            _document?.ContentProvider ?? new NullContent();

        public IContentProvider GetContentProvider(string mediaType) =>
            _document?.ContentProvider.CloneWithMediaType(mediaType) ?? new NullContent(mediaType);

        public IDirectory Directory => new DocumentDirectory(_fileProvider, Path.Parent);

        public bool Exists => _document != null;

        public Stream OpenRead()
        {
            if (_document == null)
            {
                throw new FileNotFoundException();
            }
            return _document.GetContentStream();
        }

        public async Task<string> ReadAllTextAsync()
        {
            if (_document == null)
            {
                throw new FileNotFoundException();
            }
            return await _document.GetContentStringAsync();
        }

        public long Length
        {
            get
            {
                if (_document == null)
                {
                    throw new FileNotFoundException();
                }
                using (Stream stream = _document.GetContentStream())
                {
                    return stream.Length;
                }
            }
        }

        public string MediaType => _document?.ContentProvider.MediaType;

        public DateTime LastWriteTime => throw new NotSupportedException();

        public DateTime CreationTime => throw new NotSupportedException();

        public Task CopyToAsync(IFile destination, bool overwrite = true, bool createDirectory = true) => throw new NotSupportedException();

        public Task MoveToAsync(IFile destination) => throw new NotSupportedException();

        public void Delete() => throw new NotSupportedException();

        public Stream OpenAppend(bool createDirectory = true) => throw new NotSupportedException();

        public Stream Open(bool createDirectory = true) => throw new NotSupportedException();

        public Stream OpenWrite(bool createDirectory = true) => throw new NotSupportedException();

        public Task WriteAllTextAsync(string contents, bool createDirectory = true) => throw new NotSupportedException();

        public override string ToString() => Path.ToString();

        public string ToDisplayString() => Path.ToDisplayString();
    }
}
