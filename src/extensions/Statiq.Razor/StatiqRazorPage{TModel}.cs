using System.Collections.Immutable;
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

        public Trace Trace => Trace.Current;
    }
}