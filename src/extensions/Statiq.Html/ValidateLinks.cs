using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using AngleSharp.Dom;
using AngleSharp.Dom.Html;
using AngleSharp.Parser.Html;
using Microsoft.Extensions.Logging;
using Statiq.Common;
using IDocument = Statiq.Common.IDocument;

namespace Statiq.Html
{
    /// <summary>
    /// Performs link validation for HTML elements such as anchors, images, and other resources.
    /// </summary>
    /// <remarks>
    /// Both relative and absolute links can be validated, though only relative links are checked
    /// by default due to the time it takes to check absolute links.
    /// </remarks>
    /// <category>Input/Output</category>
    public class ValidateLinks : Module
    {
        private const HttpStatusCode TooManyRequests = (HttpStatusCode)429;

        private Config<bool> _validateAbsoluteLinks;
        private Config<bool> _validateRelativeLinks = true;
        private Config<bool> _asError;

        /// <summary>
        /// Validates absolute (often external) links. This may add a significant delay to your
        /// generation process so it's recommended absolute links are only checked periodically.
        /// The default behavior is not to check absolute links. Also note that false positive
        /// failures are common when validating external links so any links that fail the check
        /// should be subsequently checked manually.
        /// </summary>
        /// <param name="validateAbsoluteLinks"><c>true</c> to validate absolute links.</param>
        /// <returns>The current module instance.</returns>
        public ValidateLinks ValidateAbsoluteLinks(Config<bool> validateAbsoluteLinks)
        {
            if (validateAbsoluteLinks.RequiresDocument)
            {
                throw new ArgumentException(nameof(validateAbsoluteLinks) + " should not require a document");
            }
            _validateAbsoluteLinks = validateAbsoluteLinks;
            return this;
        }

        /// <summary>
        /// Validates relative links, which is activated by default.
        /// </summary>
        /// <param name="validateRelativeLinks"><c>true</c> to validate relative links.</param>
        /// <returns>The current module instance.</returns>
        public ValidateLinks ValidateRelativeLinks(Config<bool> validateRelativeLinks)
        {
            if (validateRelativeLinks.RequiresDocument)
            {
                throw new ArgumentException(nameof(validateRelativeLinks) + " should not require a document");
            }
            _validateRelativeLinks = validateRelativeLinks;
            return this;
        }

        /// <summary>
        /// When the validation process is complete, all the validation failures will
        /// be output as warnings. This method can be used to report all of the failures
        /// as errors instead (possibly breaking the generation).
        /// </summary>
        /// <param name="asError"><c>true</c> to report failures as an error.</param>
        /// <returns>The current module instance.</returns>
        public ValidateLinks AsError(Config<bool> asError)
        {
            if (asError.RequiresDocument)
            {
                throw new ArgumentException(nameof(asError) + " should not require a document");
            }
            _asError = asError;
            return this;
        }

