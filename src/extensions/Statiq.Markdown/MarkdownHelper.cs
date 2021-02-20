using System;
using System.IO;
using Markdig;
using Markdig.Helpers;
using Markdig.Parsers;
using Markdig.Renderers;
using Markdig.Syntax;
using Microsoft.Extensions.Logging;
using Statiq.Common;

namespace Statiq.Markdown
{
    public static class MarkdownHelper
    {
        /// <summary>
        /// The default Markdown configuration.
        /// </summary>
        public const string DefaultConfiguration = "common";

        public static MarkdownDocument RenderMarkdown(
            IDocument document,
            string content,
            TextWriter writer,
            bool prependLinkRoot = false,
            string configuration = DefaultConfiguration,
            OrderedList<IMarkdownExtension> extensions = null)
        {
            document.ThrowIfNull(nameof(document));
            content.ThrowIfNull(nameof(content));
            configuration.ThrowIfNullOrEmpty(nameof(configuration));

            try
            {
                // Create the pipeline
                MarkdownPipelineBuilder pipelineBuilder = new MarkdownPipelineBuilder();
                pipelineBuilder.Configure(configuration);
                if (extensions is object)
                {
                    pipelineBuilder.Extensions.AddRange(extensions);
                }
                MarkdownPipeline pipeline = pipelineBuilder.Build();

                // Render the content
                HtmlRenderer htmlRenderer = new HtmlRenderer(writer);
                pipeline.Setup(htmlRenderer);

                htmlRenderer.LinkRewriter = (link) =>
                {
                    if (string.IsNullOrEmpty(link))
                    {
                        return link;
                    }

                    if (LinkGenerator.TryGetAbsoluteHttpUri(link, out string absoluteUri))
                    {
                        return absoluteUri;
                    }

                    // TODO: Remove when RenderMarkdown.PrependLinkRoot goes away.
                    if (prependLinkRoot)
                    {
                        if (!link.StartsWith("/"))
                        {
                            link = "/" + link;
                        }

                        link = "~" + link;
                    }

                    RelativeUrl relativeUrl = new RelativeUrl(link, document.GetString(Keys.LinkRoot));

                    return relativeUrl.ToString();
                };

                MarkdownDocument markdownDocument = MarkdownParser.Parse(content, pipeline);
                htmlRenderer.Render(markdownDocument);
                writer.Flush();
                return markdownDocument;
            }
            catch (Exception ex)
            {
                document.LogWarning($"Exception while rendering Markdown: {ex.Message}");
            }
            return null;
        }
    }
}