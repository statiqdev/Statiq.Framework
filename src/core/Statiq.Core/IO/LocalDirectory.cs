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
        private static readonly LocalCaseSensitivityChecker _caseSensitivtyChecker
            = new LocalCaseSensitivityChecker();

        private readonly DirectoryInfo _directory;

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
            _directory = new DirectoryInfo(Path.FullPath);
        }

        public bool IsCaseSensitive => _caseSensitivtyChecker.IsCaseSensitive(this);

        public Task<bool> GetExistsAsync() => Task.FromResult(_directory.Exists);

        public Task<IDirectory> GetParentAsync()
        {
            DirectoryInfo parent = _directory.Parent;
            return Task.FromResult<IDirectory>(parent == null ? null : new LocalDirectory(new DirectoryPath(parent.FullName)));
        }

        public Task CreateAsync()
        {
            LocalFileProvider.RetryPolicy.Execute(() => _directory.Create());
            return Task.CompletedTask;
        }

        public Task DeleteAsync(bool recursive)
        {
            LocalFileProvider.RetryPolicy.Execute(() => _directory.Delete(recursive));
            return Task.CompletedTask;
        }

        public Task<IEnumerable<IDirectory>> GetDirectoriesAsync(SearchOption searchOption = SearchOption.TopDirectoryOnly) =>
            Task.FromResult(LocalFileProvider.RetryPolicy.Execute(() =>
                _directory.GetDirectories("*", searchOption).Select(directory => (IDirectory)new LocalDirectory(directory.FullName))));

        public Task<IEnumerable<IFile>> GetFilesAsync(SearchOption searchOption = SearchOption.TopDirectoryOnly) =>
            Task.FromResult(LocalFileProvider.RetryPolicy.Execute(() =>
                _directory.GetFiles("*", searchOption).Select(file => (IFile)new LocalFile(file.FullName))));

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

            return Task.FromResult<IDirectory>(new LocalDirectory(Path.Combine(path)));
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

            return Task.FromResult<IFile>(new LocalFile(Path.CombineFile(path)));
        }
    }
}
