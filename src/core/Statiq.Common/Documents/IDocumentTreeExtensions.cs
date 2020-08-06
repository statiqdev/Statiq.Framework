namespace Statiq.Common
{
    public static class IDocumentTreeExtensions
    {
        public static DocumentList<TDocument> GetSiblingsOf<TDocument>(this IDocumentTree<TDocument> tree, TDocument document)
            where TDocument : IDocument =>
            tree.GetSiblingsOf(document, false);

        public static DocumentList<TDocument> GetDescendantsOf<TDocument>(this IDocumentTree<TDocument> tree, TDocument document)
            where TDocument : IDocument =>
            tree.GetDescendantsOf(document, false);

        public static DocumentList<TDocument> GetAncestorsOf<TDocument>(this IDocumentTree<TDocument> tree, TDocument document)
            where TDocument : IDocument =>
            tree.GetAncestorsOf(document, false);

        public static SingleDocumentMetadataTree AsMetadataTree(this IDocument document) => new SingleDocumentMetadataTree(document);

        public static SingleDocumentMetadataTree AsMetadataTree(this IDocument document, string childrenKey) => new SingleDocumentMetadataTree(document, childrenKey);
    }
}
