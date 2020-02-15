using System;
using System.Collections.Generic;
using System.IO;
using Statiq.Common;

namespace Statiq.Core
{
    // Initially based on code from Cake (http://cakebuild.net/)
    internal class FileSystem : IFileSystem
    {
        private NormalizedPath _rootPath = Directory.GetCurrentDirectory();
        private NormalizedPath _outputPath = "output";
        private NormalizedPath _tempPath = "temp";

        public FileSystem()
        {
            FileProvider = new LocalFileProvider();
            InputPaths = new PathCollection<NormalizedPath>(new[]
            {
                new NormalizedPath("theme"),
                new NormalizedPath("input")
            });
        }

        public IFileProvider FileProvider { get; set; }

        public NormalizedPath RootPath
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

        public PathCollection<NormalizedPath> InputPaths { get; }

        IReadOnlyList<NormalizedPath> IReadOnlyFileSystem.InputPaths => InputPaths;

        public NormalizedPath OutputPath
        {
            get => _outputPath;
            set => _outputPath = value ?? throw new ArgumentNullException(nameof(OutputPath));
        }

        public NormalizedPath TempPath
        {
            get => _tempPath;
            set => _tempPath = value ?? throw new ArgumentNullException(nameof(TempPath));
        }
    }
}
