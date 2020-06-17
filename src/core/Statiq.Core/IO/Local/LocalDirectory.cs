using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Statiq.Common;

namespace Statiq.Core
{
    // Initially based on code from Cake (http://cakebuild.net/)
    internal class LocalDirectory : IDirectory
    {
        private readonly IReadOnlyFileSystem _fileSystem;
        private readonly System.IO.DirectoryInfo _directory;

        public LocalDirectory(IReadOnlyFileSystem fileSystem, NormalizedPath path)
        {
            _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));

            path.ThrowIfNull(nameof(path));

            if (path.IsRelative)
            {
                throw new ArgumentException("Path must be absolute", nameof(path));
            }

            Path = path;
            _directory = new System.IO.DirectoryInfo(Path.FullPath);
        }

        public NormalizedPath Path { get; }

        NormalizedPath IFileSystemEntry.Path => Path;

        public bool Exists => _directory.Exists;

        public DateTime LastWriteTime => _directory.LastWriteTime;

        public DateTime CreationTime => _directory.CreationTime;

        public IDirectory Parent
        {
            get
            {
                System.IO.DirectoryInfo parent = _directory.Parent;
                return parent == null ? null : _fileSystem.GetDirectory(new NormalizedPath(parent.FullName));
            }
        }

        public void Create() => LocalFileProvider.RetryPolicy.Execute(() => _directory.Create());

        public void Delete(bool recursive) => LocalFileProvider.RetryPolicy.Execute(() => _directory.Delete(recursive));

        public IEnumerable<IDirectory> GetDirectories(SearchOption searchOption = SearchOption.TopDirectoryOnly) =>
            LocalFileProvider.RetryPolicy.Execute(() =>
                _directory.GetDirectories("*", searchOption).Select(directory => _fileSystem.GetDirectory(directory.FullName)));

        public IEnumerable<IFile> GetFiles(SearchOption searchOption = SearchOption.TopDirectoryOnly) =>
            LocalFileProvider.RetryPolicy.Execute(() =>
                _directory.GetFiles("*", searchOption).Select(file => _fileSystem.GetFile(file.FullName)));

        public IDirectory GetDirectory(NormalizedPath path)
        {
            path.ThrowIfNull(nameof(path));

            if (!path.IsRelative)
            {
                throw new ArgumentException("Path must be relative", nameof(path));
            }

            return _fileSystem.GetDirectory(Path.Combine(path));
        }

        public IFile GetFile(NormalizedPath path)
        {
            path.ThrowIfNull(nameof(path));

            if (!path.IsRelative)
            {
                throw new ArgumentException("Path must be relative", nameof(path));
            }

            return _fileSystem.GetFile(Path.Combine(path));
        }

        public override string ToString() => Path.ToString();

        public string ToDisplayString() => Path.ToDisplayString();
    }
}
