using System;

namespace Wyam.Common.Execution
{
    /// <summary>
    /// Extensions for working with pipeline collections.
    /// </summary>
    public static class IPipelineCollectionExtensions
    {
        public static void Add(this IPipelineCollection pipelines, IPipeline pipeline) =>
            pipelines.Add(pipeline?.GetType().Name, pipeline);

        public static IPipeline Add<TPipeline>(this IPipelineCollection pipelines, string name)
            where TPipeline : IPipeline
        {
            IPipeline pipeline = Activator.CreateInstance<TPipeline>();
            pipelines.Add(name, pipeline);
            return pipeline;
        }

        public static IPipeline Add<TPipeline>(this IPipelineCollection pipelines)
            where TPipeline : IPipeline =>
            pipelines.Add<TPipeline>(typeof(TPipeline).Name);
    }
}