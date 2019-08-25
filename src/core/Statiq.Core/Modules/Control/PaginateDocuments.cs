using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Statiq.Common;

namespace Statiq.Core
{
    /// <summary>
    /// Splits a sequence of documents into multiple pages.
    /// </summary>
    /// <remarks>
    /// This module forms pages from the input documents.
    /// Note that if there are no documents to paginate, this module will still
    /// output an empty page without any documents inside the page.
    /// </remarks>
    /// <metadata cref="Keys.Children" usage="Output" />
    /// <category>Control</category>
    public class PaginateDocuments : SyncModule
    {
        private readonly int _pageSize;
        private int _takePages = int.MaxValue;
        private int _skipPages = 0;

        /// <summary>
        /// Partitions the result of the input documents into the specified number of pages.
        /// </summary>
        /// <param name="pageSize">The number of documents on each page.</param>
        public PaginateDocuments(int pageSize)
        {
            if (pageSize <= 0)
            {
                throw new ArgumentException(nameof(pageSize));
            }

            _pageSize = pageSize;
        }

        /// <summary>
        /// Only outputs a specific number of pages.
        /// </summary>
        /// <param name="count">The number of pages to output.</param>
        /// <returns>The current module instance.</returns>
        public PaginateDocuments TakePages(int count)
        {
            if (count <= 0)
            {
                throw new ArgumentException(nameof(count));
            }

            _takePages = count;
            return this;
        }

        /// <summary>
        /// Skips a specified number of pages before outputting pages.
        /// </summary>
        /// <param name="count">The number of pages to skip.</param>
        /// <returns>The current module instance.</returns>
        public PaginateDocuments SkipPages(int count)
        {
            if (count <= 0)
            {
                throw new ArgumentException(nameof(count));
            }

            _skipPages = count;
            return this;
        }

        /// <inheritdoc />
        protected override IEnumerable<IDocument> Execute(IExecutionContext context)
        {
            // Partition the pages and get a total before skip/take
            IDocument[][] pages =
                Partition(context.Inputs, _pageSize)
                .ToArray();
            int totalItems = pages.Sum(x => x.Length);

            // Skip/take the pages
            pages = pages
                .Skip(_skipPages)
                .Take(_takePages)
                .ToArray();

            // Special case for no pages, create an empty one
            if (pages.Length == 0)
            {
                pages = new[] { Array.Empty<IDocument>() };
            }

            // Create the documents per page
            return pages.Select(children => context.CreateDocument(new MetadataItems { { Keys.Children, children } }));
        }

        // Interesting discussion of partitioning at
        // http://stackoverflow.com/questions/419019/split-list-into-sublists-with-linq
        // Note that this implementation won't work for very long sequences because it enumerates twice per chunk
        private static IEnumerable<T[]> Partition<T>(IReadOnlyList<T> source, int size)
        {
            int pos = 0;
            while (source.Skip(pos).Any())
            {
                yield return source.Skip(pos).Take(size).ToArray();
                pos += size;
            }
        }
    }
}
