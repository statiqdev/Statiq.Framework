using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Statiq.Common;

namespace Statiq.Common
{
    public static class IDocumentGetAnchorExtensions
    {
        /// <summary>
        /// Gets an anchor HTML element for the specified document using the document destination.
        /// Note that you can optionally include the host or not depending
        /// on if you want to generate host-specific links. By default, the host is not included so that
        /// sites work the same on any server including the preview server.
        /// </summary>
        /// <param name="document">The document to generate an anchor element for.</param>
        /// <param name="includeHost">
        /// If set to <c>true</c> the host configured in the output settings will
        /// be included in the link, otherwise the host will be omitted and only the root path will be included (default).
        /// </param>
        /// <returns>
        /// A string representation of the path suitable for a web link.
        /// </returns>
        public static string GetAnchor(this IDocument document, bool includeHost = false)
        {
            _ = document ?? throw new ArgumentNullException(nameof(document));
            return document.GetAnchor(document.GetTitle(), includeHost);
        }

        /// <summary>
        /// Gets an anchor HTML element for the specified document using the document destination.
        /// Note that you can optionally include the host or not depending
        /// on if you want to generate host-specific links. By default, the host is not included so that
        /// sites work the same on any server including the preview server.
        /// </summary>
        /// <param name="document">The document to generate an anchor element for.</param>
        /// <param name="title">The title to use for the anchor.</param>
        /// <param name="includeHost">
        /// If set to <c>true</c> the host configured in the output settings will
        /// be included in the link, otherwise the host will be omitted and only the root path will be included (default).
        /// </param>
        /// <returns>
        /// A string representation of the path suitable for a web link.
        /// </returns>
        public static string GetAnchor(this IDocument document, string title, bool includeHost = false)
        {
            _ = document ?? throw new ArgumentNullException(nameof(document));
            string link = IExecutionContext.Current.GetLink(document, includeHost);
            return $"<a href=\"{link}\">{title}</a>";
        }
    }
}
