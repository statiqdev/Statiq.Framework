using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Statiq.Common;

namespace Statiq.Common
{
    public partial interface IDocument
    {
        /// <summary>
        /// Gets the first document from a sequence of documents that contains the current
        /// document as one of it's children.
        /// </summary>
        /// <param name="parents">The potential parent documents.</param>
        /// <param name="recursive">If <c>true</c> will recursively descend the candidate parent documents looking for a parent.</param>
        /// <param name="key">The metadata key containing child documents.</param>
        /// <returns>The first document from <paramref name="parents"/> that contains the current document or <c>null</c>.</returns>
        public IDocument GetParent(IEnumerable<IDocument> parents, bool recursive = true, string key = Common.Keys.Children)
        {
            _ = parents ?? throw new ArgumentNullException(nameof(parents));
            _ = key ?? throw new ArgumentNullException(nameof(key));

            IDocument parent = parents.FirstOrDefault(x => x.GetChildren(key).Contains(this));
            if (parent == null && recursive)
            {
                foreach (IDocument candidate in parents)
                {
                    parent = GetParent(candidate.GetChildren(key), true, key);
                    if (parent != null)
                    {
                        break;
                    }
                }
            }
            return parent;
        }

        /// <summary>
        /// Gets the child documents of the current document.
        /// </summary>
        /// <param name="key">The metadata key containing child documents.</param>
        /// <returns>The child documents.</returns>
        public ImmutableArray<IDocument> GetChildren(string key = Common.Keys.Children) =>
            GetDocumentList(key ?? throw new ArgumentNullException(nameof(key))).ToImmutableDocumentArray();

        /// <summary>
        /// Gets the descendant documents of the current document.
        /// </summary>
        /// <param name="key">The metadata key containing child documents.</param>
        /// <returns>The descendant documents.</returns>
        public ImmutableArray<IDocument> GetDescendants(string key = Common.Keys.Children) => GetDescendants(false, key);

        /// <summary>
        /// Gets the descendant documents of the current document and the current document.
        /// </summary>
        /// <remarks>
        /// The current document will be the first one in the result array.
        /// </remarks>
        /// <param name="key">The metadata key containing child documents.</param>
        /// <returns>The descendant documents.</returns>
        public ImmutableArray<IDocument> GetDescendantsAndSelf(string key = Common.Keys.Children) => GetDescendants(true, key);

        private ImmutableArray<IDocument> GetDescendants(in bool self, string key = Common.Keys.Children)
        {
            _ = key ?? throw new ArgumentNullException(nameof(key));

            ImmutableArray<IDocument>.Builder builder = ImmutableArray.CreateBuilder<IDocument>();

            // Use a stack so we don't overflow the call stack with recursive calls for deep trees
            Stack<IDocument> stack = new Stack<IDocument>();
            stack.Push(this);
            if (self)
            {
                builder.Add(this);
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

            return builder.ToImmutable();
        }
    }
}
