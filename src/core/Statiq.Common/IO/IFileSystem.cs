namespace Statiq.Common
{
    /// <summary>
    /// A file system that can be configured.
    /// </summary>
    public interface IFileSystem : IReadOnlyFileSystem
    {
        /// <summary>
        /// Gets the file provider.
        /// </summary>
        /// <value>
        /// The file provider.
        /// </value>
        new IFileProvider FileProvider { get; set; }

        /// <summary>
        /// Gets or sets the root path.
        /// </summary>
        /// <value>
        /// The root path.
        /// </value>
        new NormalizedPath RootPath { get; set; }

        /// <summary>
        /// Gets the input paths collection which can be used
        /// to add or remove input paths.
        /// </summary>
        /// <value>
        /// The input paths.
        /// </value>
        new PathCollection InputPaths { get; }

        /// <summary>
        /// Gets or sets the output path.
        /// </summary>
        /// <value>
        /// The output path.
        /// </value>
        new NormalizedPath OutputPath { get; set; }

        /// <summary>
        /// Gets or sets the temporary file path.
        /// </summary>
        new NormalizedPath TempPath { get; set; }
    }
}
