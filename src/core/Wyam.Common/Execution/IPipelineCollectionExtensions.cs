using System;
using System.Collections.Generic;
using System.Linq;
using Wyam.Common.Modules;
using Wyam.Common.Util;

namespace Wyam.Common.Execution
{
    /// <summary>
    /// Extensions for working with pipeline collections.
    /// </summary>
    public static class IPipelineCollectionExtensions
    {
        public static IPipeline AddSequential(this IPipelineCollection pipelines, string name, params IModule[] processModules) =>
            pipelines.AddSequential(name, processModules);

        public static IPipeline Add(this IPipelineCollection pipelines, string name, params IModule[] processModules) =>
            pipelines.Add(name, null, processModules, null, null);

        public static IPipeline Add(this IPipelineCollection pipelines, string name, IEnumerable<IModule> processModules) =>
            pipelines.Add(name, null, processModules, null, null);

        public static IPipeline Add(
            this IPipelineCollection pipelines,
            string name,
            IEnumerable<IModule> readModules,
            IEnumerable<IModule> processModules,
            IEnumerable<IModule> renderModules,
            IEnumerable<IModule> writeModules)
        {
            IPipeline pipeline = pipelines.Add(name);
            if (readModules != null)
            {
                pipeline.Read.AddRange(readModules);
            }
            if (processModules != null)
            {
                pipeline.Process.AddRange(processModules);
            }
            if (renderModules != null)
            {
                pipeline.Render.AddRange(renderModules);
            }
            if (writeModules != null)
            {
                pipeline.Write.AddRange(writeModules);
            }
            return pipeline;
        }

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