using System;
using System.Collections.Generic;
using System.IO;
using Wyam.Common.IO;
using Wyam.Core.IO.FileProviders.Local;

namespace Wyam.Core.IO
{
    // Initially based on code from Cake (http://cakebuild.net/)
    internal class FileSystem : IFileSystem
    {
        private DirectoryPath _rootPath = Directory.GetCurrentDirectory();
        private DirectoryPath _outputPath = "output";
        private DirectoryPath _tempPath = "temp";

        public FileSystem()
        {
            FileProviders = new FileProviderCollection(new LocalFileProvider());
            InputPaths = new PathCollection<DirectoryPath>(new[]
            {
                new DirectoryPath("theme"),
                new DirectoryPath("input")
            });
        }

        public IFileProviderCollection FileProviders { get; }

        IReadOnlyFileProviderCollection IReadOnlyFileSystem.FileProviders => FileProviders;

        public DirectoryPath RootPath
        {
            get => _rootPath;

            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(RootPath));
                }
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
            get
            {
                return _outputPath;
            }

            set
            {
                _outputPath = value ?? throw new ArgumentNullException(nameof(OutputPath));
            }
        }

        public DirectoryPath TempPath
        {
            get
            {
                return _tempPath;
            }

            set
            {
                _tempPath = value ?? throw new ArgumentNullException(nameof(TempPath));
            }
        }

        public IFileProvider GetFileProvider(NormalizedPath path)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }
            if (path.IsRelative)
            {
                throw new ArgumentException("The path must be absolute");
            }
            if (path.FileProvider == null)
            {
                throw new ArgumentException("The path has no provider");
            }
            if (!FileProviders.TryGet(path.FileProvider.Scheme, out IFileProvider fileProvider))
            {
                throw new KeyNotFoundException($"A provider for the scheme {path.FileProvider} could not be found");
            }
            return fileProvider;
        }
    }
}