        /// <inheritdoc />
        protected override async Task<IEnumerable<IDocument>> ExecuteContextAsync(IExecutionContext context)
        {
#pragma warning disable RCS1163 // Unused parameter.
            // Handle invalid HTTPS certificates and allow alternate security protocols (see http://stackoverflow.com/a/5670954/807064)
            ServicePointManager.ServerCertificateValidationCallback = (s, cert, chain, ssl) => true;
#pragma warning restore RCS1163 // Unused parameter.

            // Get settings
            bool validateAbsoluteLinks = _validateAbsoluteLinks is object && await _validateAbsoluteLinks.GetValueAsync(null, context);
            bool validateRelativeLinks = _validateRelativeLinks is object && await _validateRelativeLinks.GetValueAsync(null, context);
            bool asError = _asError is object && await _asError.GetValueAsync(null, context);

            // Key = link, Value = source, tag HTML
            ConcurrentDictionary<string, ConcurrentBag<(IDocument documentSource, string outerHtml)>> links =
                new ConcurrentDictionary<string, ConcurrentBag<(IDocument documentSource, string outerHtml)>>();

            // Key = source, Value = tag HTML
            ConcurrentDictionary<string, ConcurrentBag<string>> failures = new ConcurrentDictionary<string, ConcurrentBag<string>>();

            // Gather all links
            HtmlParser parser = new HtmlParser();
            await context.Inputs.ParallelForEachAsync(async input => await GatherLinksAsync(input, context, parser, links));

            // Get existing relative output paths
            HashSet<string> relativeOutputPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (validateRelativeLinks)
            {
                foreach (NormalizedPath relativeOutputPath in context.FileSystem
                    .GetOutputDirectory()
                    .GetFiles(System.IO.SearchOption.AllDirectories)
                    .Select(x => context.FileSystem.GetRelativeOutputPath(x.Path)))
                {
                    relativeOutputPaths.Add(relativeOutputPath.FullPath.TrimStart('/'));
                    relativeOutputPaths.Add(context.GetLink(context.GetLink(relativeOutputPath, null, NormalizedPath.Null, false, true, false)).TrimStart('/'));
                    relativeOutputPaths.Add(context.GetLink(context.GetLink(relativeOutputPath, null, NormalizedPath.Null, false, false, true)).TrimStart('/'));
                    relativeOutputPaths.Add(context.GetLink(context.GetLink(relativeOutputPath, null, NormalizedPath.Null, false, true, true)).TrimStart('/'));
                }
            }

            // This policy will limit the number of executing link validations.
            // Limit the amount of concurrent link checks to avoid overwhelming servers.
            Task[] tasks = links.Select(
                async link =>
                {
                    // Attempt to parse the URI
                    if (!Uri.TryCreate(link.Key, UriKind.RelativeOrAbsolute, out Uri uri))
                    {
                        AddOrUpdateFailure(context, link.Value, failures, "Invalid URI");
                        return;
                    }

                    // Adjustment for double-slash link prefix which means use http:// or https:// depending on current protocol
                    // The Uri class treats these as relative, but they're really absolute
                    if (uri.ToString().StartsWith("//") && !Uri.TryCreate($"http:{link.Key}", UriKind.Absolute, out uri))
                    {
                        AddOrUpdateFailure(context, link.Value, failures, "Invalid protocol-relative URI");
                        return;
                    }

                    // Relative
                    if (!uri.IsAbsoluteUri && validateRelativeLinks && !await ValidateRelativeLinkAsync(uri, context, relativeOutputPaths))
                    {
                        AddOrUpdateFailure(context, link.Value, failures, "Unable to validate relative link");
                        return;
                    }

                    // Absolute
                    if (uri.IsAbsoluteUri && validateAbsoluteLinks && !await ValidateAbsoluteLinkAsync(uri, context))
                    {
                        AddOrUpdateFailure(context, link.Value, failures, "Unable to validate absolute link");
                        return;
                    }
                }).ToArray();

            Task.WaitAll(tasks);

            // Report failures
            if (failures.Count > 0)
            {
                int failureCount = failures.Sum(x => x.Value.Count);
                string failureMessage = string.Join(
                    Environment.NewLine,
                    failures.Select(x => $"{x.Key}{Environment.NewLine} - {string.Join(Environment.NewLine + " - ", x.Value)}"));
                context.Log(
                    asError ? LogLevel.Error : LogLevel.Warning,
                    $"{failureCount} link validation failures:{Environment.NewLine}{failureMessage}");
            }

            return context.Inputs;
        }

        // Internal for testing
        internal static async Task GatherLinksAsync(IDocument input, IExecutionContext context, HtmlParser parser, ConcurrentDictionary<string, ConcurrentBag<(IDocument source, string outerHtml)>> links)
        {
            IHtmlDocument htmlDocument = await input.ParseHtmlAsync(context, parser);
            if (htmlDocument is object)
            {
                NormalizedPath basePath = string.IsNullOrEmpty(htmlDocument.BaseUri) ? NormalizedPath.Null : new NormalizedPath(htmlDocument.BaseUri);

                // Links
                foreach (IElement element in htmlDocument.Links)
                {
                    AddOrUpdateLink(element.GetAttribute("href"), element, basePath, input, links);
                }

                // Link element
                foreach (IElement element in htmlDocument.GetElementsByTagName("link").Where(x => x.HasAttribute("href")))
                {
                    AddOrUpdateLink(element.GetAttribute("href"), element, basePath, input, links);
                }

                // Images
                foreach (IHtmlImageElement element in htmlDocument.Images)
                {
                    AddOrUpdateLink(element.GetAttribute("src"), element, basePath, input, links);
                }

                // Scripts
                foreach (IHtmlScriptElement element in htmlDocument.Scripts)
                {
                    AddOrUpdateLink(element.Source, element, basePath, input, links);
                }
            }
        }

        // Internal for testing
        internal static async Task<bool> ValidateRelativeLinkAsync(Uri uri, IExecutionContext context, HashSet<string> relativeOutputPaths)
        {
            if (uri.ToString().Contains("speakers"))
            {
                Debugger.Break();
            }

            // Remove the query string and fragment, if any
            string normalizedPath = uri.ToString();
            if (normalizedPath.Contains("#"))
            {
                normalizedPath = normalizedPath.Remove(normalizedPath.IndexOf("#", StringComparison.Ordinal));
            }
            if (normalizedPath.Contains("?"))
            {
                normalizedPath = normalizedPath.Remove(normalizedPath.IndexOf("?", StringComparison.Ordinal));
            }
            normalizedPath = Uri.UnescapeDataString(normalizedPath);
            if (normalizedPath?.Length == 0)
            {
                return true;
            }

            // Remove the link root if there is one and remove the preceding slash
            if (!context.Settings.GetPath(Keys.LinkRoot).IsNull
                && normalizedPath.StartsWith(context.Settings.GetPath(Keys.LinkRoot).FullPath))
            {
                normalizedPath = normalizedPath.Substring(context.Settings.GetPath(Keys.LinkRoot).FullPath.Length);
            }
            normalizedPath = normalizedPath.TrimStart('/');

            // See if it's in the output paths
            if (relativeOutputPaths.Contains(normalizedPath))
            {
                context.LogDebug($"Validated relative link {uri}");
                return true;
            }

            // Check the absolute URL just in case the user is using some sort of CNAME or something.
            if (Uri.TryCreate(context.GetLink() + uri, UriKind.Absolute, out Uri absoluteCheckUri))
            {
                return await ValidateAbsoluteLinkAsync(absoluteCheckUri, context);
            }

            return false;
        }

