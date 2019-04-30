using System;
using System.Collections.Generic;
using System.Text;
using Wyam.Common.Configuration;
using Wyam.Common.Execution;
using Wyam.Common.Modules;
using Wyam.Common.Util;

namespace Wyam.App
{
    public static class BootstrapperPipelineExtensions
    {
        // Add directly

        public static IBootstrapper AddPipelines(
            this IBootstrapper boostrapper,
            Action<IPipelineCollection> action) =>
            boostrapper.Configure<IEngine>(x => action(x.Pipelines));

        public static IBootstrapper AddPipelines(
            this IBootstrapper boostrapper,
            Action<IReadOnlySettings, IPipelineCollection> action) =>
            boostrapper.Configure<IEngine>(x => action(x.Settings, x.Pipelines));

        // Add directly and by type

        public static IBootstrapper AddPipeline(
            this IBootstrapper bootstrapper,
            string name,
            IPipeline pipeline) =>
            bootstrapper.Configure<IEngine>(x => x.Pipelines.Add(name, pipeline));

        public static IBootstrapper AddPipeline(
            this IBootstrapper bootstrapper,
            IPipeline pipeline) =>
            bootstrapper.Configure<IEngine>(x => x.Pipelines.Add(pipeline));

        public static IBootstrapper AddPipeline<TPipeline>(
            this IBootstrapper bootstrapper,
            string name)
            where TPipeline : IPipeline =>
            bootstrapper.Configure<IEngine>(x => x.Pipelines.Add<TPipeline>(name));

        public static IBootstrapper AddPipeline<TPipeline>(
            this IBootstrapper bootstrapper)
            where TPipeline : IPipeline =>
            bootstrapper.Configure<IEngine>(x => x.Pipelines.Add<TPipeline>());

        // Builder

        public static IBootstrapper AddPipeline(
            this IBootstrapper bootstrapper,
            string name,
            Func<PipelineBuilder, IPipeline> build) =>
            bootstrapper.Configure<IEngine>(x =>
            {
                PipelineBuilder builder = new PipelineBuilder(x.Pipelines, x.Settings);
                IPipeline pipeline = build(builder);
                if (pipeline != null)
                {
                    x.Pipelines.Add(name, pipeline);
                }
            });

        // Add with modules (adds as serial with dependencies on previously added pipelines)

        public static IBootstrapper AddPipeline(
            this IBootstrapper bootstrapper,
            string name,
            IEnumerable<IModule> processModules) =>
            bootstrapper.AddPipeline(name, null, processModules, null, null);

        public static IBootstrapper AddPipeline(
            this IBootstrapper bootstrapper,
            string name,
            params IModule[] processModules) =>
            bootstrapper.AddPipeline(name, null, processModules, null, null);

        public static IBootstrapper AddPipeline(
            this IBootstrapper bootstrapper,
            string name,
            IEnumerable<IModule> readModules,
            IEnumerable<IModule> processModules,
            IEnumerable<IModule> renderModules,
            IEnumerable<IModule> writeModules) =>
            bootstrapper.Configure<IEngine>(x =>
            {
                IPipeline pipeline = x.Pipelines.Add(name);
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
                pipeline.Dependencies.AddRange(x.Pipelines.Keys);
            });
    }
}
