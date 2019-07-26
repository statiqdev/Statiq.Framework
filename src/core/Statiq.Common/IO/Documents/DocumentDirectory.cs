using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Statiq.Common
{
    internal class DocumentDirectory : IDirectory
    {
        private readonly DocumentFileProvider _fileProvider;

        internal DocumentDirectory(DocumentFileProvider fileProvider, DirectoryPath path)
        {
            _fileProvider = fileProvider ?? throw new ArgumentNullException(nameof(fileProvider));
            Path = path ?? throw new ArgumentNullException(nameof(path));
        }

        public DirectoryPath Path { get; }

        NormalizedPath IFileSystemEntry.Path => Path;

        public Task<IEnumerable<IDirectory>> GetDirectoriesAsync(SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            if (searchOption == SearchOption.TopDirectoryOnly)
            {
                return Task.FromResult(_fileProvider.Directories
                    .Where(x => !Path.Equals(x) && Path.ContainsChild(x))
                    .Select(x => new DocumentDirectory(_fileProvider, x))
                    .Cast<IDirectory>());
            }

            return Task.FromResult(_fileProvider.Directories
                .Where(x => !Path.Equals(x) && Path.ContainsDescendant(x))
                .Select(x => new DocumentDirectory(_fileProvider, x))
                .Cast<IDirectory>());
        }

        public Task<IDirectory> GetDirectoryAsync(DirectoryPath directory)
        {
            if (directory == null)
            {
                throw new ArgumentNullException(nameof(directory));
            }
            if (!directory.IsRelative)
            {
                throw new ArgumentException("Path must be relative", nameof(directory));
            }

            return Task.FromResult<IDirectory>(new DocumentDirectory(_fileProvider, Path.Combine(directory)));
        }

        public Task<bool> GetExistsAsync() => Task.FromResult(_fileProvider.Directories.Contains(Path));

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

            return Task.FromResult<IFile>(new DocumentFile(_fileProvider, Path.CombineFile(path)));
        }

        public Task<IEnumerable<IFile>> GetFilesAsync(SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            if (searchOption == SearchOption.TopDirectoryOnly)
            {
                return Task.FromResult(_fileProvider.Files.Keys
                    .Where(x => Path.ContainsChild(x))
                    .Select(x => new DocumentFile(_fileProvider, x))
                    .Cast<IFile>());
            }

            return Task.FromResult(_fileProvider.Files.Keys
                .Where(x => Path.ContainsDescendant(x))
                .Select(x => new DocumentFile(_fileProvider, x))
                .Cast<IFile>());
        }

        public Task<IDirectory> GetParentAsync()
        {
            DirectoryPath parentPath = Path.Parent;
            if (parentPath == null)
            {
                return Task.FromResult<IDirectory>(null);
            }
            return Task.FromResult<IDirectory>(new DocumentDirectory(_fileProvider, parentPath));
        }

        public Task CreateAsync() => throw new NotSupportedException();

        public Task DeleteAsync(bool recursive) => throw new NotSupportedException();

        public string ToDisplayString() => Path.ToDisplayString();
    }
}
