using System.IO;
using System.Text;
using System.Threading.Tasks;
using Statiq.Common;

namespace Statiq.Testing
{
    public class TestFile : IFile
    {
        private readonly TestFileProvider _fileProvider;

        public TestFile(TestFileProvider fileProvider, FilePath path)
        {
            _fileProvider = fileProvider;
            Path = path;
        }

        public FilePath Path { get; }

        NormalizedPath IFileSystemEntry.Path => Path;

        public bool Exists => _fileProvider.Files.ContainsKey(Path.FullPath);

        public IDirectory Directory => new TestDirectory(_fileProvider, Path.Directory);

        public long Length => _fileProvider.Files[Path.FullPath].Length;

        public string MediaType => Path.MediaType;

        private void CreateDirectory(bool createDirectory, IFile file)
        {
            if (!createDirectory && !_fileProvider.Directories.Contains(file.Path.Directory.FullPath))
            {
                throw new IOException($"Directory {file.Path.Directory.FullPath} does not exist");
            }
            if (createDirectory)
            {
                DirectoryPath parent = file.Path.Directory;
                while (parent != null)
                {
                    _fileProvider.Directories.Add(parent.FullPath);
                    parent = parent.Parent;
                }
            }
        }

        public Task CopyToAsync(IFile destination, bool overwrite = true, bool createDirectory = true)
        {
            CreateDirectory(createDirectory, destination);

            if (overwrite)
            {
                _fileProvider.Files[destination.Path.FullPath] = new StringBuilder(_fileProvider.Files[Path.FullPath].ToString());
            }
            else
            {
                _fileProvider.Files.TryAdd(destination.Path.FullPath, new StringBuilder(_fileProvider.Files[Path.FullPath].ToString()));
            }

            return Task.CompletedTask;
        }

        public Task MoveToAsync(IFile destination)
        {
            if (!_fileProvider.Files.ContainsKey(Path.FullPath))
            {
                throw new FileNotFoundException();
            }

            if (_fileProvider.Files.TryRemove(Path.FullPath, out StringBuilder builder))
            {
                _fileProvider.Files.TryAdd(destination.Path.FullPath, builder);
            }

            return Task.CompletedTask;
        }

        public void Delete() => _fileProvider.Files.TryRemove(Path.FullPath, out StringBuilder _);

        public Task<string> ReadAllTextAsync() => Task.FromResult(_fileProvider.Files[Path.FullPath].ToString());

        public Task WriteAllTextAsync(string contents, bool createDirectory = true)
        {
            CreateDirectory(createDirectory, this);
            _fileProvider.Files[Path.FullPath] = new StringBuilder(contents);
            return Task.CompletedTask;
        }

        public Stream OpenRead()
        {
            if (!_fileProvider.Files.TryGetValue(Path.FullPath, out StringBuilder builder))
            {
                throw new FileNotFoundException();
            }
            byte[] bytes = Encoding.UTF8.GetBytes(builder.ToString());
            return new MemoryStream(bytes);
        }

        public Stream OpenWrite(bool createDirectory = true)
        {
            CreateDirectory(createDirectory, this);
            return new StringBuilderStream(_fileProvider.Files.AddOrUpdate(Path.FullPath, new StringBuilder(), (x, y) => y));
        }

        public Stream OpenAppend(bool createDirectory = true)
        {
            CreateDirectory(createDirectory, this);
            StringBuilderStream stream = new StringBuilderStream(_fileProvider.Files.AddOrUpdate(Path.FullPath, new StringBuilder(), (x, y) => y));

            // Start appending at the end of the stream.
            stream.Position = stream.Length;
            return stream;
        }

        public Stream Open(bool createDirectory = true)
        {
            CreateDirectory(createDirectory, this);
            return new StringBuilderStream(_fileProvider.Files.AddOrUpdate(Path.FullPath, new StringBuilder(), (x, y) => y));
        }

        public IContentProvider GetContentProvider() => GetContentProvider(MediaType);

        public IContentProvider GetContentProvider(string mediaType) =>
            _fileProvider.Files.ContainsKey(Path.FullPath) ? (IContentProvider)new FileContent(this, mediaType) : new NullContent(mediaType);

        public string ToDisplayString() => Path.ToSafeDisplayString();
    }
}