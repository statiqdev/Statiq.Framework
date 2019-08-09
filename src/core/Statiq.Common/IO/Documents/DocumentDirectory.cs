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

        public IEnumerable<IDirectory> GetDirectories(SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            if (searchOption == SearchOption.TopDirectoryOnly)
            {
                return _fileProvider.Directories
                    .Where(x => !Path.Equals(x) && Path.ContainsChild(x))
                    .Select(x => new DocumentDirectory(_fileProvider, x))
                    .Cast<IDirectory>();
            }

            return _fileProvider.Directories
                .Where(x => !Path.Equals(x) && Path.ContainsDescendant(x))
                .Select(x => new DocumentDirectory(_fileProvider, x))
                .Cast<IDirectory>();
        }

        public IDirectory GetDirectory(DirectoryPath directory)
        {
            if (directory == null)
            {
                throw new ArgumentNullException(nameof(directory));
            }
            if (!directory.IsRelative)
            {
                throw new ArgumentException("Path must be relative", nameof(directory));
            }

            return new DocumentDirectory(_fileProvider, Path.Combine(directory));
        }

        public bool Exists => _fileProvider.Directories.Contains(Path);

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

            return new DocumentFile(_fileProvider, Path.CombineFile(path));
        }

        public IEnumerable<IFile> GetFiles(SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            if (searchOption == SearchOption.TopDirectoryOnly)
            {
                return _fileProvider.Files.Keys
                    .Where(x => Path.ContainsChild(x))
                    .Select(x => new DocumentFile(_fileProvider, x))
                    .Cast<IFile>();
            }

            return _fileProvider.Files.Keys
                .Where(x => Path.ContainsDescendant(x))
                .Select(x => new DocumentFile(_fileProvider, x))
                .Cast<IFile>();
        }

        public IDirectory Parent
        {
            get
            {
                DirectoryPath parentPath = Path.Parent;
                return parentPath == null ? null : new DocumentDirectory(_fileProvider, parentPath);
            }
        }

        public void Create() => throw new NotSupportedException();

        public void Delete(bool recursive) => throw new NotSupportedException();

        public string ToDisplayString() => Path.ToDisplayString();
    }
}
