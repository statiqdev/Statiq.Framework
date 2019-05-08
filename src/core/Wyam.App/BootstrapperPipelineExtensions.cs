using System;
using System.Collections.Generic;
using Wyam.Common.Configuration;
using Wyam.Common.Execution;
using Wyam.Common.IO;
using Wyam.Common.Modules;

namespace Wyam.App
{
    public static class BootstrapperPipelineExtensions
    {
        // Directly

        public static IBootstrapper AddPipelines(
            this IBootstrapper boostrapper,
            Action<IPipelineCollection> action) =>
            boostrapper.Configure<IEngine>(x => action(x.Pipelines));

        public static IBootstrapper AddPipelines(
            this IBootstrapper boostrapper,
            Action<IReadOnlySettings, IPipelineCollection> action) =>
            boostrapper.Configure<IEngine>(x => action(x.Settings, x.Pipelines));

        // By type

        public static IBootstrapper AddPipeline(
            this IBootstrapper bootstrapper,
            string name,
            IPipeline pipeline) =>
            bootstrapper.Configure<IEngine>(x => x.Pipelines.Add(name, pipeline));

        public static IBootstrapper AddPipeline(
            this IBootstrapper bootstrapper,
            IPipeline pipeline) =>
            bootstrapper.Configure<IEngine>(x => x.Pipelines.Add(pipeline));

        public static IBootstrapper AddPipeline(
            this IBootstrapper bootstrapper,
            string name,
            Func<IReadOnlySettings, IPipeline> pipelineFunc) =>
            bootstrapper.Configure<IEngine>(x => x.Pipelines.Add(name, pipelineFunc(x.Settings)));

        public static IBootstrapper AddPipeline(
            this IBootstrapper bootstrapper,
            Func<IReadOnlySettings, IPipeline> pipelineFunc) =>
            bootstrapper.Configure<IEngine>(x => x.Pipelines.Add(pipelineFunc(x.Settings)));

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

        public static IBootstrapper BuildPipeline(
            this IBootstrapper bootstrapper,
            string name,
            Action<PipelineBuilder> buildAction) =>
            bootstrapper.Configure<IEngine>(x =>
            {
                PipelineBuilder builder = new PipelineBuilder(x.Pipelines, x.Settings);
                buildAction(builder);
                IPipeline pipeline = builder.Build();
                if (pipeline != null)
                {
                    x.Pipelines.Add(name, pipeline);
                }
            });

        // Without dependencies

        public static IBootstrapper AddPipeline(
            this IBootstrapper bootstrapper,
            string name,
            IEnumerable<IModule> processModules) =>
            AddPipeline(bootstrapper, Array.Empty<string>(), true, name, (IEnumerable<IModule>)null, processModules, null, null);

        public static IBootstrapper AddPipeline(
            this IBootstrapper bootstrapper,
            string name,
            IEnumerable<IModule> readModules = null,
            IEnumerable<IModule> processModules = null,
            IEnumerable<IModule> renderModules = null,
            IEnumerable<IModule> writeModules = null) =>
            AddPipeline(bootstrapper, Array.Empty<string>(), true, name, readModules, processModules, renderModules, writeModules);

        public static IBootstrapper AddPipeline(
            this IBootstrapper bootstrapper,
            string name,
            string readPattern,
            IEnumerable<IModule> processModules = null,
            IEnumerable<IModule> renderModules = null,
            IEnumerable<IModule> writeModules = null) =>
            AddPipeline(bootstrapper, Array.Empty<string>(), true, name, readPattern, processModules, renderModules, writeModules);

        public static IBootstrapper AddPipeline(
            this IBootstrapper bootstrapper,
            string name,
            string readPattern,
            bool writeFiles,
            IEnumerable<IModule> processModules = null,
            IEnumerable<IModule> renderModules = null) =>
            AddPipeline(bootstrapper, Array.Empty<string>(), true, name, readPattern, writeFiles, processModules, renderModules);

