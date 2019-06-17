namespace Statiq.Common.IO
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
        new DirectoryPath RootPath { get; set; }

        /// <summary>
        /// Gets the input paths collection which can be used
        /// to add or remove input paths.
        /// </summary>
        /// <value>
        /// The input paths.
        /// </value>
        new PathCollection<DirectoryPath> InputPaths { get; }

        /// <summary>
        /// Gets or sets the output path.
        /// </summary>
        /// <value>
        /// The output path.
        /// </value>
        new DirectoryPath OutputPath { get; set; }

        /// <summary>
        /// Gets or sets the temporary file path.
        /// </summary>
        new DirectoryPath TempPath { get; set; }
    }
}
