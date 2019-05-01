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
        public TestFileProvider FileProvider { get; set; } = new TestFileProvider();

        /// <inheritdoc />
        public IFileProviderCollection FileProviders
        {
            get { throw new NotImplementedException(); }
        }

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
        public IFileProvider GetFileProvider(NormalizedPath path) => FileProvider;
    }
}