        public static IBootstrapper AddPipeline(
            this IBootstrapper bootstrapper,
            string name,
            string readPattern,
            string writeExtension,
            IEnumerable<IModule> processModules = null,
            IEnumerable<IModule> renderModules = null) =>
            AddPipeline(bootstrapper, Array.Empty<string>(), true, name, readPattern, writeExtension, processModules, renderModules);

        public static IBootstrapper AddPipeline(
            this IBootstrapper bootstrapper,
            string name,
            string readPattern,
            DocumentConfig<FilePath> writePath,
            IEnumerable<IModule> processModules = null,
            IEnumerable<IModule> renderModules = null) =>
            AddPipeline(bootstrapper, Array.Empty<string>(), true, name, readPattern, writePath, processModules, renderModules);

        public static IBootstrapper AddPipeline(
            this IBootstrapper bootstrapper,
            string name,
            params IModule[] processModules) =>
            AddPipeline(bootstrapper, Array.Empty<string>(), true, name, (IEnumerable<IModule>)null, processModules, null, null);

        public static IBootstrapper AddPipeline(
            this IBootstrapper bootstrapper,
            string name,
            string readPattern,
            bool writeFiles,
            params IModule[] processModules) =>
            AddPipeline(bootstrapper, Array.Empty<string>(), false, name, readPattern, writeFiles, processModules, null);

        public static IBootstrapper AddPipeline(
            this IBootstrapper bootstrapper,
            string name,
            string readPattern,
            string writeExtension,
            params IModule[] processModules) =>
            AddPipeline(bootstrapper, Array.Empty<string>(), false, name, readPattern, writeExtension, processModules, null);

        public static IBootstrapper AddPipeline(
            this IBootstrapper bootstrapper,
            string name,
            string readPattern,
            DocumentConfig<FilePath> writePath,
            params IModule[] processModules) =>
            AddPipeline(bootstrapper, Array.Empty<string>(), false, name, readPattern, writePath, processModules, null);

        // With dependencies

        public static IBootstrapper AddPipeline(
            this IBootstrapper bootstrapper,
            string name,
            IEnumerable<string> dependencies,
            IEnumerable<IModule> processModules) =>
            AddPipeline(bootstrapper, dependencies, true, name, (IEnumerable<IModule>)null, processModules, null, null);

        public static IBootstrapper AddPipeline(
            this IBootstrapper bootstrapper,
            string name,
            IEnumerable<string> dependencies,
            IEnumerable<IModule> readModules = null,
            IEnumerable<IModule> processModules = null,
            IEnumerable<IModule> renderModules = null,
            IEnumerable<IModule> writeModules = null) =>
            AddPipeline(bootstrapper, dependencies, true, name, readModules, processModules, renderModules, writeModules);

        public static IBootstrapper AddPipeline(
            this IBootstrapper bootstrapper,
            string name,
            IEnumerable<string> dependencies,
            string readPattern,
            IEnumerable<IModule> processModules = null,
            IEnumerable<IModule> renderModules = null,
            IEnumerable<IModule> writeModules = null) =>
            AddPipeline(bootstrapper, dependencies, true, name, readPattern, processModules, renderModules, writeModules);

        public static IBootstrapper AddPipeline(
            this IBootstrapper bootstrapper,
            string name,
            IEnumerable<string> dependencies,
            string readPattern,
            bool writeFiles,
            IEnumerable<IModule> processModules = null,
            IEnumerable<IModule> renderModules = null) =>
            AddPipeline(bootstrapper, dependencies, true, name, readPattern, writeFiles, processModules, renderModules);

        public static IBootstrapper AddPipeline(
            this IBootstrapper bootstrapper,
            string name,
            IEnumerable<string> dependencies,
            string readPattern,
            string writeExtension,
            IEnumerable<IModule> processModules = null,
            IEnumerable<IModule> renderModules = null) =>
            AddPipeline(bootstrapper, dependencies, true, name, readPattern, writeExtension, processModules, renderModules);

