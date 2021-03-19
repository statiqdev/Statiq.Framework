using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Statiq.Common;

namespace Statiq.Core
{
    // Initially based on code from Cake (http://cakebuild.net/)
    internal class LocalDirectory : IDirectory
    {
        private readonly LocalFileProvider _fileProvider;
        private readonly System.IO.DirectoryInfo _directory;

        public LocalDirectory(LocalFileProvider fileProvider, in NormalizedPath path)
        {
            _fileProvider = fileProvider.ThrowIfNull(nameof(fileProvider));
            path.ThrowIfNull(nameof(path));
            path.ThrowIfRelative(nameof(path));

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
                return parent is null ? null : _fileProvider.FileSystem.GetDirectory(new NormalizedPath(parent.FullName));
            }
        }

        public void Create() => LocalFileProvider.RetryPolicy.Execute(() => _directory.Create());

        public void Delete(bool recursive) => LocalFileProvider.RetryPolicy.Execute(() => _directory.Delete(recursive));

        public void MoveTo(NormalizedPath path)
        {
            path.ThrowIfNull(nameof(path));
            path.ThrowIfRelative(nameof(path));
            LocalFileProvider.RetryPolicy.Execute(() => _directory.MoveTo(path.FullPath));
        }

        public IEnumerable<IDirectory> GetDirectories(SearchOption searchOption = SearchOption.TopDirectoryOnly) =>
            LocalFileProvider.RetryPolicy.Execute(() => _directory
                .GetDirectories("*", searchOption)
                .Select(directory => _fileProvider.FileSystem.GetDirectory(directory.FullName))
                .Where(x => x.Exists));

        public IEnumerable<IFile> GetFiles(SearchOption searchOption = SearchOption.TopDirectoryOnly) =>
            LocalFileProvider.RetryPolicy.Execute(() => _directory
                .GetFiles("*", searchOption)
                .Select(file => _fileProvider.FileSystem.GetFile(file.FullName))
                .Where(x => x.Exists));

        public IDirectory GetDirectory(NormalizedPath path)
        {
            path.ThrowIfNull(nameof(path));
            path.ThrowIfAbsolute(nameof(path));

            return _fileProvider.FileSystem.GetDirectory(Path.Combine(path));
        }

        public IFile GetFile(NormalizedPath path)
        {
            path.ThrowIfNull(nameof(path));
            path.ThrowIfAbsolute(nameof(path));

            return _fileProvider.FileSystem.GetFile(Path.Combine(path));
        }

        public override string ToString() => Path.ToString();

        public string ToDisplayString() => Path.ToDisplayString();
    }
}
