using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Statiq.Common;

namespace Statiq.Core
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
