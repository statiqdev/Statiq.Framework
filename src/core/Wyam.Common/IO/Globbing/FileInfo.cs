using System;
using Microsoft.Extensions.FileSystemGlobbing.Abstractions;

namespace Wyam.Common.IO.Globbing
{
    internal class FileInfo : FileInfoBase
    {
        private readonly IFile _file;

        public FileInfo(IFile file)
        {
            _file = file ?? throw new ArgumentNullException(nameof(file));
        }

        public override string Name => _file.Path.FileName.FullPath;

        public override string FullName => _file.Path.FullPath;

        public override DirectoryInfoBase ParentDirectory => new DirectoryInfo(_file.GetDirectoryAsync().Result);
    }
}
