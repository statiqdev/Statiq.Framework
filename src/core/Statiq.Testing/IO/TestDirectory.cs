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

        public TestDirectory(TestFileProvider fileProvider, DirectoryPath path)
        {
            _fileProvider = fileProvider;
            Path = path;
        }

        public DirectoryPath Path { get; }

        NormalizedPath IFileSystemEntry.Path => Path;

        public bool Exists => _fileProvider.Directories.Contains(Path.FullPath);

        public IDirectory Parent
        {
            get
            {
                DirectoryPath parentPath = Path.Parent;
                return parentPath == null ? null : new TestDirectory(_fileProvider, parentPath);
            }
        }

        public void Create() => _fileProvider.Directories.Add(Path.FullPath);

        public void Delete(bool recursive) => _fileProvider.Directories.Remove(Path.FullPath);

        public IEnumerable<IDirectory> GetDirectories(SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            if (searchOption == SearchOption.TopDirectoryOnly)
            {
                string adjustedPath = Path.FullPath.EndsWith("/", StringComparison.Ordinal)
                    ? Path.FullPath.Substring(0, Path.FullPath.Length - 1)
                    : Path.FullPath;
                return _fileProvider.Directories
                    .Where(x => x.StartsWith(adjustedPath + "/")
                        && adjustedPath.Count(c => c == '/') == x.Count(c => c == '/') - 1
                        && Path.FullPath != x)
                    .Select(x => new TestDirectory(_fileProvider, x))
                    .Cast<IDirectory>();
            }
            return _fileProvider.Directories
                .Where(x => x.StartsWith(Path.FullPath + "/") && Path.FullPath != x)
                .Select(x => new TestDirectory(_fileProvider, x))
                .Cast<IDirectory>();
        }

        public IEnumerable<IFile> GetFiles(SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            if (searchOption == SearchOption.TopDirectoryOnly)
            {
                string adjustedPath = Path.FullPath.EndsWith("/", StringComparison.Ordinal)
                    ? Path.FullPath.Substring(0, Path.FullPath.Length - 1)
                    : Path.FullPath;
                return _fileProvider.Files.Keys
                    .Where(x => x.StartsWith(adjustedPath)
                        && adjustedPath.Count(c => c == '/') == x.Count(c => c == '/') - 1)
                    .Select(x => new TestFile(_fileProvider, x))
                    .Cast<IFile>();
            }
            return _fileProvider.Files.Keys
                .Where(x => x.StartsWith(Path.FullPath))
                .Select(x => new TestFile(_fileProvider, x))
                .Cast<IFile>();
        }

        public IDirectory GetDirectory(DirectoryPath path)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }
            if (!path.IsRelative)
            {
                throw new ArgumentException("Path must be relative", nameof(path));
            }

            return new TestDirectory(_fileProvider, Path.Combine(path));
        }

        public IFile GetFile(FilePath path)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }
            if (!path.IsRelative)
            {
                throw new ArgumentException("Path must be relative", nameof(path));
            }

            return new TestFile(_fileProvider, Path.CombineFile(path));
        }

        public string ToDisplayString() => Path.ToSafeDisplayString();
    }
}