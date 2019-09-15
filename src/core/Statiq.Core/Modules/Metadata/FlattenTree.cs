using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Statiq.Common;

namespace Statiq.Core
{
    /// <summary>
    /// Flattens a tree structure.
    /// </summary>
    /// <remarks>
    /// This module will either get all descendants of all input documents from
    /// a given metadata key (<see cref="Keys.Children"/> by default) or all
    /// descendants from all metadata if a <c>null</c> key is specified. The
    /// output also includes the initial input documents in both cases.
    /// </remarks>
    /// <remarks>
    /// The documents will be returned in no particular order and only distinct
    /// documents will be returned (I.e., if a document exists as a
    /// child of more than one parent, it will only appear once in the result set).
    /// </remarks>
    /// <metadata cref="Keys.Children" usage="Input"/>
    /// <category>Metadata</category>
    public class FlattenTree : SyncModule
    {
        private readonly string _childrenKey = Keys.Children;

        /// <summary>
        /// Creates a new flatten module.
        /// </summary>
        public FlattenTree()
        {
        }

        /// <summary>
        /// Creates a new flatten module with the specified key for child documents.
        /// Specify <c>null</c> to flatten all descendant documents regardless of key.
        /// </summary>
        /// <param name="childrenKey">
        /// The metadata key that contains the children or <c>null</c> to flatten all documents.
        /// </param>
        public FlattenTree(string childrenKey)
        {
            _childrenKey = childrenKey;
        }

        protected override IEnumerable<IDocument> ExecuteContext(IExecutionContext context)
        {
            // Use a stack so we don't overflow the call stack with recursive calls for deep trees
            Stack<IDocument> stack = new Stack<IDocument>(context.Inputs);
            HashSet<IDocument> results = new HashSet<IDocument>();
            while (stack.Count > 0)
            {
                IDocument current = stack.Pop();

                // Only process if we haven't already processed this document
                if (results.Add(current))
                {
                    IEnumerable<IDocument> children = _childrenKey == null
                        ? current.SelectMany(x => current.GetDocumentList(x.Key) ?? Array.Empty<IDocument>())
                        : current.GetDocumentList(_childrenKey);
                    if (children != null)
                    {
                        foreach (IDocument child in children.Where(x => x != null))
                        {
                            stack.Push(child);
                        }
                    }
                }
            }
            return results;
        }
    }
}