        public static IBootstrapper AddPipeline(
            this IBootstrapper bootstrapper,
            string name,
            IEnumerable<string> dependencies,
            string readPattern,
            DocumentConfig<FilePath> writePath,
            IEnumerable<IModule> processModules = null,
            IEnumerable<IModule> renderModules = null) =>
            AddPipeline(bootstrapper, dependencies, true, name, readPattern, writePath, processModules, renderModules);

        public static IBootstrapper AddPipeline(
            this IBootstrapper bootstrapper,
            string name,
            IEnumerable<string> dependencies,
            params IModule[] processModules) =>
            AddPipeline(bootstrapper, dependencies, true, name, (IEnumerable<IModule>)null, processModules, null, null);

        public static IBootstrapper AddPipeline(
            this IBootstrapper bootstrapper,
            string name,
            IEnumerable<string> dependencies,
            string readPattern,
            bool writeFiles,
            params IModule[] processModules) =>
            AddPipeline(bootstrapper, dependencies, false, name, readPattern, writeFiles, processModules, null);

        public static IBootstrapper AddPipeline(
            this IBootstrapper bootstrapper,
            string name,
            IEnumerable<string> dependencies,
            string readPattern,
            string writeExtension,
            params IModule[] processModules) =>
            AddPipeline(bootstrapper, dependencies, false, name, readPattern, writeExtension, processModules, null);

        public static IBootstrapper AddPipeline(
            this IBootstrapper bootstrapper,
            string name,
            IEnumerable<string> dependencies,
            string readPattern,
            DocumentConfig<FilePath> writePath,
            params IModule[] processModules) =>
            AddPipeline(bootstrapper, dependencies, false, name, readPattern, writePath, processModules, null);

        // Serial

        public static IBootstrapper AddSerialPipeline(
            this IBootstrapper bootstrapper,
            string name,
            IEnumerable<IModule> processModules) =>
            AddPipeline(bootstrapper, null, false, name, (IEnumerable<IModule>)null, processModules, null, null);

        public static IBootstrapper AddSerialPipeline(
            this IBootstrapper bootstrapper,
            string name,
            IEnumerable<IModule> readModules = null,
            IEnumerable<IModule> processModules = null,
            IEnumerable<IModule> renderModules = null,
            IEnumerable<IModule> writeModules = null) =>
            AddPipeline(bootstrapper, null, false, name, readModules, processModules, renderModules, writeModules);

        public static IBootstrapper AddSerialPipeline(
            this IBootstrapper bootstrapper,
            string name,
            string readPattern,
            IEnumerable<IModule> processModules = null,
            IEnumerable<IModule> renderModules = null,
            IEnumerable<IModule> writeModules = null) =>
            AddPipeline(bootstrapper, null, false, name, readPattern, processModules, renderModules, writeModules);

        public static IBootstrapper AddSerialPipeline(
            this IBootstrapper bootstrapper,
            string name,
            string readPattern,
            bool writeFiles,
            IEnumerable<IModule> processModules = null,
            IEnumerable<IModule> renderModules = null) =>
            AddPipeline(bootstrapper, null, false, name, readPattern, writeFiles, processModules, renderModules);

        public static IBootstrapper AddSerialPipeline(
            this IBootstrapper bootstrapper,
            string name,
            string readPattern,
            string writeExtension,
            IEnumerable<IModule> processModules = null,
            IEnumerable<IModule> renderModules = null) =>
            AddPipeline(bootstrapper, null, false, name, readPattern, writeExtension, processModules, renderModules);

        public static IBootstrapper AddSerialPipeline(
            this IBootstrapper bootstrapper,
            string name,
            string readPattern,
            DocumentConfig<FilePath> writePath,
            IEnumerable<IModule> processModules = null,
            IEnumerable<IModule> renderModules = null) =>
            AddPipeline(bootstrapper, null, false, name, readPattern, writePath, processModules, renderModules);

        public static IBootstrapper AddSerialPipeline(
            this IBootstrapper bootstrapper,
            string name,
            params IModule[] processModules) =>
            AddPipeline(bootstrapper, null, false, name, (IEnumerable<IModule>)null, processModules, null, null);

