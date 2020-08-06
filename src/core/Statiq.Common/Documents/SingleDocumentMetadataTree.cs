namespace Statiq.Common
{
    public class SingleDocumentMetadataTree
    {
        private readonly IDocument _document;
        private readonly DocumentMetadataTree<IDocument> _tree;

        public SingleDocumentMetadataTree(IDocument document)
            : this(document, null)
        {
        }

        public SingleDocumentMetadataTree(IDocument document, string childrenKey)
        {
            _document = document.ThrowIfNull(nameof(document));
            _tree = new DocumentMetadataTree<IDocument>(childrenKey);
        }

        public DocumentList<IDocument> GetChildren() => _tree.GetChildrenOf(_document);

        public DocumentList<IDocument> GetDescendants() => _tree.GetDescendantsOf(_document);

        public DocumentList<IDocument> GetDescendants(bool includeSelf) => _tree.GetDescendantsOf(_document, includeSelf);
    }
}
