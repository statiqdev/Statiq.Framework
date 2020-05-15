using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Statiq.Common;

namespace Statiq.Testing
{
    public class TestDirectory : IDirectory
    {
        private readonly TestFileProvider _fileProvider;

        public TestDirectory(TestFileProvider fileProvider, NormalizedPath path)
        {
            _fileProvider = fileProvider;
            Path = path;
        }

        public NormalizedPath Path { get; }

        NormalizedPath IFileSystemEntry.Path => Path;

        public bool Exists => _fileProvider.Directories.Contains(Path);

        public DateTime LastWriteTime { get; set; }

        public DateTime CreationTime { get; set; }

        public IDirectory Parent
        {
            get
            {
                NormalizedPath parentPath = Path.Parent;
                return parentPath.IsNull ? null : new TestDirectory(_fileProvider, parentPath);
            }
        }

        public void Create() => _fileProvider.Directories.Add(Path);

        public void Delete(bool recursive) => _fileProvider.Directories.Remove(Path);

        public IEnumerable<IDirectory> GetDirectories(SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            if (searchOption == SearchOption.TopDirectoryOnly)
            {
                return _fileProvider.Directories
                    .Where(x => Path.ContainsChild(x))
                    .Select(x => new TestDirectory(_fileProvider, x));
            }
            return _fileProvider.Directories
                .Where(x => Path.ContainsDescendant(x))
                .Select(x => new TestDirectory(_fileProvider, x));
        }

        public IEnumerable<IFile> GetFiles(SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            if (searchOption == SearchOption.TopDirectoryOnly)
            {
                return _fileProvider.Files.Keys
                    .Where(x => Path.ContainsChild(x))
                    .Select(x => new TestFile(_fileProvider, x));
            }
            return _fileProvider.Files.Keys
                .Where(x => Path.ContainsDescendant(x))
                .Select(x => new TestFile(_fileProvider, x));
        }

        public IDirectory GetDirectory(NormalizedPath path)
        {
            path.ThrowIfNull(nameof(path));

            if (!path.IsRelative)
            {
                throw new ArgumentException("Path must be relative", nameof(path));
            }

            return new TestDirectory(_fileProvider, Path.Combine(path));
        }

        public IFile GetFile(NormalizedPath path)
        {
            path.ThrowIfNull(nameof(path));

            if (!path.IsRelative)
            {
                throw new ArgumentException("Path must be relative", nameof(path));
            }

            return new TestFile(_fileProvider, Path.Combine(path));
        }

        public override string ToString() => Path.ToString();

        public string ToDisplayString() => Path.ToSafeDisplayString();
    }
}