        public static IBootstrapper AddSerialPipeline(
            this IBootstrapper bootstrapper,
            string name,
            string readPattern,
            bool writeFiles,
            params IModule[] processModules) =>
            AddPipeline(bootstrapper, null, false, name, readPattern, writeFiles, processModules, null);

        public static IBootstrapper AddSerialPipeline(
            this IBootstrapper bootstrapper,
            string name,
            string readPattern,
            string writeExtension,
            params IModule[] processModules) =>
            AddPipeline(bootstrapper, null, false, name, readPattern, writeExtension, processModules, null);

        public static IBootstrapper AddSerialPipeline(
            this IBootstrapper bootstrapper,
            string name,
            string readPattern,
            DocumentConfig<FilePath> writePath,
            params IModule[] processModules) =>
            AddPipeline(bootstrapper, null, false, name, readPattern, writePath, processModules, null);

        // Isolated

        public static IBootstrapper AddIsolatedPipeline(
            this IBootstrapper bootstrapper,
            string name,
            IEnumerable<IModule> processModules) =>
            AddPipeline(bootstrapper, null, true, name, (IEnumerable<IModule>)null, processModules, null, null);

        public static IBootstrapper AddIsolatedPipeline(
            this IBootstrapper bootstrapper,
            string name,
            IEnumerable<IModule> readModules = null,
            IEnumerable<IModule> processModules = null,
            IEnumerable<IModule> renderModules = null,
            IEnumerable<IModule> writeModules = null) =>
            AddPipeline(bootstrapper, null, true, name, readModules, processModules, renderModules, writeModules);

        public static IBootstrapper AddIsolatedPipeline(
            this IBootstrapper bootstrapper,
            string name,
            string readPattern,
            IEnumerable<IModule> processModules = null,
            IEnumerable<IModule> renderModules = null,
            IEnumerable<IModule> writeModules = null) =>
            AddPipeline(bootstrapper, null, true, name, readPattern, processModules, renderModules, writeModules);

        public static IBootstrapper AddIsolatedPipeline(
            this IBootstrapper bootstrapper,
            string name,
            string readPattern,
            bool writeFiles,
            IEnumerable<IModule> processModules = null,
            IEnumerable<IModule> renderModules = null) =>
            AddPipeline(bootstrapper, null, true, name, readPattern, writeFiles, processModules, renderModules);

        public static IBootstrapper AddIsolatedPipeline(
            this IBootstrapper bootstrapper,
            string name,
            string readPattern,
            string writeExtension,
            IEnumerable<IModule> processModules = null,
            IEnumerable<IModule> renderModules = null) =>
            AddPipeline(bootstrapper, null, true, name, readPattern, writeExtension, processModules, renderModules);

        public static IBootstrapper AddIsolatedPipeline(
            this IBootstrapper bootstrapper,
            string name,
            string readPattern,
            DocumentConfig<FilePath> writePath,
            IEnumerable<IModule> processModules = null,
            IEnumerable<IModule> renderModules = null) =>
            AddPipeline(bootstrapper, null, true, name, readPattern, writePath, processModules, renderModules);

        public static IBootstrapper AddIsolatedPipeline(
            this IBootstrapper bootstrapper,
            string name,
            params IModule[] processModules) =>
            AddPipeline(bootstrapper, null, true, name, (IEnumerable<IModule>)null, processModules, null, null);

        public static IBootstrapper AddIsolatedPipeline(
            this IBootstrapper bootstrapper,
            string name,
            string readPattern,
            bool writeFiles,
            params IModule[] processModules) =>
            AddPipeline(bootstrapper, null, true, name, readPattern, writeFiles, processModules, null);

        public static IBootstrapper AddIsolatedPipeline(
            this IBootstrapper bootstrapper,
            string name,
            string readPattern,
            string writeExtension,
            params IModule[] processModules) =>
            AddPipeline(bootstrapper, null, true, name, readPattern, writeExtension, processModules, null);

