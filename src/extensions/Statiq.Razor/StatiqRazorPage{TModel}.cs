using System;
using System.Collections.Immutable;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Razor;
using Statiq.Common;

namespace Statiq.Razor
{
    public abstract class StatiqRazorPage<TModel> : RazorPage<TModel>
    {
        public IDocument Document => ViewData[ViewDataKeys.StatiqDocument] as IDocument;
        public IMetadata Metadata => Document;

        public IExecutionContext ExecutionContext => ViewData[ViewDataKeys.StatiqExecutionContext] as IExecutionContext;
        public new IExecutionContext Context => ExecutionContext;
        public ImmutableArray<IDocument> Inputs => ExecutionContext.Inputs;
        public IPipelineOutputs Outputs => ExecutionContext.Outputs;

        public HttpContext HttpContext => base.Context;

        /// <summary>
        /// Renders the content of the specified section if it's defined.
        /// If the section is not defined, renders the provided content.
        /// </summary>
        /// <param name="name">The name of the section to render.</param>
        /// <param name="defaultContents">The default content to render if the section is not defined.</param>
        /// <returns>The HTML content.</returns>
        public IHtmlContent RenderSection(string name, Func<dynamic, HelperResult> defaultContents)
        {
            if (IsSectionDefined(name))
            {
                return RenderSection(name);
            }
            return defaultContents(null);
        }
    }
}