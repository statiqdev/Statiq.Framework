using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Statiq.Common;

namespace Statiq.Testing
{
    public class TestFile : IFile
    {
        private readonly IReadOnlyFileSystem _fileSystem;
        private readonly TestFileProvider _fileProvider;

        public TestFile(IReadOnlyFileSystem fileSystem, TestFileProvider fileProvider, in NormalizedPath path)
        {
            _fileSystem = fileSystem;
            _fileProvider = fileProvider;
            Path = path;
        }

        public NormalizedPath Path { get; }

        NormalizedPath IFileSystemEntry.Path => Path;

        public bool Exists => _fileProvider.Files.ContainsKey(Path);

        public IDirectory Directory => _fileSystem.GetDirectory(Path.Parent);

        public long Length => _fileProvider.Files[Path].Length;

        public string MediaType => Path.MediaType;

        public DateTime LastWriteTime { get; set; }

        public DateTime CreationTime { get; set; }

        private void CreateDirectory(bool createDirectory, IFile file)
        {
            if (!createDirectory && !_fileProvider.Directories.Contains(file.Path.Parent))
            {
                throw new IOException($"Directory {file.Path.Parent} does not exist");
            }
            if (createDirectory)
            {
                NormalizedPath parent = file.Path.Parent;
                while (!parent.IsNullOrEmpty)
                {
                    _fileProvider.Directories.Add(parent);
                    parent = parent.Parent;
                }
            }
        }

        public Task CopyToAsync(IFile destination, bool overwrite = true, bool createDirectory = true, CancellationToken cancellationToken = default)
        {
            CreateDirectory(createDirectory, destination);

            if (overwrite)
            {
                _fileProvider.Files[destination.Path] = new StringBuilder(_fileProvider.Files[Path].ToString());
            }
            else
            {
                _fileProvider.Files.TryAdd(destination.Path, new StringBuilder(_fileProvider.Files[Path].ToString()));
            }

            return Task.CompletedTask;
        }

        public Task MoveToAsync(IFile destination, CancellationToken cancellationToken = default)
        {
            if (!_fileProvider.Files.ContainsKey(Path))
            {
                throw new FileNotFoundException();
            }

            if (_fileProvider.Files.TryRemove(Path, out StringBuilder builder))
            {
                _fileProvider.Files.TryAdd(destination.Path, builder);
            }

            return Task.CompletedTask;
        }

        public void Delete() => _fileProvider.Files.TryRemove(Path, out StringBuilder _);

        public Task<string> ReadAllTextAsync(CancellationToken cancellationToken = default) => Task.FromResult(_fileProvider.Files[Path].ToString());

        public Task WriteAllTextAsync(string contents, bool createDirectory = true, CancellationToken cancellationToken = default)
        {
            CreateDirectory(createDirectory, this);
            _fileProvider.Files[Path] = new StringBuilder(contents);
            return Task.CompletedTask;
        }

        public Stream OpenRead()
        {
            if (!_fileProvider.Files.TryGetValue(Path, out StringBuilder builder))
            {
                throw new FileNotFoundException();
            }
            byte[] bytes = Encoding.UTF8.GetBytes(builder.ToString());
            return new MemoryStream(bytes);
        }

        public Stream OpenWrite(bool createDirectory = true)
        {
            CreateDirectory(createDirectory, this);
            return new StringBuilderStream(_fileProvider.Files.AddOrUpdate(Path, new StringBuilder(), (x, y) => y));
        }

        public Stream OpenAppend(bool createDirectory = true)
        {
            CreateDirectory(createDirectory, this);
            StringBuilderStream stream = new StringBuilderStream(_fileProvider.Files.AddOrUpdate(Path, new StringBuilder(), (x, y) => y));

            // Start appending at the end of the stream.
            stream.Position = stream.Length;
            return stream;
        }

        public Stream Open(bool createDirectory = true)
        {
            CreateDirectory(createDirectory, this);
            return new StringBuilderStream(_fileProvider.Files.AddOrUpdate(Path, new StringBuilder(), (x, y) => y));
        }

        public IContentProvider GetContentProvider() => GetContentProvider(MediaType);

        public IContentProvider GetContentProvider(string mediaType) =>
            _fileProvider.Files.ContainsKey(Path) ? (IContentProvider)new FileContent(this, mediaType) : new NullContent(mediaType);

        public override string ToString() => Path.ToString();

        public string ToDisplayString() => Path.ToSafeDisplayString();
    }
}