        public static IBootstrapper AddIsolatedPipeline(
            this IBootstrapper bootstrapper,
            string name,
            string readPattern,
            DocumentConfig<FilePath> writePath,
            params IModule[] processModules) =>
            AddPipeline(bootstrapper, null, true, name, readPattern, writePath, processModules, null);

        // Helpers for adding pipelines from modules

        private static IBootstrapper AddPipeline(
            IBootstrapper bootstrapper,
            IEnumerable<string> dependencies,
            bool isolated,
            string name,
            IEnumerable<IModule> readModules,
            IEnumerable<IModule> processModules,
            IEnumerable<IModule> renderModules,
            IEnumerable<IModule> writeModules) =>
            bootstrapper.BuildPipeline(name, builder =>
            {
                builder
                    .WithDependencies(dependencies, isolated)
                    .WithReadModules(readModules)
                    .WithProcessModules(processModules)
                    .WithRenderModules(renderModules)
                    .WithWriteModules(writeModules);
            });

        private static IBootstrapper AddPipeline(
            IBootstrapper bootstrapper,
            IEnumerable<string> dependencies,
            bool isolated,
            string name,
            string readPattern,
            IEnumerable<IModule> processModules,
            IEnumerable<IModule> renderModules,
            IEnumerable<IModule> writeModules) =>
            bootstrapper.BuildPipeline(name, builder =>
            {
                builder
                    .WithDependencies(dependencies, isolated)
                    .WithProcessModules(processModules)
                    .WithRenderModules(renderModules)
                    .WithWriteModules(writeModules);
                if (readPattern != null)
                {
                    builder.WithReadFiles(readPattern);
                }
            });

        private static IBootstrapper AddPipeline(
            IBootstrapper bootstrapper,
            IEnumerable<string> dependencies,
            bool isolated,
            string name,
            string readPattern,
            bool writeFiles,
            IEnumerable<IModule> processModules,
            IEnumerable<IModule> renderModules) =>
            bootstrapper.BuildPipeline(name, builder =>
            {
                builder
                    .WithDependencies(dependencies, isolated)
                    .WithProcessModules(processModules)
                    .WithRenderModules(renderModules);
                if (readPattern != null)
                {
                    builder.WithReadFiles(readPattern);
                }
                if (writeFiles)
                {
                    builder.WithWriteFiles();
                }
            });

        private static IBootstrapper AddPipeline(
            IBootstrapper bootstrapper,
            IEnumerable<string> dependencies,
            bool isolated,
            string name,
            string readPattern,
            string writeExtension,
            IEnumerable<IModule> processModules,
            IEnumerable<IModule> renderModules) =>
            bootstrapper.BuildPipeline(name, builder =>
            {
                builder
                    .WithDependencies(dependencies, isolated)
                    .WithProcessModules(processModules)
                    .WithRenderModules(renderModules)
                    .WithWriteFiles(writeExtension);
                if (readPattern != null)
                {
                    builder.WithReadFiles(readPattern);
                }
            });

        private static IBootstrapper AddPipeline(
            IBootstrapper bootstrapper,
            IEnumerable<string> dependencies,
            bool isolated,
            string name,
            string readPattern,
            DocumentConfig<FilePath> writePath,
            IEnumerable<IModule> processModules,
            IEnumerable<IModule> renderModules) =>
            bootstrapper.BuildPipeline(name, builder =>
            {
                builder
                    .WithDependencies(dependencies, isolated)
                    .WithProcessModules(processModules)
                    .WithRenderModules(renderModules)
                    .WithWriteFiles(writePath);
                if (readPattern != null)
                {
                    builder.WithReadFiles(readPattern);
                }
            });

        private static PipelineBuilder WithDependencies(this PipelineBuilder builder, IEnumerable<string> dependencies, bool isolated)
        {
            if (dependencies != null)
            {
                builder.WithDependencies(dependencies);
            }
            else
            {
                if (isolated)
                {
                    builder.AsIsolated();
                }
                else
                {
                    builder.AsSerial();
                }
            }
            return builder;
        }
    }
}
