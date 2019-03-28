using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.FileSystemGlobbing.Abstractions;
using Wyam.Common.IO;

namespace Wyam.Core.IO.Globbing
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
            if (_directory.GetExistsAsync().Result)
            {
                foreach (IDirectory childDirectory in _directory.GetDirectoriesAsync().Result)
                {
                    yield return new DirectoryInfo(childDirectory);
                }
                foreach (IFile childFile in _directory.GetFilesAsync().Result)
                {
                    yield return new FileInfo(childFile);
                }
            }
        }

        public override DirectoryInfoBase GetDirectory(string name)
        {
            if (string.Equals(name, "..", StringComparison.Ordinal))
            {
                return new DirectoryInfo(_directory.GetDirectoryAsync("..").Result, true);
            }
            return _directory.GetDirectoriesAsync().Result
                .Where(x => x.Path.Collapse().Name == name)
                .Select(x => new DirectoryInfo(x))
                .FirstOrDefault();
        }

        public override FileInfoBase GetFile(string path) => new FileInfo(_directory.GetFileAsync(path).Result);

        public override string Name => _isParentPath ? ".." : _directory.Path.Collapse().Name;

        public override string FullName => _directory.Path.Collapse().FullPath;

        public override DirectoryInfoBase ParentDirectory
        {
            get
            {
                IDirectory parent = _directory.GetParentAsync().Result;
                return parent == null ? null : new DirectoryInfo(_directory.GetParentAsync().Result);
            }
        }
    }
}
