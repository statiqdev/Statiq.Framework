using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Razor.Compilation;
using Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Statiq.Common;

namespace Statiq.Razor
{
    public static class IHtmlHelperExtensions
    {
        /// <summary>
        /// Gets an anchor HTML element for the specified document using the document destination.
        /// Note that you can optionally include the host or not depending
        /// on if you want to generate host-specific links. By default, the host is not included so that
        /// sites work the same on any server including the preview server.
        /// </summary>
        /// <param name="htmlHelper">The HTML helper.</param>
        /// <param name="document">The document to generate an anchor element for.</param>
        /// <returns>
        /// A string representation of the path suitable for a web link.
        /// </returns>
        public static IHtmlContent DocumentLink(this IHtmlHelper htmlHelper, IDocument document) => htmlHelper.DocumentLink(document, false);

        /// <summary>
        /// Gets an anchor HTML element for the specified document using the document destination.
        /// Note that you can optionally include the host or not depending
        /// on if you want to generate host-specific links. By default, the host is not included so that
        /// sites work the same on any server including the preview server.
        /// </summary>
        /// <param name="htmlHelper">The HTML helper.</param>
        /// <param name="document">The document to generate an anchor element for.</param>
        /// <param name="htmlAttributes">HTML attributes to add to the link.</param>
        /// <returns>
        /// A string representation of the path suitable for a web link.
        /// </returns>
        public static IHtmlContent DocumentLink(this IHtmlHelper htmlHelper, IDocument document, IDictionary<object, object> htmlAttributes) =>
            htmlHelper.DocumentLink(document, false, htmlAttributes);

        /// <summary>
        /// Gets an anchor HTML element for the specified document using the document destination.
        /// Note that you can optionally include the host or not depending
        /// on if you want to generate host-specific links. By default, the host is not included so that
        /// sites work the same on any server including the preview server.
        /// </summary>
        /// <param name="htmlHelper">The HTML helper.</param>
        /// <param name="document">The document to generate an anchor element for.</param>
        /// <param name="includeHost">
        /// If set to <c>true</c> the host configured in the output settings will
        /// be included in the link, otherwise the host will be omitted and only the root path will be included (default).
        /// </param>
        /// <returns>
        /// A string representation of the path suitable for a web link.
        /// </returns>
        public static IHtmlContent DocumentLink(this IHtmlHelper htmlHelper, IDocument document, bool includeHost)
        {
            document.ThrowIfNull(nameof(document));
            return htmlHelper.DocumentLink(document, document.GetTitle(), includeHost);
        }

        /// <summary>
        /// Gets an anchor HTML element for the specified document using the document destination.
        /// Note that you can optionally include the host or not depending
        /// on if you want to generate host-specific links. By default, the host is not included so that
        /// sites work the same on any server including the preview server.
        /// </summary>
        /// <param name="htmlHelper">The HTML helper.</param>
        /// <param name="document">The document to generate an anchor element for.</param>
        /// <param name="includeHost">
        /// If set to <c>true</c> the host configured in the output settings will
        /// be included in the link, otherwise the host will be omitted and only the root path will be included (default).
        /// </param>
        /// <param name="htmlAttributes">HTML attributes to add to the link.</param>
        /// <returns>
        /// A string representation of the path suitable for a web link.
        /// </returns>
        public static IHtmlContent DocumentLink(
            this IHtmlHelper htmlHelper,
            IDocument document,
            bool includeHost,
            IDictionary<object, object> htmlAttributes)
        {
            document.ThrowIfNull(nameof(document));
            return htmlHelper.DocumentLink(document, document.GetTitle(), includeHost, htmlAttributes);
        }

        /// <summary>
        /// Gets an anchor HTML element for the specified document using the document destination.
        /// Note that you can optionally include the host or not depending
        /// on if you want to generate host-specific links. By default, the host is not included so that
        /// sites work the same on any server including the preview server.
        /// </summary>
        /// <param name="htmlHelper">The HTML helper.</param>
        /// <param name="document">The document to generate an anchor element for.</param>
        /// <param name="linkText">The title to use for the anchor, or null to use the document title.</param>
        /// <returns>
        /// A string representation of the path suitable for a web link.
        /// </returns>
        public static IHtmlContent DocumentLink(this IHtmlHelper htmlHelper, IDocument document, string linkText) =>
            htmlHelper.DocumentLink(document, linkText, false);

