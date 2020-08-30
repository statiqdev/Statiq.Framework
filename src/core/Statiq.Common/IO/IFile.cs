using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Statiq.Common
{
    /// <summary>
    /// Represents a file. Not all implementations will support all
    /// available methods and may throw <see cref="NotSupportedException"/>.
    /// </summary>
    // Initially based on code from Cake (http://cakebuild.net/)
    public interface IFile : IFileSystemEntry, IContentProviderFactory
    {
        /// <summary>
        /// Gets the path to the file.
        /// </summary>
        /// <value>The path.</value>
        new NormalizedPath Path { get; }

        /// <summary>
        /// Gets the directory of the file.
        /// </summary>
        /// <value>
        /// The directory of the file.
        /// </value>
        IDirectory Directory { get; }

        /// <summary>
        /// Gets the length of the file.
        /// </summary>
        /// <value>The length of the file.</value>
        long Length { get; }

        /// <summary>
        /// Gets the media type of the file.
        /// </summary>
        /// <remarks>
        /// A registered IANA media type will be used if available.
        /// Unregistered media type names may be returned if a registered type is unavailable.
        /// If the media type is unknown this may be null or empty.
        /// </remarks>
        string MediaType { get; }

        /// <summary>
        /// Copies the file to the specified destination file.
        /// </summary>
        /// <param name="destination">The destination file.</param>
        /// <param name="overwrite">Will overwrite existing destination file if set to <c>true</c>.</param>
        /// <param name="createDirectory">Will create any needed directories that don't already exist if set to <c>true</c>.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        Task CopyToAsync(IFile destination, bool overwrite = true, bool createDirectory = true, CancellationToken cancellationToken = default);

        /// <summary>
        /// Moves the file to the specified destination file.
        /// </summary>
        /// <param name="destination">The destination file.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        Task MoveToAsync(IFile destination, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes the file.
        /// </summary>
        void Delete();

        /// <summary>
        /// Reads all text from the file.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>All text in the file.</returns>
        Task<string> ReadAllTextAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Writes the specified text to a file.
        /// </summary>
        /// <param name="contents">The text to write.</param>
        /// <param name="createDirectory">Will create any needed directories that don't already exist if set to <c>true</c>.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        Task WriteAllTextAsync(string contents, bool createDirectory = true, CancellationToken cancellationToken = default);

        /// <summary>
        /// Opens the file for reading. If it does not exist, an exception
        /// will be thrown.
        /// </summary>
        /// <returns>The stream.</returns>
        Stream OpenRead();

        /// <summary>
        /// Opens the file for writing. This will either create the file
        /// if it doesn't exist or overwrite it if it does.
        /// </summary>
        /// <param name="createDirectory">Will create any needed directories that don't already exist if set to <c>true</c>.</param>
        /// <returns>The stream.</returns>
        Stream OpenWrite(bool createDirectory = true);

        /// <summary>
        /// Opens the file for writing. This will either create the file
        /// if it doesn't exist or append to it if it does.
        /// </summary>
        /// <param name="createDirectory">Will create any needed directories that don't already exist if set to <c>true</c>.</param>
        /// <returns>The stream.</returns>
        Stream OpenAppend(bool createDirectory = true);

        /// <summary>
        /// Opens the file for reading and writing. This will either create the file
        /// if it doesn't exist or overwrite it if it does.
        /// </summary>
        /// <param name="createDirectory">Will create any needed directories that don't already exist if set to <c>true</c>.</param>
        /// <returns>The stream.</returns>
        Stream Open(bool createDirectory = true);
    }
}
