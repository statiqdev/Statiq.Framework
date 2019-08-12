using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Statiq.Common;

namespace Statiq.Core
{
    /// <summary>
    /// Creates new documents.
    /// </summary>
    /// <remarks>
    /// This module does not include the input documents as part of it's output.
    /// </remarks>
    /// <category>Control</category>
    public class CreateDocuments : Module
    {
        private readonly Config<IEnumerable<IDocument>> _documents;

        /// <summary>
        /// Generates a specified number of new empty documents.
        /// </summary>
        /// <param name="count">The number of new documents to output.</param>
        public CreateDocuments(int count)
        {
            _documents = Config.FromContext(ctx =>
            {
                List<IDocument> documents = new List<IDocument>();
                for (int c = 0; c < count; c++)
                {
                    documents.Add(ctx.CreateDocument());
                }
                return (IEnumerable<IDocument>)documents;
            });
        }

        /// <summary>
        /// Generates new documents with the specified content.
        /// </summary>
        /// <param name="content">The content for each output document.</param>
        public CreateDocuments(params string[] content)
            : this((IEnumerable<string>)content)
        {
        }

        /// <summary>
        /// Generates new documents with the specified content.
        /// </summary>
        /// <param name="content">The content for each output document.</param>
        public CreateDocuments(IEnumerable<string> content)
        {
            _documents = Config.FromContext(async ctx =>
                (IEnumerable<IDocument>)await content
                    .ToAsyncEnumerable()
                    .SelectAwait(async x => ctx.CreateDocument(await ctx.GetContentProviderAsync(x)))
                    .ToListAsync(ctx.CancellationToken));
        }

        /// <summary>
        /// Generates new documents with the specified metadata.
        /// </summary>
        /// <param name="metadata">The metadata for each output document.</param>
        public CreateDocuments(params IEnumerable<KeyValuePair<string, object>>[] metadata)
            : this((IEnumerable<IEnumerable<KeyValuePair<string, object>>>)metadata)
        {
        }

        /// <summary>
        /// Generates new documents with the specified metadata.
        /// </summary>
        /// <param name="metadata">The metadata for each output document.</param>
        public CreateDocuments(IEnumerable<IEnumerable<KeyValuePair<string, object>>> metadata)
        {
            _documents = Config.FromContext(ctx => metadata.Select(x => ctx.CreateDocument(x)));
        }

        /// <summary>
        /// Generates new documents with the specified content and metadata.
        /// </summary>
        /// <param name="contentAndMetadata">The content and metadata for each output document.</param>
        public CreateDocuments(params Tuple<string, IEnumerable<KeyValuePair<string, object>>>[] contentAndMetadata)
            : this((IEnumerable<Tuple<string, IEnumerable<KeyValuePair<string, object>>>>)contentAndMetadata)
        {
        }

        /// <summary>
        /// Generates new documents with the specified content and metadata.
        /// </summary>
        /// <param name="contentAndMetadata">The content and metadata for each output document.</param>
        public CreateDocuments(IEnumerable<Tuple<string, IEnumerable<KeyValuePair<string, object>>>> contentAndMetadata)
        {
            _documents = Config.FromContext(async ctx =>
                (IEnumerable<IDocument>)await contentAndMetadata
                    .ToAsyncEnumerable()
                    .SelectAwait(async x => ctx.CreateDocument(x.Item2, await ctx.GetContentProviderAsync(x.Item1)))
                    .ToListAsync(ctx.CancellationToken));
        }

        public override async Task<IEnumerable<IDocument>> ExecuteAsync(IExecutionContext context) =>
            _documents.RequiresDocument
                ? await context.Inputs
                    .ToAsyncEnumerable()
                    .SelectManyAwait(async input => (await _documents.GetValueAsync(input, context)).ToAsyncEnumerable())
                    .ToListAsync(context.CancellationToken)
                : await _documents.GetValueAsync(null, context);
    }
}