        /// <summary>
        /// Gets an anchor HTML element for the specified document using the document destination.
        /// Note that you can optionally include the host or not depending
        /// on if you want to generate host-specific links. By default, the host is not included so that
        /// sites work the same on any server including the preview server.
        /// </summary>
        /// <param name="htmlHelper">The HTML helper.</param>
        /// <param name="document">The document to generate an anchor element for.</param>
        /// <param name="queryAndFragment">
        /// Appends a query and/or fragment to the document path. If a value is provided for this parameter
        /// and it does not start with "?" or "#" then it will be assumed a query and a "?" will be prefixed.
        /// </param>
        /// <param name="linkText">The title to use for the anchor, or null to use the document title.</param>
        /// <returns>
        /// A string representation of the path suitable for a web link.
        /// </returns>
        public static IHtmlContent DocumentLink(
            this IHtmlHelper htmlHelper,
            IDocument document,
            string queryAndFragment,
            string linkText) =>
            htmlHelper.DocumentLink(document, queryAndFragment, linkText, false);

        /// <summary>
        /// Gets an anchor HTML element for the specified document using the document destination.
        /// Note that you can optionally include the host or not depending
        /// on if you want to generate host-specific links. By default, the host is not included so that
        /// sites work the same on any server including the preview server.
        /// </summary>
        /// <param name="htmlHelper">The HTML helper.</param>
        /// <param name="document">The document to generate an anchor element for.</param>
        /// <param name="linkText">The title to use for the anchor, or null to use the document title.</param>
        /// <param name="htmlAttributes">HTML attributes to add to the link.</param>
        /// <returns>
        /// A string representation of the path suitable for a web link.
        /// </returns>
        public static IHtmlContent DocumentLink(
            this IHtmlHelper htmlHelper,
            IDocument document,
            string linkText,
            IDictionary<object, object> htmlAttributes) =>
            htmlHelper.DocumentLink(document, linkText, false, htmlAttributes);

        /// <summary>
        /// Gets an anchor HTML element for the specified document using the document destination.
        /// Note that you can optionally include the host or not depending
        /// on if you want to generate host-specific links. By default, the host is not included so that
        /// sites work the same on any server including the preview server.
        /// </summary>
        /// <param name="htmlHelper">The HTML helper.</param>
        /// <param name="document">The document to generate an anchor element for.</param>
        /// <param name="queryAndFragment">
        /// Appends a query and/or fragment to the document path. If a value is provided for this parameter
        /// and it does not start with "?" or "#" then it will be assumed a query and a "?" will be prefixed.
        /// </param>
        /// <param name="linkText">The title to use for the anchor, or null to use the document title.</param>
        /// <param name="htmlAttributes">HTML attributes to add to the link.</param>
        /// <returns>
        /// A string representation of the path suitable for a web link.
        /// </returns>
        public static IHtmlContent DocumentLink(
            this IHtmlHelper htmlHelper,
            IDocument document,
            string queryAndFragment,
            string linkText,
            IDictionary<object, object> htmlAttributes) =>
            htmlHelper.DocumentLink(document, queryAndFragment, linkText, false, htmlAttributes);

        /// <summary>
        /// Gets an anchor HTML element for the specified document using the document destination.
        /// Note that you can optionally include the host or not depending
        /// on if you want to generate host-specific links. By default, the host is not included so that
        /// sites work the same on any server including the preview server.
        /// </summary>
        /// <param name="htmlHelper">The HTML helper.</param>
        /// <param name="document">The document to generate an anchor element for.</param>
        /// <param name="linkText">The title to use for the anchor, or null to use the document title.</param>
        /// <param name="includeHost">
        /// If set to <c>true</c> the host configured in the output settings will
        /// be included in the link, otherwise the host will be omitted and only the root path will be included (default).
        /// </param>
        /// <returns>
        /// A string representation of the path suitable for a web link.
        /// </returns>
        public static IHtmlContent DocumentLink(
            this IHtmlHelper htmlHelper,
            IDocument document,
            string linkText,
            bool includeHost) =>
            htmlHelper.DocumentLink(document, linkText, includeHost, null);

        /// <summary>
        /// Gets an anchor HTML element for the specified document using the document destination.
        /// Note that you can optionally include the host or not depending
        /// on if you want to generate host-specific links. By default, the host is not included so that
        /// sites work the same on any server including the preview server.
        /// </summary>
        /// <param name="htmlHelper">The HTML helper.</param>
        /// <param name="document">The document to generate an anchor element for.</param>
        /// <param name="queryAndFragment">
        /// Appends a query and/or fragment to the document path. If a value is provided for this parameter
        /// and it does not start with "?" or "#" then it will be assumed a query and a "?" will be prefixed.
        /// </param>
        /// <param name="linkText">The title to use for the anchor, or null to use the document title.</param>
        /// <param name="includeHost">
        /// If set to <c>true</c> the host configured in the output settings will
        /// be included in the link, otherwise the host will be omitted and only the root path will be included (default).
        /// </param>
        /// <returns>
        /// A string representation of the path suitable for a web link.
        /// </returns>
        public static IHtmlContent DocumentLink(
            this IHtmlHelper htmlHelper,
            IDocument document,
            string queryAndFragment,
            string linkText,
            bool includeHost) =>
            htmlHelper.DocumentLink(document, queryAndFragment, linkText, includeHost, null);

