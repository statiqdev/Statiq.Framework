using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Common.IO;
using Wyam.Common.Util;

namespace Wyam.Testing.IO
{
    /// <summary>
    /// A file system for testing that uses a single file provider.
    /// </summary>
    public class TestFileSystem : IFileSystem
    {
        /// <summary>
        /// The file provider to use for this file system.
        /// </summary>
        public IFileProvider FileProvider
        {
            get => FileProviders.Get(NormalizedPath.DefaultFileProvider.Scheme);
            set => FileProviders.Add(NormalizedPath.DefaultFileProvider.Scheme, value);
        }

        /// <inheritdoc />
        public IFileProviderCollection FileProviders { get; } =
            new TestFileProviderCollection(new TestFileProvider());

        /// <inheritdoc />
        IReadOnlyFileProviderCollection IReadOnlyFileSystem.FileProviders => FileProviders;

        /// <inheritdoc />
        public DirectoryPath RootPath { get; set; } = new DirectoryPath("/");

        /// <inheritdoc />
        public PathCollection<DirectoryPath> InputPaths { get; set; } = new PathCollection<DirectoryPath>(new[]
        {
            new DirectoryPath("theme"),
            new DirectoryPath("input")
        });

        IReadOnlyList<DirectoryPath> IReadOnlyFileSystem.InputPaths => InputPaths;

        /// <inheritdoc />
        public DirectoryPath OutputPath { get; set; } = "output";

        /// <inheritdoc />
        public DirectoryPath TempPath { get; set; } = "temp";

        /// <inheritdoc />
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
