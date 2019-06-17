using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Razor;
using Statiq.Common.Documents;
using Statiq.Common.Execution;
using Statiq.Common.Meta;
using Statiq.Common.Tracing;

namespace Statiq.Razor
{
    public abstract class StatiqRazorPage<TModel> : RazorPage<TModel>
    {
        public IDocument Document => ViewData[ViewDataKeys.StatiqDocument] as IDocument;
        public IMetadata Metadata => Document;

        public IExecutionContext ExecutionContext => ViewData[ViewDataKeys.StatiqExecutionContext] as IExecutionContext;
        public new IExecutionContext Context => ExecutionContext;
        public HttpContext HttpContext => base.Context;

        public IDocumentCollection Documents => ExecutionContext.Documents;

        public ITrace Trace => Common.Tracing.Trace.Current;
    }
}