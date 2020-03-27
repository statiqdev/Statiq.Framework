using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Statiq.Common;

namespace Statiq.Common
{
    public static class IDocumentHierarchyExtensions
    {
        /// <summary>
        /// Gets the first document from a sequence of documents that contains the current
        /// document as one of it's children.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <param name="parents">The potential parent documents.</param>
        /// <param name="recursive">If <c>true</c> will recursively descend the candidate parent documents looking for a parent.</param>
        /// <param name="key">The metadata key containing child documents.</param>
        /// <returns>The first document from <paramref name="parents"/> that contains the current document or <c>null</c>.</returns>
        public static IDocument GetParent(this IDocument document, IEnumerable<IDocument> parents, bool recursive = true, string key = Keys.Children)
        {
            _ = parents ?? throw new ArgumentNullException(nameof(parents));
            _ = key ?? throw new ArgumentNullException(nameof(key));

            IDocument parent = parents.FirstOrDefault(x => x.GetChildren(key).Contains(document));
            if (parent == null && recursive)
            {
                foreach (IDocument candidate in parents)
                {
                    parent = document.GetParent(candidate.GetChildren(key), true, key);
                    if (parent != null)
                    {
                        break;
                    }
                }
            }
            return parent;
        }

        /// <summary>
        /// Returns if the document has any child documents.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <param name="key">The metadata key containing child documents.</param>
        /// <returns><c>true</c> if the document contains child documents, <c>false</c> otherwise.</returns>
        public static bool HasChildren(this IDocument document, string key = Keys.Children) =>
            document.GetDocumentList(key ?? throw new ArgumentNullException(nameof(key)))?.Count > 0;

        /// <summary>
        /// Gets the child documents of the current document.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <param name="key">The metadata key containing child documents.</param>
        /// <returns>The child documents.</returns>
        public static DocumentList<IDocument> GetChildren(this IDocument document, string key = Keys.Children) =>
            document.GetDocumentList(key ?? throw new ArgumentNullException(nameof(key))).ToDocumentList();

        /// <summary>
        /// Gets the descendant documents of the current document.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <param name="key">The metadata key containing child documents.</param>
        /// <returns>The descendant documents.</returns>
        public static DocumentList<IDocument> GetDescendants(this IDocument document, string key = Keys.Children) => GetDescendants(document, false, key);

        /// <summary>
        /// Gets the descendant documents of the current document and the current document.
        /// </summary>
        /// <remarks>
        /// The current document will be the first one in the result array.
        /// </remarks>
        /// <param name="document">The document.</param>
        /// <param name="key">The metadata key containing child documents.</param>
        /// <returns>The descendant documents.</returns>
        public static DocumentList<IDocument> GetDescendantsAndSelf(this IDocument document, string key = Keys.Children) => GetDescendants(document, true, key);

        private static DocumentList<IDocument> GetDescendants(IDocument document, in bool self, string key = Keys.Children)
        {
            _ = key ?? throw new ArgumentNullException(nameof(key));

            ImmutableArray<IDocument>.Builder builder = ImmutableArray.CreateBuilder<IDocument>();

            // Use a stack so we don't overflow the call stack with recursive calls for deep trees
            Stack<IDocument> stack = new Stack<IDocument>();
            stack.Push(document);
            if (self)
            {
                builder.Add(document);
            }

            // Depth-first iterate children
            while (stack.Count > 0)
            {
                foreach (IDocument child in stack.Pop().GetChildren(key))
                {
                    stack.Push(child);
                    builder.Add(child);
                }
            }

            return builder.ToImmutable().ToDocumentList();
        }

        /// <summary>
        /// Flattens a tree structure.
        /// </summary>
        /// <remarks>
        /// This extension will either get all descendants of all documents from
        /// a given metadata key (<see cref="Keys.Children"/> by default) or all
        /// descendants from all metadata if a <c>null</c> key is specified. The
        /// result also includes the initial documents in both cases.
        /// </remarks>
        /// <remarks>
        /// The documents will be returned in no particular order and only distinct
        /// documents will be returned (I.e., if a document exists as a
        /// child of more than one parent, it will only appear once in the result set).
        /// </remarks>
        /// <param name="documents">The documents.</param>
        /// <param name="childrenKey">The metadata key that contains the children or <c>null</c> to flatten all documents.</param>
        /// <returns>The flattened documents.</returns>
        public static DocumentList<IDocument> Flatten(this IEnumerable<IDocument> documents, string childrenKey = Keys.Children)
        {
            _ = documents ?? throw new ArgumentNullException(nameof(documents));

            // Use a stack so we don't overflow the call stack with recursive calls for deep trees
            Stack<IDocument> stack = new Stack<IDocument>(documents);
            HashSet<IDocument> results = new HashSet<IDocument>();
            while (stack.Count > 0)
            {
                IDocument current = stack.Pop();

                // Only process if we haven't already processed this document
                if (results.Add(current))
                {
                    IEnumerable<IDocument> children = childrenKey == null
                        ? current.SelectMany(x => current.GetDocumentList(x.Key) ?? Array.Empty<IDocument>())
                        : current.GetDocumentList(childrenKey);
                    if (children != null)
                    {
                        foreach (IDocument child in children.Where(x => x != null))
                        {
                            stack.Push(child);
                        }
                    }
                }
            }
            return results.ToDocumentList();
        }
    }
}
