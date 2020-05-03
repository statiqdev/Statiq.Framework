using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;
using AngleSharp.Dom;
using AngleSharp.Dom.Html;
using AngleSharp.Parser.Html;
using Microsoft.Extensions.Logging;
using Statiq.Common;

namespace Statiq.Html
{
    /// <summary>
    /// Converts all relative links to absolute using the "Host" and other settings values.
    /// </summary>
    /// <remarks>
    /// This module is particularly useful when presenting content for external consumption such
    /// as with the <c>GenerateFeeds</c> module or for use in an API.
    /// </remarks>
    /// <category>Content</category>
    public class AbsolutizeLinks : ParallelModule
    {
        protected override Task<IEnumerable<Common.IDocument>> ExecuteInputAsync(Common.IDocument input, IExecutionContext context) =>
            ProcessHtml.ProcessElementsAsync(
                input,
                context,
                "a",
                false,
                (d, c, e, m) =>
                {
                    string href = e.GetAttribute("href");
                    if (!string.IsNullOrEmpty(href)
                        && Uri.TryCreate(href, UriKind.RelativeOrAbsolute, out Uri uri)
                        && !uri.IsAbsoluteUri)
                    {
                        e.SetAttribute("href", context.GetLink(href, true));
                    }
                });
    }
}
