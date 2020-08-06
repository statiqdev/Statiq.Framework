using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Statiq.Common
{
    /// <summary>
    /// A filtered list of documents that also implements <see cref="IDocumentTree{TDocument}"/>
    /// so that the resulting documents can be easily traversed as a tree structure.
    /// </summary>
    /// <typeparam name="TDocument">The document type the list contains.</typeparam>
    public class FilteredDocumentList<TDocument> : DocumentList<TDocument>, IDocumentTree<TDocument>
        where TDocument : IDocument
    {
        private readonly DocumentPathTree<TDocument> _tree;

        internal FilteredDocumentList(IEnumerable<TDocument> documents, Func<TDocument, NormalizedPath> pathFunc)
            : base(documents)
        {
            _tree = new DocumentPathTree<TDocument>(documents, pathFunc);
        }

        public DocumentList<TDocument> GetAncestorsOf(TDocument document, bool includeSelf) => _tree.GetAncestorsOf(document, includeSelf);

        public DocumentList<TDocument> GetChildrenOf(TDocument document) => _tree.GetChildrenOf(document);

        public DocumentList<TDocument> GetDescendantsOf(TDocument document, bool includeSelf) => _tree.GetDescendantsOf(document, includeSelf);

        public TDocument GetParentOf(TDocument document) => _tree.GetParentOf(document);

        public DocumentList<TDocument> GetSiblingsOf(TDocument document, bool includeSelf) => _tree.GetSiblingsOf(document, includeSelf);
    }
}
