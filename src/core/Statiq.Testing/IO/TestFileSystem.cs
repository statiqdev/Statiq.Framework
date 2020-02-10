using System.Collections.Generic;
using Statiq.Common;

namespace Statiq.Testing
{
    /// <summary>
    /// A file system for testing that uses a single file provider.
    /// </summary>
    public class TestFileSystem : IFileSystem
    {
        public TestFileSystem()
            : this(new TestFileProvider("input", "theme"))
        {
        }

        public TestFileSystem(IFileProvider fileProvider)
        {
            FileProvider = fileProvider;
        }

        /// <summary>
        /// The file provider to use for this file system.
        /// </summary>
        public IFileProvider FileProvider { get; set; }

        /// <inheritdoc />
        public DirectoryPath RootPath { get; set; } = DirectoryPath.RootPath;

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
    }
}
