using System.Collections.Generic;

namespace Statiq.Common.IO
{
    /// <summary>
    /// Represents a file system.
    /// </summary>
    public interface IReadOnlyFileSystem
    {
        // Initially based on code from Cake (http://cakebuild.net/)

        /// <summary>
        /// Gets the file provider.
        /// </summary>
        /// <value>
        /// The file provider.
        /// </value>
        IFileProvider FileProvider { get; }

        /// <summary>
        /// Gets the root path.
        /// </summary>
        /// <value>
        /// The root path.
        /// </value>
        DirectoryPath RootPath { get; }

        /// <summary>
        /// Gets the input paths. These are searched in reverse order for
        /// files and directories. For example, given input paths "A", "B",
        /// and "C" in that order, "C" will be checked for a requested file
        /// or directory first, and then if it doesn't exist in "C", "B"
        /// will be checked, and then "A". If none of the input paths contain
        /// the requested file or directory, the last input path (in this case,
        /// "C") will be used as the location of the requested non-existent file
        /// or directory. If you attempt to create it at this point, it will be
        /// created under path "C".
        /// </summary>
        /// <value>
        /// The input paths.
        /// </value>
        IReadOnlyList<DirectoryPath> InputPaths { get; }

        /// <summary>
        /// Gets the output path.
        /// </summary>
        /// <value>
        /// The output path.
        /// </value>
        DirectoryPath OutputPath { get; }

        /// <summary>
        /// Gets the temporary file path.
        /// </summary>
        /// <value>
        /// The temporary file path.
        /// </value>
        DirectoryPath TempPath { get; }
    }
}
