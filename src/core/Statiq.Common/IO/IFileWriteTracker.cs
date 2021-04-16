using System.Collections.Generic;

namespace Statiq.Common
{
    /// <summary>
    /// Tracks the state of files being written to and their source content.
    /// This helps determine when a file should be overwritten when using <see cref="CleanMode.Changed"/>.
    /// </summary>
    public interface IFileWriteTracker
    {
        /// <summary>
        /// Resets the tracking for another execution.
        /// </summary>
        void Reset();

        /// <summary>
        /// Indicates that a file has been written to and sets a hash code
        /// that represents the file state after the write operation.
        /// </summary>
        /// <param name="path">The path that was written to.</param>
        /// <param name="hashCode">A hash code that represents the file state.</param>
        void AddWrite(NormalizedPath path, int hashCode);

        /// <summary>
        /// Gets the path and hash code for all current file writes.
        /// </summary>
        IEnumerable<KeyValuePair<NormalizedPath, int>> CurrentWrites { get; }

        /// <summary>
        /// Gets the path and hash code for all previous file writes (from before the most recent <see cref="Reset"/>.
        /// </summary>
        IEnumerable<KeyValuePair<NormalizedPath, int>> PreviousWrites { get; }
    }
}
