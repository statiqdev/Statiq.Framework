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
        private readonly System.IO.DirectoryInfo _directory;

        public DirectoryPath Path { get; }

        NormalizedPath IFileSystemEntry.Path => Path;

        public LocalDirectory(DirectoryPath path)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }
            if (path.IsRelative)
            {
                throw new ArgumentException("Path must be absolute", nameof(path));
            }

            Path = path;
            _directory = new System.IO.DirectoryInfo(Path.FullPath);
        }

        public bool Exists => _directory.Exists;

        public IDirectory Parent
        {
            get
            {
                System.IO.DirectoryInfo parent = _directory.Parent;
                return parent == null ? null : new LocalDirectory(new DirectoryPath(parent.FullName));
            }
        }

        public void Create() => LocalFileProvider.RetryPolicy.Execute(() => _directory.Create());

        public void Delete(bool recursive) => LocalFileProvider.RetryPolicy.Execute(() => _directory.Delete(recursive));

        public IEnumerable<IDirectory> GetDirectories(SearchOption searchOption = SearchOption.TopDirectoryOnly) =>
            LocalFileProvider.RetryPolicy.Execute(() =>
                _directory.GetDirectories("*", searchOption).Select(directory => (IDirectory)new LocalDirectory(directory.FullName)));

        public IEnumerable<IFile> GetFiles(SearchOption searchOption = SearchOption.TopDirectoryOnly) =>
            LocalFileProvider.RetryPolicy.Execute(() =>
                _directory.GetFiles("*", searchOption).Select(file => (IFile)new LocalFile(file.FullName)));

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

            return new LocalDirectory(Path.Combine(path));
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

            return new LocalFile(Path.CombineFile(path));
        }

        public override string ToString() => Path.ToString();

        public string ToDisplayString() => Path.ToDisplayString();
    }
}
