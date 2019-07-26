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

        public Task<bool> GetExistsAsync() => Task.FromResult(_fileProvider.Directories.Contains(Path.FullPath));

        public Task<IDirectory> GetParentAsync()
        {
            DirectoryPath parentPath = Path.Parent;
            if (parentPath == null)
            {
                return Task.FromResult<IDirectory>(null);
            }
            return Task.FromResult<IDirectory>(new TestDirectory(_fileProvider, parentPath));
        }

        public Task CreateAsync()
        {
            _fileProvider.Directories.Add(Path.FullPath);
            return Task.CompletedTask;
        }

        public Task DeleteAsync(bool recursive)
        {
            _fileProvider.Directories.Remove(Path.FullPath);
            return Task.CompletedTask;
        }

        public Task<IEnumerable<IDirectory>> GetDirectoriesAsync(SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            if (searchOption == SearchOption.TopDirectoryOnly)
            {
                string adjustedPath = Path.FullPath.EndsWith("/", StringComparison.Ordinal)
                    ? Path.FullPath.Substring(0, Path.FullPath.Length - 1)
                    : Path.FullPath;
                return Task.FromResult(_fileProvider.Directories
                    .Where(x => x.StartsWith(adjustedPath + "/")
                        && adjustedPath.Count(c => c == '/') == x.Count(c => c == '/') - 1
                        && Path.FullPath != x)
                    .Select(x => new TestDirectory(_fileProvider, x))
                    .Cast<IDirectory>());
            }
            return Task.FromResult(_fileProvider.Directories
                .Where(x => x.StartsWith(Path.FullPath + "/") && Path.FullPath != x)
                .Select(x => new TestDirectory(_fileProvider, x))
                .Cast<IDirectory>());
        }

        public Task<IEnumerable<IFile>> GetFilesAsync(SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            if (searchOption == SearchOption.TopDirectoryOnly)
            {
                string adjustedPath = Path.FullPath.EndsWith("/", StringComparison.Ordinal)
                    ? Path.FullPath.Substring(0, Path.FullPath.Length - 1)
                    : Path.FullPath;
                return Task.FromResult(_fileProvider.Files.Keys
                    .Where(x => x.StartsWith(adjustedPath)
                        && adjustedPath.Count(c => c == '/') == x.Count(c => c == '/') - 1)
                    .Select(x => new TestFile(_fileProvider, x))
                    .Cast<IFile>());
            }
            return Task.FromResult(_fileProvider.Files.Keys
                .Where(x => x.StartsWith(Path.FullPath))
                .Select(x => new TestFile(_fileProvider, x))
                .Cast<IFile>());
        }

        public Task<IDirectory> GetDirectoryAsync(DirectoryPath path)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }
            if (!path.IsRelative)
            {
                throw new ArgumentException("Path must be relative", nameof(path));
            }

            return Task.FromResult<IDirectory>(new TestDirectory(_fileProvider, Path.Combine(path)));
        }

        public Task<IFile> GetFileAsync(FilePath path)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }
            if (!path.IsRelative)
            {
                throw new ArgumentException("Path must be relative", nameof(path));
            }

            return Task.FromResult<IFile>(new TestFile(_fileProvider, Path.CombineFile(path)));
        }

        public string ToDisplayString() => Path.ToSafeDisplayString();
    }
}