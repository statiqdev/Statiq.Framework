using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Statiq.Common.Documents;
using Statiq.Common.Execution;
using Statiq.Core.Modules.Extensibility;

namespace Statiq.Core.Execution
{
    public static class IPipelineExtensions
    {
        public static TPipeline WithInputDelegate<TPipeline>(this TPipeline pipeline, Action<IReadOnlyList<IDocument>, IExecutionContext> action)
            where TPipeline : IPipeline
        {
            pipeline.InputModules.Add(new Execute(action));
            return pipeline;
        }

        public static TPipeline WithInputDelegate<TPipeline>(this TPipeline pipeline, Func<IReadOnlyList<IDocument>, IExecutionContext, Task> func)
            where TPipeline : IPipeline
        {
            pipeline.InputModules.Add(new Execute(func));
            return pipeline;
        }

        public static TPipeline WithInputDelegate<TPipeline>(this TPipeline pipeline, Func<IReadOnlyList<IDocument>, IExecutionContext, Task<object>> func)
            where TPipeline : IPipeline
        {
            pipeline.InputModules.Add(new Execute(func));
            return pipeline;
        }

        public static TPipeline WithProcessDelegate<TPipeline>(this TPipeline pipeline, Action<IReadOnlyList<IDocument>, IExecutionContext> action)
            where TPipeline : IPipeline
        {
            pipeline.ProcessModules.Add(new Execute(action));
            return pipeline;
        }

        public static TPipeline WithProcessDelegate<TPipeline>(this TPipeline pipeline, Func<IReadOnlyList<IDocument>, IExecutionContext, Task> func)
            where TPipeline : IPipeline
        {
            pipeline.ProcessModules.Add(new Execute(func));
            return pipeline;
        }

        public static TPipeline WithProcessDelegate<TPipeline>(this TPipeline pipeline, Func<IReadOnlyList<IDocument>, IExecutionContext, Task<object>> func)
            where TPipeline : IPipeline
        {
            pipeline.ProcessModules.Add(new Execute(func));
            return pipeline;
        }

        public static TPipeline WithTransformDelegate<TPipeline>(this TPipeline pipeline, Action<IReadOnlyList<IDocument>, IExecutionContext> action)
            where TPipeline : IPipeline
        {
            pipeline.TransformModules.Add(new Execute(action));
            return pipeline;
        }

        public static TPipeline WithTransformDelegate<TPipeline>(this TPipeline pipeline, Func<IReadOnlyList<IDocument>, IExecutionContext, Task> func)
            where TPipeline : IPipeline
        {
            pipeline.TransformModules.Add(new Execute(func));
            return pipeline;
        }

        public static TPipeline WithTransformDelegate<TPipeline>(this TPipeline pipeline, Func<IReadOnlyList<IDocument>, IExecutionContext, Task<object>> func)
            where TPipeline : IPipeline
        {
            pipeline.TransformModules.Add(new Execute(func));
            return pipeline;
        }

        public static TPipeline WithOutputDelegate<TPipeline>(this TPipeline pipeline, Action<IReadOnlyList<IDocument>, IExecutionContext> action)
            where TPipeline : IPipeline
        {
            pipeline.OutputModules.Add(new Execute(action));
            return pipeline;
        }

        public static TPipeline WithOutputDelegate<TPipeline>(this TPipeline pipeline, Func<IReadOnlyList<IDocument>, IExecutionContext, Task> func)
            where TPipeline : IPipeline
        {
            pipeline.OutputModules.Add(new Execute(func));
            return pipeline;
        }

        public static TPipeline WithOutputDelegate<TPipeline>(this TPipeline pipeline, Func<IReadOnlyList<IDocument>, IExecutionContext, Task<object>> func)
            where TPipeline : IPipeline
        {
            pipeline.OutputModules.Add(new Execute(func));
            return pipeline;
        }
    }
}
