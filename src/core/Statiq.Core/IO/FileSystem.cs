using System;
using System.Collections.Generic;
using System.IO;
using Statiq.Common;

namespace Statiq.Core
{
    // Initially based on code from Cake (http://cakebuild.net/)
    internal class FileSystem : IFileSystem
    {
        private DirectoryPath _rootPath = Directory.GetCurrentDirectory();
        private DirectoryPath _outputPath = "output";
        private DirectoryPath _tempPath = "temp";

        public FileSystem()
        {
            FileProvider = new LocalFileProvider();
            InputPaths = new PathCollection<DirectoryPath>(new[]
            {
                new DirectoryPath("theme"),
                new DirectoryPath("input")
            });
        }

        public IFileProvider FileProvider { get; set; }

        public DirectoryPath RootPath
        {
            get => _rootPath;

            set
            {
                _ = value ?? throw new ArgumentNullException(nameof(RootPath));

                if (value.IsRelative)
                {
                    throw new ArgumentException("The root path must not be relative");
                }

                _rootPath = value;
            }
        }

        public PathCollection<DirectoryPath> InputPaths { get; }

        IReadOnlyList<DirectoryPath> IReadOnlyFileSystem.InputPaths => InputPaths;

        public DirectoryPath OutputPath
        {
            get => _outputPath;
            set => _outputPath = value ?? throw new ArgumentNullException(nameof(OutputPath));
        }

        public DirectoryPath TempPath
        {
            get => _tempPath;
            set => _tempPath = value ?? throw new ArgumentNullException(nameof(TempPath));
        }
    }
}
