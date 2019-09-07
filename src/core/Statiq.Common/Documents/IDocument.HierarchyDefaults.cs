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
        public ImmutableArray<IDocument> GetChildren(string key = Common.Keys.Children) =>
            this.GetDocumentList(key ?? throw new ArgumentNullException(nameof(key))).ToImmutableDocumentArray();

        public ImmutableArray<IDocument> GetDescendants(string key = Common.Keys.Children) => GetDescendants(false, key);

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
