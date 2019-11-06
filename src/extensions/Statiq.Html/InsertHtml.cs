using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AngleSharp.Dom;
using AngleSharp.Dom.Html;
using AngleSharp.Parser.Html;
using Microsoft.Extensions.Logging;
using Statiq.Common;

namespace Statiq.Html
{
    /// <summary>
    /// Queries HTML content of the input documents and inserts new content into the elements that
    /// match a query selector.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Note that because this module parses the document
    /// content as standards-compliant HTML and outputs the formatted post-parsed DOM, you should
    /// only place this module after all other template processing has been performed.
    /// </para>
    /// </remarks>
    /// <category>Content</category>
    public class InsertHtml : ParallelModule
    {
        private static readonly HtmlParser HtmlParser = new HtmlParser();

        private readonly string _querySelector;
        private readonly Config<string> _content;
        private bool _first;
        private AdjacentPosition _position = AdjacentPosition.BeforeEnd;

        /// <summary>
        /// Creates the module with the specified query selector.
        /// </summary>
        /// <param name="querySelector">The query selector to use.</param>
        /// <param name="content">The content to insert as a delegate that should return a <c>string</c>.</param>
        public InsertHtml(string querySelector, Config<string> content)
        {
            _querySelector = querySelector;
            _content = content;
        }

        /// <summary>
        /// Specifies that only the first query result should be processed (the default is <c>false</c>).
        /// </summary>
        /// <param name="first">If set to <c>true</c>, only the first result is processed.</param>
        /// <returns>The current module instance.</returns>
        public InsertHtml First(bool first = true)
        {
            _first = first;
            return this;
        }

        /// <summary>
        /// Specifies where in matching elements the new content should be inserted.
        /// </summary>
        /// <param name="position">A <see cref="AdjacentPosition"/> indicating where the new content should be inserted.</param>
        /// <returns>The current module instance.</returns>
        public InsertHtml AtPosition(AdjacentPosition position = AdjacentPosition.BeforeEnd)
        {
            _position = position;
            return this;
        }

        protected override async Task<IEnumerable<Common.IDocument>> ExecuteInputAsync(Common.IDocument input, IExecutionContext context)
        {
            // Get the replacement content
            string content = await _content.GetValueAsync(input, context);
            if (content == null)
            {
                return input.Yield();
            }

            // Parse the HTML content
            IHtmlDocument htmlDocument = await input.ParseHtmlAsync(context, HtmlParser);
            if (htmlDocument == null)
            {
                return input.Yield();
            }

            // Evaluate the query selector
            try
            {
                if (!string.IsNullOrWhiteSpace(_querySelector))
                {
                    IElement[] elements = _first
                        ? new[] { htmlDocument.QuerySelector(_querySelector) }
                        : htmlDocument.QuerySelectorAll(_querySelector).ToArray();
                    if (elements.Length > 0 && elements[0] != null)
                    {
                        foreach (IElement element in elements)
                        {
                            element.Insert(_position, content);
                        }

                        using (Stream contentStream = await context.GetContentStreamAsync())
                        {
                            using (StreamWriter writer = contentStream.GetWriter())
                            {
                                htmlDocument.ToHtml(writer, ProcessingInstructionFormatter.Instance);
                                writer.Flush();
                                return input.Clone(context.GetContentProvider(contentStream, MediaTypes.Html)).Yield();
                            }
                        }
                    }
                }
                return input.Yield();
            }
            catch (Exception ex)
            {
                context.LogWarning("Exception while processing HTML for {0}: {1}", input.ToSafeDisplayString(), ex.Message);
                return input.Yield();
            }
        }
    }
}
