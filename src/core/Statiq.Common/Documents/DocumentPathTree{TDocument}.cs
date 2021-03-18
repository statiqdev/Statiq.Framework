using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Statiq.Common
{
    public class DocumentPathTree<TDocument> : IDocumentPathTree<TDocument>
        where TDocument : IDocument
    {
        // Cache the documents by their path for faster lookups
        private readonly Dictionary<NormalizedPath, TDocument> _documentsByPath = new Dictionary<NormalizedPath, TDocument>();

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

            // Add them by hand instead of using .Distinct() so we can guarantee ordering
            // and that earlier documents win (pipeline outputs are ordered with later phase results first)
            HashSet<Guid> documentIdHashes = new HashSet<Guid>();
            _documents = documents is null
                ? Array.Empty<(NormalizedPath, TDocument)>()
                : documents
                    .Where(x => documentIdHashes.Add(x.Id))
                    .Select(x => (ResolvePath(x), x))
                    .Where(x =>
                    {
                        if (!x.Item1.IsNull)
                        {
                            // Cache the path if we're saving this one
                            // Do it in the Where clause so we don't have to loop again
                            // Use TryAdd so behavior matches FirstOrDefault on the array of documents if there are more than one
                            _documentsByPath.TryAdd(x.Item1, x.x);
                            return true;
                        }
                        return false;
                    })
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

        public TDocument Get(NormalizedPath path) =>
            _documents.Select(x => x.Item2).FirstOrDefault(x => _pathFunc(x).Equals(path));

        public TDocument GetParentOf(TDocument document)
        {
            document.ThrowIfNull(nameof(document));

            NormalizedPath path = ResolvePath(document);
            if (path.IsNullOrEmpty)
            {
                return default;
            }
            return _documentsByPath.TryGetValue(path.Parent, out TDocument parent) ? parent : default;
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

            return _documents.Where(x => !x.Item1.IsNullOrEmpty && path.IsSiblingOrSelf(x.Item1) && (includeSelf || !x.Item2.IdEquals(document))).Select(x => x.Item2).ToDocumentList();
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
                if (_documentsByPath.TryGetValue(path, out TDocument parent) && !parent.IdEquals(document))
                {
                    ancestors.Add(parent);
                }
                if (path.IsNullOrEmpty)
                {
                    break;
                }
                path = path.Parent;
            }
            return ancestors.ToDocumentList();
        }
    }
}
