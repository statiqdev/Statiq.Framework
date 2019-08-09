using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.FileSystemGlobbing.Abstractions;

namespace Statiq.Common
{
    internal class DirectoryInfo : DirectoryInfoBase
    {
        private readonly IDirectory _directory;
        private readonly bool _isParentPath;

        public DirectoryInfo(IDirectory directory, bool isParentPath = false)
        {
            _directory = directory ?? throw new ArgumentNullException(nameof(directory));
            _isParentPath = isParentPath;
        }

        public override IEnumerable<FileSystemInfoBase> EnumerateFileSystemInfos()
        {
            if (_directory.Exists)
            {
                foreach (IDirectory childDirectory in _directory.GetDirectories())
                {
                    yield return new DirectoryInfo(childDirectory);
                }
                foreach (IFile childFile in _directory.GetFiles())
                {
                    yield return new FileInfo(childFile);
                }
            }
        }

        public override DirectoryInfoBase GetDirectory(string name)
        {
            if (string.Equals(name, "..", StringComparison.Ordinal))
            {
                return new DirectoryInfo(_directory.GetDirectory(".."), true);
            }
            return _directory.GetDirectories()
                .Where(x => x.Path.Name == name)
                .Select(x => new DirectoryInfo(x))
                .FirstOrDefault();
        }

        public override FileInfoBase GetFile(string path) => new FileInfo(_directory.GetFile(path));

        public override string Name => _isParentPath ? ".." : _directory.Path.Name;

        public override string FullName => _directory.Path.FullPath;

        public override DirectoryInfoBase ParentDirectory
        {
            get
            {
                IDirectory parent = _directory.Parent;
                return parent == null ? null : new DirectoryInfo(_directory.Parent);
            }
        }
    }
}
