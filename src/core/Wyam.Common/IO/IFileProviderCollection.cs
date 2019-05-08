namespace Wyam.Common.IO
{
    /// <summary>
    /// A collection of file providers mapped to schemes.
    /// </summary>
    public interface IFileProviderCollection : IReadOnlyFileProviderCollection
    {
        /// <summary>
        /// Adds a file provider.
        /// </summary>
        /// <param name="scheme">The scheme the file provider supports.</param>
        /// <param name="provider">The file provider.</param>
        void Add(string scheme, IFileProvider provider);

        /// <summary>
        /// Removes a file provider by scheme.
        /// </summary>
        /// <param name="scheme">The scheme to remove.</param>
        /// <returns><c>true</c> if the provider was found and removed,
        /// <c>false</c> if the provider was not found.</returns>
        bool Remove(string scheme);
    }
}
