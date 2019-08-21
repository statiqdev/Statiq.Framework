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
        public static ImmutableArray<IDocument> GetChildren(this IDocument document)
        {
            _ = document ?? throw new ArgumentNullException(nameof(document));

            return document.DocumentList(Keys.Children).ToImmutableDocumentArray();
        }

        public static ImmutableArray<IDocument> GetDescendants(this IDocument document) =>
            GetDescendants(document, false);

        public static ImmutableArray<IDocument> GetDescendantsAndSelf(this IDocument document) =>
            GetDescendants(document, true);

        private static ImmutableArray<IDocument> GetDescendants(IDocument document, in bool self)
        {
            _ = document ?? throw new ArgumentNullException(nameof(document));

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
                foreach (IDocument child in stack.Pop().GetChildren())
                {
                    stack.Push(child);
                    builder.Add(child);
                }
            }

            return builder.ToImmutable();
        }
    }
}
