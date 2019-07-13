namespace Statiq.Common
{
    /// <summary>
    /// This is a non-generic base class for <see cref="Document{TDocument}"/> and
    /// is not intended to be used directly.
    /// </summary>
    public abstract class FactoryDocument
    {
        internal abstract IDocument Initialize(
            IMetadata defaultMetadata,
            FilePath source,
            FilePath destination,
            IMetadata metadata,
            IContentProvider contentProvider);
    }
}
