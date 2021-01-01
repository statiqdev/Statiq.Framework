using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using AngleSharp.Dom;
using AngleSharp.Html;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using Statiq.Common;

namespace Statiq.Html
{
    /// <summary>
    /// Mirrors external <c>link</c> and <c>script</c> resources locally.
    /// </summary>
    /// <remarks>
    /// <para>
    /// For each input document, looks for <c>link</c> and <c>script</c> elements that link to external
    /// resources and copies them locally, replacing the <c>link</c> and <c>script</c> links to the local path.
    /// To prevent local mirroring for specific <c>link</c> and <c>script</c> elements, add a
    /// <c>data-no-mirror</c> attribute.
    /// </para>
    /// <para>
    /// Note that because this module parses the document
    /// content as standards-compliant HTML and outputs the formatted post-parsed DOM, you should
    /// only place this module after all other template processing has been performed.
    /// </para>
    /// </remarks>
    /// <category>Input/Output</category>
    public class MirrorResources : Module
    {
        private readonly Func<Uri, NormalizedPath> _pathFunc;

        /// <summary>
        /// Mirrors external resources locally. By default, resources will be copied into the output folder
        /// under the <c>mirror</c> directory.
        /// </summary>
        public MirrorResources()
            : this(x => $"mirror/{x.Host}{x.LocalPath}")
        {
        }

        /// <summary>
        /// Mirrors external resources locally, specifying the output location where they should be copied.
        /// </summary>
        /// <param name="pathFunc">A function that specifies where downloaded external resources should be copied to.</param>
        public MirrorResources(Func<Uri, NormalizedPath> pathFunc)
        {
            _pathFunc = pathFunc.ThrowIfNull(nameof(pathFunc));
        }

        protected override async Task<IEnumerable<Common.IDocument>> ExecuteContextAsync(IExecutionContext context)
        {
#pragma warning disable RCS1163 // Unused parameter.
            // Handle invalid HTTPS certificates and allow alternate security protocols (see http://stackoverflow.com/a/5670954/807064)
            ServicePointManager.ServerCertificateValidationCallback = (s, cert, chain, ssl) => true;
#pragma warning restore RCS1163 // Unused parameter.

            // Cache downloaded resources
            Dictionary<string, string> mirrorCache = new Dictionary<string, string>();

            // Iterate the input documents synchronously so we don't download the same resource more than once
            return await context.Inputs
                .ToAsyncEnumerable()
                .SelectAwait(async x => await GetDocumentAsync(x))
                .ToListAsync();

            async Task<Common.IDocument> GetDocumentAsync(Common.IDocument input)
            {
                IHtmlDocument htmlDocument = await HtmlHelper.ParseHtmlAsync(input);
                if (htmlDocument is object)
                {
                    bool modifiedDocument = false;

                    // Link element
                    foreach (IElement element in htmlDocument
                            .GetElementsByTagName("link")
                            .Where(x => x.HasAttribute("href") && !x.HasAttribute("data-no-mirror")))
                    {
                        string replacement = await DownloadAndReplaceAsync(element.GetAttribute("href"), mirrorCache, context);
                        if (replacement is object)
                        {
                            element.Attributes["href"].Value = replacement;
                            element.RemoveAttribute("integrity");
                            element.RemoveAttribute("crossorigin");
                            modifiedDocument = true;
                        }
                    }

                    // Scripts
                    foreach (IHtmlScriptElement element in htmlDocument.Scripts
                            .Where(x => !string.IsNullOrEmpty(x.Source) && !x.HasAttribute("data-no-mirror")))
                    {
                        string replacement = await DownloadAndReplaceAsync(element.Source, mirrorCache, context);
                        if (replacement is object)
                        {
                            element.Source = replacement;
                            element.RemoveAttribute("integrity");
                            element.RemoveAttribute("crossorigin");
                            modifiedDocument = true;
                        }
                    }

                    // Return a new document with the replacements if we performed any
                    if (modifiedDocument)
                    {
                        using (Stream contentStream = await context.GetContentStreamAsync())
                        {
                            using (StreamWriter writer = contentStream.GetWriter())
                            {
                                htmlDocument.ToHtml(writer, ProcessingInstructionFormatter.Instance);
                                writer.Flush();
                                Common.IDocument output = input.Clone(context.GetContentProvider(contentStream, MediaTypes.Html));
                                await HtmlHelper.AddOrUpdateCacheAsync(output, htmlDocument);
                                return output;
                            }
                        }
                    }
                }

                return input;
            }
        }

        private async Task<string> DownloadAndReplaceAsync(string source, Dictionary<string, string> mirrorCache, IExecutionContext context)
        {
            if (mirrorCache.TryGetValue(source, out string cachedValue))
            {
                return cachedValue;
            }

            // Get the destination path and link
            if (source.StartsWith("//"))
            {
                source = $"http:{source}";
            }

            // Make sure it's a valid HTTP or HTTPS URI
            if (!Uri.TryCreate(source, UriKind.Absolute, out Uri uri)
                || uri is null
                || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
            {
                return null;
            }

            NormalizedPath path = _pathFunc(uri);
            if (path.IsNull)
            {
                throw new ExecutionException($"Null resource mirror path for {source}");
            }
            string link = context.GetLink(
                path,
                null,
                context.Settings.GetPath(Keys.LinkRoot),
                context.Settings.GetBool(Keys.LinksUseHttps),
                false,
                false,
                context.Settings.GetBool(Keys.LinkLowercase));

            // Download the resource, but only if we haven't already written it to disk
            IFile outputFile = context.FileSystem.GetOutputFile(path);
            if (!outputFile.Exists)
            {
                // Download the resource
                context.LogDebug($"Downloading resource from {uri} to {path.FullPath}");
                HttpResponseMessage response = await context.SendHttpRequestWithRetryAsync(uri);
                response.EnsureSuccessStatusCode();

                // Copy the result to output
                using (Stream outputStream = outputFile.OpenWrite())
                {
                    await response.Content.CopyToAsync(outputStream);
                }
            }

            mirrorCache.Add(source, link);
            return link;
        }
    }
}
