using System;
using System.Collections.Generic;
using Statiq.Common.Configuration;
using Statiq.Common.Execution;
using Statiq.Common.IO;
using Statiq.Common.Modules;

namespace Statiq.App
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
            IEnumerable<IModule> inputModules = null,
            IEnumerable<IModule> processModules = null,
            IEnumerable<IModule> transformModules = null,
            IEnumerable<IModule> outputModules = null) =>
            AddPipeline(bootstrapper, Array.Empty<string>(), true, name, inputModules, processModules, transformModules, outputModules);

        public static IBootstrapper AddPipeline(
            this IBootstrapper bootstrapper,
            string name,
            string readFilesPattern,
            IEnumerable<IModule> processModules = null,
            IEnumerable<IModule> transformModules = null,
            IEnumerable<IModule> outputModules = null) =>
            AddPipeline(bootstrapper, Array.Empty<string>(), true, name, readFilesPattern, processModules, transformModules, outputModules);

        public static IBootstrapper AddPipeline(
            this IBootstrapper bootstrapper,
            string name,
            string readFilesPattern,
            bool writeFiles,
            IEnumerable<IModule> processModules = null,
            IEnumerable<IModule> transformModules = null) =>
            AddPipeline(bootstrapper, Array.Empty<string>(), true, name, readFilesPattern, writeFiles, processModules, transformModules);

        public static IBootstrapper AddPipeline(
            this IBootstrapper bootstrapper,
            string name,
            string readFilesPattern,
            string destinationExtension,
            IEnumerable<IModule> processModules = null,
            IEnumerable<IModule> transformModules = null) =>
            AddPipeline(bootstrapper, Array.Empty<string>(), true, name, readFilesPattern, destinationExtension, processModules, transformModules);

        public static IBootstrapper AddPipeline(
            this IBootstrapper bootstrapper,
            string name,
            string readFilesPattern,
            DocumentConfig<FilePath> destinationPath,
            IEnumerable<IModule> processModules = null,
            IEnumerable<IModule> transformModules = null) =>
            AddPipeline(bootstrapper, Array.Empty<string>(), true, name, readFilesPattern, destinationPath, processModules, transformModules);

        public static IBootstrapper AddPipeline(
            this IBootstrapper bootstrapper,
            string name,
            params IModule[] processModules) =>
            AddPipeline(bootstrapper, Array.Empty<string>(), true, name, (IEnumerable<IModule>)null, processModules, null, null);

        public static IBootstrapper AddPipeline(
            this IBootstrapper bootstrapper,
            string name,
            string readFilesPattern,
            bool writeFiles,
            params IModule[] processModules) =>
            AddPipeline(bootstrapper, Array.Empty<string>(), false, name, readFilesPattern, writeFiles, processModules, null);

        public static IBootstrapper AddPipeline(
            this IBootstrapper bootstrapper,
            string name,
            string readFilesPattern,
            string destinationExtension,
            params IModule[] processModules) =>
            AddPipeline(bootstrapper, Array.Empty<string>(), false, name, readFilesPattern, destinationExtension, processModules, null);

        public static IBootstrapper AddPipeline(
            this IBootstrapper bootstrapper,
            string name,
            string readFilesPattern,
            DocumentConfig<FilePath> destinationPath,
            params IModule[] processModules) =>
            AddPipeline(bootstrapper, Array.Empty<string>(), false, name, readFilesPattern, destinationPath, processModules, null);

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
            IEnumerable<IModule> inputModules = null,
            IEnumerable<IModule> processModules = null,
            IEnumerable<IModule> transformModules = null,
            IEnumerable<IModule> outputModules = null) =>
            AddPipeline(bootstrapper, dependencies, true, name, inputModules, processModules, transformModules, outputModules);

        public static IBootstrapper AddPipeline(
            this IBootstrapper bootstrapper,
            string name,
            IEnumerable<string> dependencies,
            string readFilesPattern,
            IEnumerable<IModule> processModules = null,
            IEnumerable<IModule> transformModules = null,
            IEnumerable<IModule> outputModules = null) =>
            AddPipeline(bootstrapper, dependencies, true, name, readFilesPattern, processModules, transformModules, outputModules);

        public static IBootstrapper AddPipeline(
            this IBootstrapper bootstrapper,
            string name,
            IEnumerable<string> dependencies,
            string readFilesPattern,
            bool writeFiles,
            IEnumerable<IModule> processModules = null,
            IEnumerable<IModule> transformModules = null) =>
            AddPipeline(bootstrapper, dependencies, true, name, readFilesPattern, writeFiles, processModules, transformModules);

        public static IBootstrapper AddPipeline(
            this IBootstrapper bootstrapper,
            string name,
            IEnumerable<string> dependencies,
            string readFilesPattern,
            string destinationExtension,
            IEnumerable<IModule> processModules = null,
            IEnumerable<IModule> transformModules = null) =>
            AddPipeline(bootstrapper, dependencies, true, name, readFilesPattern, destinationExtension, processModules, transformModules);

        public static IBootstrapper AddPipeline(
            this IBootstrapper bootstrapper,
            string name,
            IEnumerable<string> dependencies,
            string readFilesPattern,
            DocumentConfig<FilePath> destinationPath,
            IEnumerable<IModule> processModules = null,
            IEnumerable<IModule> transformModules = null) =>
            AddPipeline(bootstrapper, dependencies, true, name, readFilesPattern, destinationPath, processModules, transformModules);

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
            string readFilesPattern,
            bool writeFiles,
            params IModule[] processModules) =>
            AddPipeline(bootstrapper, dependencies, false, name, readFilesPattern, writeFiles, processModules, null);

        public static IBootstrapper AddPipeline(
            this IBootstrapper bootstrapper,
            string name,
            IEnumerable<string> dependencies,
            string readFilesPattern,
            string destinationExtension,
            params IModule[] processModules) =>
            AddPipeline(bootstrapper, dependencies, false, name, readFilesPattern, destinationExtension, processModules, null);

        public static IBootstrapper AddPipeline(
            this IBootstrapper bootstrapper,
            string name,
            IEnumerable<string> dependencies,
            string readFilesPattern,
            DocumentConfig<FilePath> destinationPath,
            params IModule[] processModules) =>
            AddPipeline(bootstrapper, dependencies, false, name, readFilesPattern, destinationPath, processModules, null);

        // Serial

        public static IBootstrapper AddSerialPipeline(
            this IBootstrapper bootstrapper,
            string name,
            IEnumerable<IModule> processModules) =>
            AddPipeline(bootstrapper, null, false, name, (IEnumerable<IModule>)null, processModules, null, null);

        public static IBootstrapper AddSerialPipeline(
            this IBootstrapper bootstrapper,
            string name,
            IEnumerable<IModule> inputModules = null,
            IEnumerable<IModule> processModules = null,
            IEnumerable<IModule> transformModules = null,
            IEnumerable<IModule> outputModules = null) =>
            AddPipeline(bootstrapper, null, false, name, inputModules, processModules, transformModules, outputModules);

        public static IBootstrapper AddSerialPipeline(
            this IBootstrapper bootstrapper,
            string name,
            string readFilesPattern,
            IEnumerable<IModule> processModules = null,
            IEnumerable<IModule> transformModules = null,
            IEnumerable<IModule> outputModules = null) =>
            AddPipeline(bootstrapper, null, false, name, readFilesPattern, processModules, transformModules, outputModules);

        public static IBootstrapper AddSerialPipeline(
            this IBootstrapper bootstrapper,
            string name,
            string readFilesPattern,
            bool writeFiles,
            IEnumerable<IModule> processModules = null,
            IEnumerable<IModule> transformModules = null) =>
            AddPipeline(bootstrapper, null, false, name, readFilesPattern, writeFiles, processModules, transformModules);

        public static IBootstrapper AddSerialPipeline(
            this IBootstrapper bootstrapper,
            string name,
            string readFilesPattern,
            string destinationExtension,
            IEnumerable<IModule> processModules = null,
            IEnumerable<IModule> transformModules = null) =>
            AddPipeline(bootstrapper, null, false, name, readFilesPattern, destinationExtension, processModules, transformModules);

        public static IBootstrapper AddSerialPipeline(
            this IBootstrapper bootstrapper,
            string name,
            string readFilesPattern,
            DocumentConfig<FilePath> destinationPath,
            IEnumerable<IModule> processModules = null,
            IEnumerable<IModule> transformModules = null) =>
            AddPipeline(bootstrapper, null, false, name, readFilesPattern, destinationPath, processModules, transformModules);

        public static IBootstrapper AddSerialPipeline(
            this IBootstrapper bootstrapper,
            string name,
            params IModule[] processModules) =>
            AddPipeline(bootstrapper, null, false, name, (IEnumerable<IModule>)null, processModules, null, null);

        public static IBootstrapper AddSerialPipeline(
            this IBootstrapper bootstrapper,
            string name,
            string readFilesPattern,
            bool writeFiles,
            params IModule[] processModules) =>
            AddPipeline(bootstrapper, null, false, name, readFilesPattern, writeFiles, processModules, null);

        public static IBootstrapper AddSerialPipeline(
            this IBootstrapper bootstrapper,
            string name,
            string readFilesPattern,
            string destinationExtension,
            params IModule[] processModules) =>
            AddPipeline(bootstrapper, null, false, name, readFilesPattern, destinationExtension, processModules, null);

        public static IBootstrapper AddSerialPipeline(
            this IBootstrapper bootstrapper,
            string name,
            string readFilesPattern,
            DocumentConfig<FilePath> destinationPath,
            params IModule[] processModules) =>
            AddPipeline(bootstrapper, null, false, name, readFilesPattern, destinationPath, processModules, null);

        // Isolated

        public static IBootstrapper AddIsolatedPipeline(
            this IBootstrapper bootstrapper,
            string name,
            IEnumerable<IModule> processModules) =>
            AddPipeline(bootstrapper, null, true, name, (IEnumerable<IModule>)null, processModules, null, null);

        public static IBootstrapper AddIsolatedPipeline(
            this IBootstrapper bootstrapper,
            string name,
            IEnumerable<IModule> inputModules = null,
            IEnumerable<IModule> processModules = null,
            IEnumerable<IModule> transformModules = null,
            IEnumerable<IModule> outputModules = null) =>
            AddPipeline(bootstrapper, null, true, name, inputModules, processModules, transformModules, outputModules);

        public static IBootstrapper AddIsolatedPipeline(
            this IBootstrapper bootstrapper,
            string name,
            string readFilesPattern,
            IEnumerable<IModule> processModules = null,
            IEnumerable<IModule> transformModules = null,
            IEnumerable<IModule> outputModules = null) =>
            AddPipeline(bootstrapper, null, true, name, readFilesPattern, processModules, transformModules, outputModules);

        public static IBootstrapper AddIsolatedPipeline(
            this IBootstrapper bootstrapper,
            string name,
            string readFilesPattern,
            bool writeFiles,
            IEnumerable<IModule> processModules = null,
            IEnumerable<IModule> transformModules = null) =>
            AddPipeline(bootstrapper, null, true, name, readFilesPattern, writeFiles, processModules, transformModules);

        public static IBootstrapper AddIsolatedPipeline(
            this IBootstrapper bootstrapper,
            string name,
            string readFilesPattern,
            string destinationExtension,
            IEnumerable<IModule> processModules = null,
            IEnumerable<IModule> transformModules = null) =>
            AddPipeline(bootstrapper, null, true, name, readFilesPattern, destinationExtension, processModules, transformModules);

        public static IBootstrapper AddIsolatedPipeline(
            this IBootstrapper bootstrapper,
            string name,
            string readFilesPattern,
            DocumentConfig<FilePath> destinationPath,
            IEnumerable<IModule> processModules = null,
            IEnumerable<IModule> transformModules = null) =>
            AddPipeline(bootstrapper, null, true, name, readFilesPattern, destinationPath, processModules, transformModules);

        public static IBootstrapper AddIsolatedPipeline(
            this IBootstrapper bootstrapper,
            string name,
            params IModule[] processModules) =>
            AddPipeline(bootstrapper, null, true, name, (IEnumerable<IModule>)null, processModules, null, null);

        public static IBootstrapper AddIsolatedPipeline(
            this IBootstrapper bootstrapper,
            string name,
            string readFilesPattern,
            bool writeFiles,
            params IModule[] processModules) =>
            AddPipeline(bootstrapper, null, true, name, readFilesPattern, writeFiles, processModules, null);

        public static IBootstrapper AddIsolatedPipeline(
            this IBootstrapper bootstrapper,
            string name,
            string readFilesPattern,
            string destinationExtension,
            params IModule[] processModules) =>
            AddPipeline(bootstrapper, null, true, name, readFilesPattern, destinationExtension, processModules, null);

        public static IBootstrapper AddIsolatedPipeline(
            this IBootstrapper bootstrapper,
            string name,
            string readFilesPattern,
            DocumentConfig<FilePath> destinationPath,
            params IModule[] processModules) =>
            AddPipeline(bootstrapper, null, true, name, readFilesPattern, destinationPath, processModules, null);

        // Helpers for adding pipelines from modules

        private static IBootstrapper AddPipeline(
            IBootstrapper bootstrapper,
            IEnumerable<string> dependencies,
            bool isolated,
            string name,
            IEnumerable<IModule> inputModules,
            IEnumerable<IModule> processModules,
            IEnumerable<IModule> transformModules,
            IEnumerable<IModule> outputModules) =>
            bootstrapper.BuildPipeline(name, builder =>
            {
                builder
                    .WithDependencies(dependencies, isolated)
                    .WithInputModules(inputModules)
                    .WithProcessModules(processModules)
                    .WithTransformModules(transformModules)
                    .WithOutputModules(outputModules);
            });

        private static IBootstrapper AddPipeline(
            IBootstrapper bootstrapper,
            IEnumerable<string> dependencies,
            bool isolated,
            string name,
            string readFilesPattern,
            IEnumerable<IModule> processModules,
            IEnumerable<IModule> transformModules,
            IEnumerable<IModule> outputModules) =>
            bootstrapper.BuildPipeline(name, builder =>
            {
                builder
                    .WithDependencies(dependencies, isolated)
                    .WithProcessModules(processModules)
                    .WithTransformModules(transformModules)
                    .WithOutputModules(outputModules);
                if (readFilesPattern != null)
                {
                    builder.WithInputReadFiles(readFilesPattern);
                }
            });

        private static IBootstrapper AddPipeline(
            IBootstrapper bootstrapper,
            IEnumerable<string> dependencies,
            bool isolated,
            string name,
            string readFilesPattern,
            bool writeFiles,
            IEnumerable<IModule> processModules,
            IEnumerable<IModule> transformModules) =>
            bootstrapper.BuildPipeline(name, builder =>
            {
                builder
                    .WithDependencies(dependencies, isolated)
                    .WithProcessModules(processModules)
                    .WithTransformModules(transformModules);
                if (readFilesPattern != null)
                {
                    builder.WithInputReadFiles(readFilesPattern);
                }
                if (writeFiles)
                {
                    builder.WithOutputWriteFiles();
                }
            });

        private static IBootstrapper AddPipeline(
            IBootstrapper bootstrapper,
            IEnumerable<string> dependencies,
            bool isolated,
            string name,
            string readFilesPattern,
            string destinationExtension,
            IEnumerable<IModule> processModules,
            IEnumerable<IModule> transformModules) =>
            bootstrapper.BuildPipeline(name, builder =>
            {
                builder
                    .WithDependencies(dependencies, isolated)
                    .WithProcessModules(processModules)
                    .WithTransformModules(transformModules)
                    .WithOutputWriteFiles(destinationExtension);
                if (readFilesPattern != null)
                {
                    builder.WithInputReadFiles(readFilesPattern);
                }
            });

        private static IBootstrapper AddPipeline(
            IBootstrapper bootstrapper,
            IEnumerable<string> dependencies,
            bool isolated,
            string name,
            string readFilesPattern,
            DocumentConfig<FilePath> destinationPath,
            IEnumerable<IModule> processModules,
            IEnumerable<IModule> transformModules) =>
            bootstrapper.BuildPipeline(name, builder =>
            {
                builder
                    .WithDependencies(dependencies, isolated)
                    .WithProcessModules(processModules)
                    .WithTransformModules(transformModules)
                    .WithOutputWriteFiles(destinationPath);
                if (readFilesPattern != null)
                {
                    builder.WithInputReadFiles(readFilesPattern);
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
