using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Statiq.Common
{
    public class DocumentPathTree<TDocument> : IDocumentTree<TDocument>
        where TDocument : IDocument
    {
        private readonly Func<TDocument, NormalizedPath> _pathFunc;
        private readonly string _indexFileName;
        private readonly (NormalizedPath, TDocument)[] _documents;

        public DocumentPathTree(IEnumerable<TDocument> documents, Func<TDocument, NormalizedPath> pathFunc)
            : this(documents, pathFunc, IExecutionContext.Current.Settings.GetIndexFileName())
        {
        }

        public DocumentPathTree(IEnumerable<TDocument> documents, Func<TDocument, NormalizedPath> pathFunc, string indexFileName)
        {
            _pathFunc = pathFunc.ThrowIfNull(nameof(pathFunc));
            _indexFileName = indexFileName.ThrowIfNullOrEmpty(nameof(indexFileName));
            _documents = documents
                .ThrowIfNull(nameof(documents))
                .Select(x => (ResolvePath(x), x))
                .Where(x => !x.Item1.IsNull)
                .ToArray();
        }

        private NormalizedPath ResolvePath(TDocument document)
        {
            NormalizedPath path = _pathFunc(document);
            if (path.IsNullOrEmpty)
            {
                return default;
            }

            // Treat an index file as if it were the directory
            if (path.FileName.Equals(_indexFileName))
            {
                path = path.Parent;
                if (path.IsNull)
                {
                    return default;
                }
            }

            return path;
        }

        public TDocument GetParentOf(TDocument document)
        {
            document.ThrowIfNull(nameof(document));

            NormalizedPath path = ResolvePath(document);
            if (path.IsNullOrEmpty)
            {
                return default;
            }

            path = path.Parent;
            return Array.Find(_documents, x => x.Item1.Equals(path)).Item2;
        }

        public DocumentList<TDocument> GetChildrenOf(TDocument document)
        {
            document.ThrowIfNull(nameof(document));

            NormalizedPath path = ResolvePath(document);
            if (path.IsNull)
            {
                return DocumentList<TDocument>.Empty;
            }

            return _documents.Where(x => path.ContainsChild(x.Item1) && !x.Item2.IdEquals(document)).Select(x => x.Item2).ToDocumentList();
        }

        public DocumentList<TDocument> GetSiblingsOf(TDocument document, bool includeSelf)
        {
            document.ThrowIfNull(nameof(document));

            NormalizedPath path = ResolvePath(document);
            if (path.IsNullOrEmpty)
            {
                return DocumentList<TDocument>.Empty;
            }

            return _documents.Where(x => !x.Item1.IsEmpty && path.IsSiblingOrSelf(x.Item1) && (includeSelf || !x.Item2.IdEquals(document))).Select(x => x.Item2).ToDocumentList();
        }

        public DocumentList<TDocument> GetDescendantsOf(TDocument document, bool includeSelf)
        {
            document.ThrowIfNull(nameof(document));

            NormalizedPath path = ResolvePath(document);
            if (path.IsNull)
            {
                return DocumentList<TDocument>.Empty;
            }

            return _documents.Where(x => path.ContainsDescendantOrSelf(x.Item1) && (includeSelf || !x.Item2.IdEquals(document))).Select(x => x.Item2).ToDocumentList();
        }

        public DocumentList<TDocument> GetAncestorsOf(TDocument document, bool includeSelf)
        {
            document.ThrowIfNull(nameof(document));

            NormalizedPath path = ResolvePath(document);
            if (path.IsNull)
            {
                return DocumentList<TDocument>.Empty;
            }

            // We want to build the ancestors up from closest to furthest
            List<TDocument> ancestors = new List<TDocument>();
            if (includeSelf)
            {
                ancestors.Add(document);
            }
            path = path.Parent;
            while (!path.IsNull)
            {
                (NormalizedPath, TDocument) ancestor = Array.Find(_documents, x => x.Item1.Equals(path));
                if (ancestor.Item2 is object && !ancestor.Item2.IdEquals(document))
                {
                    ancestors.Add(ancestor.Item2);
                }
                if (path.IsEmpty)
                {
                    break;
                }
                path = path.Parent;
            }
            return ancestors.ToDocumentList();
        }
    }
}