        /// <summary>
        /// Gets an anchor HTML element for the specified document using the document destination.
        /// Note that you can optionally include the host or not depending
        /// on if you want to generate host-specific links. By default, the host is not included so that
        /// sites work the same on any server including the preview server.
        /// </summary>
        /// <param name="htmlHelper">The HTML helper.</param>
        /// <param name="document">The document to generate an anchor element for.</param>
        /// <param name="linkText">The title to use for the anchor, or null to use the document title.</param>
        /// <param name="includeHost">
        /// If set to <c>true</c> the host configured in the output settings will
        /// be included in the link, otherwise the host will be omitted and only the root path will be included (default).
        /// </param>
        /// <param name="htmlAttributes">HTML attributes to add to the link.</param>
        /// <returns>
        /// A string representation of the path suitable for a web link.
        /// </returns>
        public static IHtmlContent DocumentLink(
            this IHtmlHelper htmlHelper,
            IDocument document,
            string linkText,
            bool includeHost,
            IDictionary<object, object> htmlAttributes) =>
            htmlHelper.DocumentLink(document, null, linkText, includeHost, htmlAttributes);

        /// <summary>
        /// Gets an anchor HTML element for the specified document using the document destination.
        /// Note that you can optionally include the host or not depending
        /// on if you want to generate host-specific links. By default, the host is not included so that
        /// sites work the same on any server including the preview server.
        /// </summary>
        /// <param name="htmlHelper">The HTML helper.</param>
        /// <param name="document">The document to generate an anchor element for.</param>
        /// <param name="queryAndFragment">
        /// Appends a query and/or fragment to the document path. If a value is provided for this parameter
        /// and it does not start with "?" or "#" then it will be assumed a query and a "?" will be prefixed.
        /// </param>
        /// <param name="linkText">The title to use for the anchor, or null to use the document title.</param>
        /// <param name="includeHost">
        /// If set to <c>true</c> the host configured in the output settings will
        /// be included in the link, otherwise the host will be omitted and only the root path will be included (default).
        /// </param>
        /// <param name="htmlAttributes">HTML attributes to add to the link.</param>
        /// <returns>
        /// A string representation of the path suitable for a web link.
        /// </returns>
        public static IHtmlContent DocumentLink(
            this IHtmlHelper htmlHelper,
            IDocument document,
            string queryAndFragment,
            string linkText,
            bool includeHost,
            IDictionary<object, object> htmlAttributes)
        {
            htmlHelper.ThrowIfNull(nameof(htmlHelper));
            document.ThrowIfNull(nameof(document));

            TagBuilder tagBuilder = new TagBuilder("a");
            tagBuilder.InnerHtml.SetContent(linkText ?? document.GetTitle());
            if (htmlAttributes is object)
            {
                tagBuilder.MergeAttributes(htmlAttributes);
            }
            tagBuilder.MergeAttribute("href", document.GetLink(queryAndFragment, includeHost));
            return tagBuilder;
        }

        public static void RenderCachedPartial(
            this IHtmlHelper htmlHelper,
            string partialViewName) =>
            RenderCachedPartialAsync(htmlHelper, partialViewName).GetAwaiter().GetResult();

        public static void RenderCachedPartial(
            this IHtmlHelper htmlHelper,
            string partialViewName,
            object model) =>
            RenderCachedPartialAsync(htmlHelper, partialViewName, model).GetAwaiter().GetResult();

        public static void RenderCachedPartial(
            this IHtmlHelper htmlHelper,
            string partialViewName,
            object model,
            object cacheKey) =>
            RenderCachedPartialAsync(htmlHelper, partialViewName, model, cacheKey).GetAwaiter().GetResult();

        public static IHtmlContent CachedPartial(
            this IHtmlHelper htmlHelper,
            string partialViewName) =>
            CachedPartialAsync(htmlHelper, partialViewName).GetAwaiter().GetResult();

        public static IHtmlContent CachedPartial(
            this IHtmlHelper htmlHelper,
            string partialViewName,
            object model) =>
            CachedPartialAsync(htmlHelper, partialViewName, model).GetAwaiter().GetResult();

        public static IHtmlContent CachedPartial(
            this IHtmlHelper htmlHelper,
            string partialViewName,
            object model,
            object cacheKey) =>
            CachedPartialAsync(htmlHelper, partialViewName, model, cacheKey).GetAwaiter().GetResult();

