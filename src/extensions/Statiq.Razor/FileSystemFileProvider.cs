using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;
using Statiq.Common;
using IFileProvider = Microsoft.Extensions.FileProviders.IFileProvider;

namespace Statiq.Razor
{
    /// <summary>
    /// Looks up files using the Statiq virtual file system.
    /// </summary>
    internal class FileSystemFileProvider : IFileProvider
    {
        private readonly ConcurrentBag<ExecutionChangeToken> _executionChangeTokens =
            new ConcurrentBag<ExecutionChangeToken>();

        public IReadOnlyFileSystem StatiqFileSystem { get; }

        public FileSystemFileProvider(IReadOnlyFileSystem fileSystem)
        {
            StatiqFileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
        }

        public IFileInfo GetFileInfo(string subpath)
        {
            if (string.IsNullOrEmpty(subpath))
            {
                return new NotFoundFileInfo(subpath);
            }
            IFile file = StatiqFileSystem.GetInputFile(subpath.TrimStart('/'));
            return new StatiqFileInfo(file);
        }

        public IDirectoryContents GetDirectoryContents(string subpath)
        {
            if (subpath == null)
            {
                return new NotFoundDirectoryContents();
            }
            IDirectory directory = StatiqFileSystem.GetInputDirectory(subpath);
            List<IFileInfo> fileInfos = new List<IFileInfo>();
            fileInfos.AddRange(directory.GetDirectories().Select(x => new StatiqDirectoryInfo(x)));
            fileInfos.AddRange(directory.GetFiles().Select(x => new StatiqFileInfo(x)));
            return new EnumerableDirectoryContents(fileInfos);
        }

        IChangeToken IFileProvider.Watch(string filter)
        {
            ExecutionChangeToken token = new ExecutionChangeToken();
            _executionChangeTokens.Add(token);
            return token;
        }

        public void ExpireChangeTokens()
        {
            ExecutionChangeToken token;
            while (_executionChangeTokens.TryTake(out token))
            {
                token.Expire();
            }
        }

        private class EnumerableDirectoryContents : IDirectoryContents
        {
            private readonly IEnumerable<IFileInfo> _entries;

            public bool Exists => true;

            public EnumerableDirectoryContents(IEnumerable<IFileInfo> entries)
            {
                _entries = entries;
            }

            public IEnumerator<IFileInfo> GetEnumerator() => _entries.GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => _entries.GetEnumerator();
        }

        private class StatiqFileInfo : IFileInfo
        {
            private readonly IFile _file;

            public StatiqFileInfo(IFile file)
            {
                _file = file;
            }

            public bool Exists => _file.Exists;

            public long Length => _file.Length;

            public string PhysicalPath => _file.Path.FullPath;

            public string Name => _file.Path.FileName.FullPath;

            public DateTimeOffset LastModified => DateTimeOffset.Now;

            public bool IsDirectory => false;

            public Stream CreateReadStream() => _file.OpenRead();
        }

        private class StatiqDirectoryInfo : IFileInfo
        {
            private readonly IDirectory _directory;

            public StatiqDirectoryInfo(IDirectory directory)
            {
                _directory = directory;
            }

            public bool Exists => _directory.Exists;

            public long Length => -1L;

            public string PhysicalPath => _directory.Path.FullPath;

            public string Name => _directory.Path.Name;

            public DateTimeOffset LastModified => DateTimeOffset.Now;

            public bool IsDirectory => true;

            public Stream CreateReadStream()
            {
                throw new InvalidOperationException("Cannot create a stream for a directory.");
            }
        }
    }
}