        // Internal for testing
        internal static async Task<bool> ValidateAbsoluteLinkAsync(Uri uri, IExecutionContext context)
        {
            // If this is a mailto: link, it's sufficient just that it passed the Uri validation.
            if (uri.Scheme == Uri.UriSchemeMailto)
            {
                return true;
            }

            // Perform request as HEAD
            bool result = await ValidateAbsoluteLinkAsync(uri, HttpMethod.Head, context);

            if (result)
            {
                return true;
            }

            // Try one more time as GET
            return await ValidateAbsoluteLinkAsync(uri, HttpMethod.Get, context);
        }

        private static async Task<bool> ValidateAbsoluteLinkAsync(Uri uri, HttpMethod method, IExecutionContext context)
        {
            try
            {
                HttpResponseMessage response = await context.SendHttpRequestWithRetryAsync(() => new HttpRequestMessage(method, uri));

                // Even with exponential backoff we have TooManyRequests, just skip, since we have to assume it's valid.
                if (response.StatusCode == TooManyRequests)
                {
                    context.LogDebug($"Skipping absolute link {uri}: too many requests have been issued so can't reliably test.");
                    return true;
                }

                // We don't use IsSuccessStatusCode, we consider in this case 300's valid.
                if (response.StatusCode >= HttpStatusCode.BadRequest)
                {
                    context.LogDebug($"Validation failure for absolute link {method} {uri}: returned status code {(int)response.StatusCode} {response.StatusCode}");
                    return false;
                }

                // We don't bother disposing of the response in this case. Due to advice from here: https://stackoverflow.com/questions/15705092/do-httpclient-and-httpclienthandler-have-to-be-disposed
                context.LogDebug($"Validation success for absolute link {method} {uri}: returned status code {(int)response.StatusCode} {response.StatusCode}");
                return true;
            }
            catch (TaskCanceledException ex)
            {
                context.LogDebug($"Skipping absolute link {method} {uri} due to timeout: {ex}.");
                return true;
            }
            catch (ArgumentException ex)
            {
                context.LogDebug($"Skipping absolute link {method} {uri} due to invalid uri: {ex}.");
                return true;
            }
            catch (Exception ex)
            {
                context.LogDebug($"Skipping absolute link {method} {uri} due to unknown error: {ex}.");
                return true;
            }
        }

        private static void AddOrUpdateLink(string link, IElement element, NormalizedPath basePath, IDocument source, ConcurrentDictionary<string, ConcurrentBag<(IDocument source, string outerHtml)>> links)
        {
            if (string.IsNullOrEmpty(link))
            {
                return;
            }

            // Attempt to parse the URI
            if (Uri.TryCreate(link, UriKind.RelativeOrAbsolute, out Uri uri))
            {
                // If this is a relative Uri, adjust it relative to the source document.
                // Adjustment for double-slash link prefix which means use http:// or https:// depending on current protocol
                // The Uri class treats these as relative, but they're really absolute
                if (!uri.IsAbsoluteUri && !uri.ToString().StartsWith("//") && !source.Destination.IsNull)
                {
                    link = basePath.IsNull
                        ? source.Destination.Parent.Combine(new NormalizedPath(uri.ToString())).FullPath
                        : basePath.Combine(new NormalizedPath(uri.ToString())).FullPath;
                }
            }

            links.AddOrUpdate(
                link,
                _ => new ConcurrentBag<(IDocument, string)> { (source, ((IElement)element.Clone(false)).OuterHtml) },
                (_, list) =>
                {
                    list.Add((source, ((IElement)element.Clone(false)).OuterHtml));
                    return list;
                });
        }

        private static void AddOrUpdateFailure(
            IExecutionContext context,
            ConcurrentBag<(IDocument source, string outerHtml)> links,
            ConcurrentDictionary<string, ConcurrentBag<string>> failures,
            string message)
        {
            foreach ((IDocument source, string outerHtml) in links)
            {
                if (source.Source.IsNull)
                {
                    context.LogWarning($"Validation failure for link: unknown file for {outerHtml}.");
                    continue;
                }

                failures.AddOrUpdate(
                    source.Source.FullPath,
                    _ => new ConcurrentBag<string> { $"{outerHtml} ({message})" },
                    (_, list) =>
                    {
                        list.Add($"{outerHtml} ({message})");
                        return list;
                    });
            }
        }
    }
}
