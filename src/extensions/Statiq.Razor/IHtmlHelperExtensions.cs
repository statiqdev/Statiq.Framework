using System;
using System.Collections.Generic;
using System.Net;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
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
    }
}