        public static async Task RenderCachedPartialAsync(
            this IHtmlHelper htmlHelper,
            string partialViewName) =>
            await RenderCachedPartialAsync(htmlHelper, partialViewName, null, null);

        public static async Task RenderCachedPartialAsync(
            this IHtmlHelper htmlHelper,
            string partialViewName,
            object model) =>
            await RenderCachedPartialAsync(htmlHelper, partialViewName, model, null);

        public static async Task RenderCachedPartialAsync(
            this IHtmlHelper htmlHelper,
            string partialViewName,
            object model,
            object cacheKey)
        {
            LockingStreamWrapper contentStreamWrapper =
                await GetCachedPartialMemoryStreamAsync(htmlHelper, partialViewName, model, cacheKey);
            using (Stream contentStream = contentStreamWrapper.GetStream())
            {
                await contentStream.CopyToAsync(htmlHelper.ViewContext.Writer);
                await htmlHelper.ViewContext.Writer.FlushAsync();
            }
        }

        public static async Task<IHtmlContent> CachedPartialAsync(
            this IHtmlHelper htmlHelper,
            string partialViewName) =>
            await CachedPartialAsync(htmlHelper, partialViewName, null, null);

        public static async Task<IHtmlContent> CachedPartialAsync(
            this IHtmlHelper htmlHelper,
            string partialViewName,
            object model) =>
            await CachedPartialAsync(htmlHelper, partialViewName, model, model);

        public static async Task<IHtmlContent> CachedPartialAsync(
            this IHtmlHelper htmlHelper,
            string partialViewName,
            object model,
            object cacheKey)
        {
            LockingStreamWrapper contentStreamWrapper =
                await GetCachedPartialMemoryStreamAsync(htmlHelper, partialViewName, model, cacheKey);
            return new HelperResult(async writer =>
            {
                using (Stream contentStream = contentStreamWrapper.GetStream())
                {
                    await contentStream.CopyToAsync(writer);
                    await writer.FlushAsync();
                }
            });
        }

        private static readonly ConcurrentCache<(string, object), Task<LockingStreamWrapper>> _cachedPartialContent
            = new ConcurrentCache<(string, object), Task<LockingStreamWrapper>>(true, true);

        private static async Task<LockingStreamWrapper> GetCachedPartialMemoryStreamAsync(
            this IHtmlHelper htmlHelper,
            string partialViewName,
            object model,
            object cacheKey)
        {
            htmlHelper.ThrowIfNull(nameof(htmlHelper));

            IExecutionContext context = (IExecutionContext)htmlHelper.ViewContext.ViewData[ViewDataKeys.StatiqExecutionContext];
            IServiceProvider serviceProvider = (IServiceProvider)htmlHelper.ViewContext.ViewData[ViewDataKeys.StatiqServiceProvider];
            ICompositeViewEngine viewEngine = serviceProvider.GetRequiredService<ICompositeViewEngine>();
            HtmlEncoder htmlEncoder = serviceProvider.GetRequiredService<HtmlEncoder>();

            // Get the normalized path so that we can match up the partial regardless of where it's called from or the name
            // Copied from HtmlHelper.RenderPartialCoreAsync()
            ViewEngineResult viewEngineResult = viewEngine.GetView(
                htmlHelper.ViewContext.ExecutingFilePath,
                partialViewName,
                isMainPage: false);
            if (!viewEngineResult.Success)
            {
                viewEngineResult = viewEngine.FindView(htmlHelper.ViewContext, partialViewName, isMainPage: false);
            }

            // If we can't find it this way, go ahead and try again normally and that'll throw the error
            if (!viewEngineResult.Success)
            {
                throw new Exception($"Could not find partial to cache with name {partialViewName}");
            }

            // Cache the partial results using the path name by writing to a memory stream
            return await _cachedPartialContent.GetOrAdd(
                (viewEngineResult.View.Path, cacheKey),
                async (key, args) =>
                {
                    MemoryStream contentStream = args.context.MemoryStreamFactory.GetStream();
                    IHtmlContent content = args.model is object
                        ? await args.htmlHelper.PartialAsync(args.partialViewName, args.model)
                        : await args.htmlHelper.PartialAsync(args.partialViewName);
                    using (TextWriter writer = contentStream.GetWriter())
                    {
                        content.WriteTo(writer, args.htmlEncoder);
                        await writer.FlushAsync();
                    }
                    return new LockingStreamWrapper(contentStream, true);
                },
                (partialViewName, htmlHelper, model, context, htmlEncoder));
        }
    }
}