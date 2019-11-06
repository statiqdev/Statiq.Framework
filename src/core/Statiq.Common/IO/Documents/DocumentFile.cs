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

        internal DocumentFile(DocumentFileProvider fileProvider, FilePath path)
        {
            _fileProvider = fileProvider ?? throw new ArgumentNullException(nameof(fileProvider));
            if (!fileProvider.Files.TryGetValue(path, out _document))
            {
                _document = null;
            }
            Path = path ?? throw new ArgumentNullException(nameof(path));
        }

        public FilePath Path { get; }

        NormalizedPath IFileSystemEntry.Path => Path;

        public IContentProvider GetContentProvider() =>
            _document == null ? NullContent.Provider : _document.ContentProvider;

        public IContentProvider GetContentProvider(string mediaType) => throw new NotSupportedException();

        public IDirectory Directory => new DocumentDirectory(_fileProvider, Path.Directory);

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

        public string MediaType => _document?.ContentProvider?.MediaType;

        public Task CopyToAsync(IFile destination, bool overwrite = true, bool createDirectory = true) => throw new NotSupportedException();

        public Task MoveToAsync(IFile destination) => throw new NotSupportedException();

        public void Delete() => throw new NotSupportedException();

        public Stream OpenAppend(bool createDirectory = true) => throw new NotSupportedException();

        public Stream Open(bool createDirectory = true) => throw new NotSupportedException();

        public Stream OpenWrite(bool createDirectory = true) => throw new NotSupportedException();

        public Task WriteAllTextAsync(string contents, bool createDirectory = true) => throw new NotSupportedException();

        public string ToDisplayString() => Path.ToDisplayString();
    }
}
