using System;
using System.IO;
using System.Threading.Tasks;

namespace Wyam.Common.IO
{
    /// <summary>
    /// Represents a file. Not all implementations will support all
    /// available methods and may throw <see cref="NotSupportedException"/>.
    /// </summary>
    // Initially based on code from Cake (http://cakebuild.net/)
    public interface IFile : IFileSystemEntry
    {
        /// <summary>
        /// Gets the path to the file.
        /// </summary>
        /// <value>The path.</value>
        new FilePath Path { get; }

        /// <summary>
        /// Gets the directory of the file.
        /// </summary>
        /// <returns>
        /// The directory of the file.
        /// </returns>
        Task<IDirectory> GetDirectoryAsync();

        /// <summary>
        /// Gets the length of the file.
        /// </summary>
        /// <returns>The length of the file.</returns>
        Task<long> GetLengthAsync();

        /// <summary>
        /// Copies the file to the specified destination file.
        /// </summary>
        /// <param name="destination">The destination file.</param>
        /// <param name="overwrite">Will overwrite existing destination file if set to <c>true</c>.</param>
        /// <param name="createDirectory">Will create any needed directories that don't already exist if set to <c>true</c>.</param>
        Task CopyToAsync(IFile destination, bool overwrite = true, bool createDirectory = true);

        /// <summary>
        /// Moves the file to the specified destination file.
        /// </summary>
        /// <param name="destination">The destination file.</param>
        Task MoveToAsync(IFile destination);

        /// <summary>
        /// Deletes the file.
        /// </summary>
        Task DeleteAsync();

        /// <summary>
        /// Reads all text from the file.
        /// </summary>
        /// <returns>All text in the file.</returns>
        Task<string> ReadAllTextAsync();

        /// <summary>
        /// Writes the specified text to a file.
        /// </summary>
        /// <param name="contents">The text to write.</param>
        /// <param name="createDirectory">Will create any needed directories that don't already exist if set to <c>true</c>.</param>
        Task WriteAllTextAsync(string contents, bool createDirectory = true);

        /// <summary>
        /// Opens the file for reading. If it does not exist, an exception
        /// will be thrown.
        /// </summary>
        /// <returns>The stream.</returns>
        Task<Stream> OpenReadAsync();

        /// <summary>
        /// Opens the file for writing. This will either create the file
        /// if it doesn't exist or overwrite it if it does.
        /// </summary>
        /// <param name="createDirectory">Will create any needed directories that don't already exist if set to <c>true</c>.</param>
        /// <returns>The stream.</returns>
        Task<Stream> OpenWriteAsync(bool createDirectory = true);

        /// <summary>
        /// Opens the file for writing. This will either create the file
        /// if it doesn't exist or append to it if it does.
        /// </summary>
        /// <param name="createDirectory">Will create any needed directories that don't already exist if set to <c>true</c>.</param>
        /// <returns>The stream.</returns>
        Task<Stream> OpenAppendAsync(bool createDirectory = true);

        /// <summary>
        /// Opens the file for reading and writing. This will either create the file
        /// if it doesn't exist or overwrite it if it does.
        /// </summary>
        /// <param name="createDirectory">Will create any needed directories that don't already exist if set to <c>true</c>.</param>
        /// <returns>The stream.</returns>
        Task<Stream> OpenAsync(bool createDirectory = true);
    }
}
