using System.Threading.Tasks;

namespace Statiq.Common.IO
{
    /// <summary>
    /// Represents an entry in the file system
    /// </summary>
    public interface IFileSystemEntry
    {
        // Initially based on code from Cake (http://cakebuild.net/)

        /// <summary>
        /// Gets the path to the entry.
        /// </summary>
        /// <value>The path.</value>
        NormalizedPath Path { get; }

        /// <summary>
        /// Gets a value indicating whether this <see cref="IFileSystemEntry"/> exists.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the entry exists; otherwise, <c>false</c>.
        /// </value>
        Task<bool> GetExistsAsync();
    }
}
