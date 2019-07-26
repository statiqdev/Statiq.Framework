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

        public Task<IDirectory> GetDirectoryAsync() =>
            Task.FromResult<IDirectory>(new DocumentDirectory(_fileProvider, Path.Directory));

        public Task<bool> GetExistsAsync() => Task.FromResult(_document != null);

        public async Task<Stream> OpenReadAsync()
        {
            if (_document == null)
            {
                throw new FileNotFoundException();
            }
            return await _document.GetStreamAsync();
        }

        public async Task<string> ReadAllTextAsync()
        {
            if (_document == null)
            {
                throw new FileNotFoundException();
            }
            return await _document.GetStringAsync();
        }

        public async Task<long> GetLengthAsync()
        {
            if (_document == null)
            {
                throw new FileNotFoundException();
            }
            using (Stream stream = await _document.GetStreamAsync())
            {
                return stream.Length;
            }
        }

        public Task CopyToAsync(IFile destination, bool overwrite = true, bool createDirectory = true) => throw new NotSupportedException();

        public Task DeleteAsync() => throw new NotSupportedException();

        public Task MoveToAsync(IFile destination) => throw new NotSupportedException();

        public Task<Stream> OpenAppendAsync(bool createDirectory = true) => throw new NotSupportedException();

        public Task<Stream> OpenAsync(bool createDirectory = true) => throw new NotSupportedException();

        public Task<Stream> OpenWriteAsync(bool createDirectory = true) => throw new NotSupportedException();

        public Task WriteAllTextAsync(string contents, bool createDirectory = true) => throw new NotSupportedException();

        public string ToDisplayString() => Path.ToDisplayString();
    }
}
