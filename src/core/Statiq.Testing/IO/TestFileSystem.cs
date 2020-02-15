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
        public NormalizedPath RootPath { get; set; } = NormalizedPath.Root;

        /// <inheritdoc />
        public PathCollection<NormalizedPath> InputPaths { get; set; } = new PathCollection<NormalizedPath>(new[]
        {
            new NormalizedPath("theme"),
            new NormalizedPath("input")
        });

        IReadOnlyList<NormalizedPath> IReadOnlyFileSystem.InputPaths => InputPaths;

        /// <inheritdoc />
        public NormalizedPath OutputPath { get; set; } = "output";

        /// <inheritdoc />
        public NormalizedPath TempPath { get; set; } = "temp";
    }
}
