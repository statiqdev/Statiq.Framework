using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using AngleSharp.Dom;
using AngleSharp.Dom.Html;
using AngleSharp.Parser.Html;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
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
        private const int MaxAbsoluteLinkRetry = 5;
        private const HttpStatusCode TooManyRequests = (HttpStatusCode)429;

        private bool _validateAbsoluteLinks;
        private bool _validateRelativeLinks = true;
        private bool _asError;

        /// <summary>
        /// Validates absolute (often external) links. This may add a significant delay to your
        /// generation process so it's recommended absolute links are only checked periodically.
        /// The default behavior is not to check absolute links. Also note that false positive
        /// failures are common when validating external links so any links that fail the check
        /// should be subsequently checked manually.
        /// </summary>
        /// <param name="validateAbsoluteLinks"><c>true</c> to validate absolute links.</param>
        /// <returns>The current module instance.</returns>
        public ValidateLinks ValidateAbsoluteLinks(bool validateAbsoluteLinks = true)
        {
            _validateAbsoluteLinks = validateAbsoluteLinks;
            return this;
        }

        /// <summary>
        /// Validates relative links, which is activated by default.
        /// </summary>
        /// <param name="validateRelativeLinks"><c>true</c> to validate relative links.</param>
        /// <returns>The current module instance.</returns>
        public ValidateLinks ValidateRelativeLinks(bool validateRelativeLinks = true)
        {
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
        public ValidateLinks AsError(bool asError = true)
        {
            _asError = asError;
            return this;
        }

        /// <inheritdoc />
        public override async Task<IEnumerable<IDocument>> ExecuteAsync(IExecutionContext context)
        {
#pragma warning disable RCS1163 // Unused parameter.
            // Handle invalid HTTPS certificates and allow alternate security protocols (see http://stackoverflow.com/a/5670954/807064)
            ServicePointManager.ServerCertificateValidationCallback = (s, cert, chain, ssl) => true;
#pragma warning restore RCS1163 // Unused parameter.

            // Key = link, Value = source, tag HTML
            ConcurrentDictionary<string, ConcurrentBag<(string documentSource, string outerHtml)>> links =
                new ConcurrentDictionary<string, ConcurrentBag<(string documentSource, string outerHtml)>>();

            // Key = source, Value = tag HTML
            ConcurrentDictionary<string, ConcurrentBag<string>> failures = new ConcurrentDictionary<string, ConcurrentBag<string>>();

            // Gather all links
            HtmlParser parser = new HtmlParser();
            await context.Inputs.ParallelForEachAsync(async input => await GatherLinksAsync(input, context, parser, links));

            // This policy will limit the number of executing link validations.
            // Limit the amount of concurrent link checks to avoid overwhelming servers.
            Task[] tasks = links.Select(
                async link =>
                {
                    // Attempt to parse the URI
                    if (!Uri.TryCreate(link.Key, UriKind.RelativeOrAbsolute, out Uri uri))
                    {
                        AddOrUpdateFailure(context, link.Value, failures);
                    }

                    // Adjustment for double-slash link prefix which means use http:// or https:// depending on current protocol
                    // The Uri class treats these as relative, but they're really absolute
                    if (uri.ToString().StartsWith("//") && !Uri.TryCreate($"http:{link.Key}", UriKind.Absolute, out uri))
                    {
                        AddOrUpdateFailure(context, link.Value, failures);
                    }

                    // Relative
                    if (!uri.IsAbsoluteUri && _validateRelativeLinks && !await ValidateRelativeLinkAsync(uri, context))
                    {
                        AddOrUpdateFailure(context, link.Value, failures);
                    }

                    // Absolute
                    if (uri.IsAbsoluteUri && _validateAbsoluteLinks && !await ValidateAbsoluteLinkAsync(uri, context))
                    {
                        AddOrUpdateFailure(context, link.Value, failures);
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
                context.Logger.Log(
                    _asError ? LogLevel.Error : LogLevel.Warning,
                    $"{failureCount} link validation failures:{Environment.NewLine}{failureMessage}");
            }

            return context.Inputs;
        }

        // Internal for testing
        internal static async Task GatherLinksAsync(IDocument input, IExecutionContext context, HtmlParser parser, ConcurrentDictionary<string, ConcurrentBag<(string source, string outerHtml)>> links)
        {
            IHtmlDocument htmlDocument = await input.ParseHtmlAsync(context, parser);
            if (htmlDocument != null)
            {
                // Links
                foreach (IElement element in htmlDocument.Links)
                {
                    AddOrUpdateLink(element.GetAttribute("href"), element, input.Source?.FullPath, links);
                }

                // Link element
                foreach (IElement element in htmlDocument.GetElementsByTagName("link").Where(x => x.HasAttribute("href")))
                {
                    AddOrUpdateLink(element.GetAttribute("href"), element, input.Source?.FullPath, links);
                }

                // Images
                foreach (IHtmlImageElement element in htmlDocument.Images)
                {
                    AddOrUpdateLink(element.GetAttribute("src"), element, input.Source?.FullPath, links);
                }

                // Scripts
                foreach (IHtmlScriptElement element in htmlDocument.Scripts)
                {
                    AddOrUpdateLink(element.Source, element, input.Source?.FullPath, links);
                }
            }
        }

        // Internal for testing
        internal static async Task<bool> ValidateRelativeLinkAsync(Uri uri, IExecutionContext context)
        {
            List<FilePath> checkPaths = new List<FilePath>();

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
            if (context.Settings.DirectoryPath(Keys.LinkRoot) != null
                && normalizedPath.StartsWith(context.Settings.DirectoryPath(Keys.LinkRoot).FullPath))
            {
                normalizedPath = normalizedPath.Substring(context.Settings.DirectoryPath(Keys.LinkRoot).FullPath.Length);
            }
            if (normalizedPath.StartsWith("/"))
            {
                normalizedPath = normalizedPath.Length > 1 ? normalizedPath.Substring(1) : string.Empty;
            }

            // Add the base path
            if (normalizedPath != string.Empty)
            {
                checkPaths.Add(new FilePath(normalizedPath));
            }

            // Add filenames
            checkPaths.AddRange(LinkGenerator.DefaultHidePages.Select(x => new FilePath(normalizedPath?.Length == 0 ? x : $"{normalizedPath}/{x}")));

            // Add extensions
            checkPaths.AddRange(LinkGenerator.DefaultHideExtensions.SelectMany(x => checkPaths.Select(y => y.AppendExtension(x))).ToArray());

            // Check all the candidate paths
            FilePath validatedPath = checkPaths.Find(x =>
            {
                IFile outputFile;
                try
                {
                    outputFile = context.FileSystem.GetOutputFile(x);
                }
                catch (Exception ex)
                {
                    context.Logger.LogDebug($"Could not validate path {x.FullPath} for relative link {uri}: {ex.Message}");
                    return false;
                }

                return outputFile.Exists;
            });

            if (validatedPath != null)
            {
                context.Logger.LogDebug($"Validated relative link {uri} at {validatedPath.FullPath}");
                return true;
            }

            // Check the absolute URL just in case the user is using some sort of CNAME or something.
            if (Uri.TryCreate(context.GetLink() + uri, UriKind.Absolute, out Uri absoluteCheckUri))
            {
                return await ValidateAbsoluteLinkAsync(absoluteCheckUri, context);
            }

            context.Logger.LogDebug($"Validation failure for relative link {uri}: could not find output file at any of {string.Join(", ", checkPaths.Select(x => x.FullPath))}");
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
            // Retry with exponential backoff links. This helps with websites like GitHub that will give us a 429 -- TooManyRequests.
            AsyncRetryPolicy<HttpResponseMessage> retryPolicy = Policy
                .Handle<HttpRequestException>()
                .OrResult<HttpResponseMessage>(r => r.StatusCode == TooManyRequests)
                .WaitAndRetryAsync(MaxAbsoluteLinkRetry, attempt => TimeSpan.FromSeconds(0.1 * Math.Pow(2, attempt)));

            try
            {
                HttpResponseMessage response = await retryPolicy.ExecuteAsync(async () =>
                {
                    using (HttpClient httpClient = context.CreateHttpClient())
                    {
                        return await httpClient.SendAsync(new HttpRequestMessage(method, uri));
                    }
                });

                // Even with exponential backoff we have TooManyRequests, just skip, since we have to assume it's valid.
                if (response.StatusCode == TooManyRequests)
                {
                    context.Logger.LogDebug($"Skipping absolute link {uri}: too many requests have been issued so can't reliably test.");
                    return true;
                }

                // We don't use IsSuccessStatusCode, we consider in this case 300's valid.
                if (response.StatusCode >= HttpStatusCode.BadRequest)
                {
                    context.Logger.LogDebug($"Validation failure for absolute link {method} {uri}: returned status code {(int)response.StatusCode} {response.StatusCode}");
                    return false;
                }

                // We don't bother disposing of the response in this case. Due to advice from here: https://stackoverflow.com/questions/15705092/do-httpclient-and-httpclienthandler-have-to-be-disposed
                context.Logger.LogDebug($"Validation success for absolute link {method} {uri}: returned status code {(int)response.StatusCode} {response.StatusCode}");
                return true;
            }
            catch (TaskCanceledException ex)
            {
                context.Logger.LogDebug($"Skipping absolute link {method} {uri} due to timeout: {ex}.");
                return true;
            }
            catch (ArgumentException ex)
            {
                context.Logger.LogDebug($"Skipping absolute link {method} {uri} due to invalid uri: {ex}.");
                return true;
            }
            catch (Exception ex)
            {
                context.Logger.LogDebug($"Skipping absolute link {method} {uri} due to unknown error: {ex}.");
                return true;
            }
        }

        private static void AddOrUpdateLink(string link, IElement element, string source, ConcurrentDictionary<string, ConcurrentBag<(string source, string outerHtml)>> links)
        {
            if (string.IsNullOrEmpty(link))
            {
                return;
            }

            links.AddOrUpdate(
                link,
                _ => new ConcurrentBag<(string, string)> { (source, ((IElement)element.Clone(false)).OuterHtml) },
                (_, list) =>
                {
                    list.Add((source, ((IElement)element.Clone(false)).OuterHtml));
                    return list;
                });
        }

        private static void AddOrUpdateFailure(IExecutionContext context, ConcurrentBag<(string source, string outerHtml)> links, ConcurrentDictionary<string, ConcurrentBag<string>> failures)
        {
            foreach ((string source, string outerHtml) in links)
            {
                if (source == null)
                {
                    context.Logger.LogWarning($"Validation failure for link: unknown file for {outerHtml}.");
                    continue;
                }

                failures.AddOrUpdate(
                    source,
                    _ => new ConcurrentBag<string> { outerHtml },
                    (_, list) =>
                    {
                        list.Add(outerHtml);
                        return list;
                    });
            }
        }
    }
}
