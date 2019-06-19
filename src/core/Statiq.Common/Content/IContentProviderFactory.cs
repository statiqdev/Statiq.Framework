using Statiq.Common.Execution;

namespace Statiq.Common.Content
{
    /// <summary>
    /// Implementations of this interface can return a content provider.
    /// </summary>
    public interface IContentProviderFactory
    {
        /// <summary>
        /// Gets the content provider.
        /// </summary>
        /// <returns>The content provider to use with a document.</returns>
        IContentProvider GetContentProvider();
    }
}
