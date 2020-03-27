using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Statiq.Common;

namespace Statiq.App
{
    public static class BootstrapperPipelineExtensions
    {
        public static Bootstrapper AddPipeline<TPipeline>(this Bootstrapper bootstrapper)
            where TPipeline : IPipeline =>
            bootstrapper.ConfigureServices(x => x.AddSingleton(typeof(IPipeline), typeof(TPipeline)));

        public static Bootstrapper AddPipelines<TParent>(this Bootstrapper bootstrapper) =>
            bootstrapper.AddPipelines(typeof(TParent));

        // Builder

        public static TBootstrapper BuildPipeline<TBootstrapper>(this TBootstrapper bootstrapper, string name, Action<PipelineBuilder> buildAction)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.ConfigureEngine(x =>
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

        public static TBootstrapper AddPipeline<TBootstrapper>(this TBootstrapper bootstrapper, string name, IEnumerable<IModule> processModules)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.AddPipeline(Array.Empty<string>(), true, name, (IEnumerable<IModule>)null, processModules, null, null);

        public static TBootstrapper AddPipeline<TBootstrapper>(
            this TBootstrapper bootstrapper,
            string name,
            IEnumerable<IModule> inputModules = null,
            IEnumerable<IModule> processModules = null,
            IEnumerable<IModule> transformModules = null,
            IEnumerable<IModule> outputModules = null)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.AddPipeline(Array.Empty<string>(), true, name, inputModules, processModules, transformModules, outputModules);

        public static TBootstrapper AddPipeline<TBootstrapper>(
            this TBootstrapper bootstrapper,
            string name,
            string readFilesPattern,
            IEnumerable<IModule> processModules = null,
            IEnumerable<IModule> transformModules = null,
            IEnumerable<IModule> outputModules = null)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.AddPipeline(Array.Empty<string>(), true, name, readFilesPattern, processModules, transformModules, outputModules);

        public static TBootstrapper AddPipeline<TBootstrapper>(
            this TBootstrapper bootstrapper,
            string name,
            string readFilesPattern,
            bool writeFiles,
            IEnumerable<IModule> processModules = null,
            IEnumerable<IModule> transformModules = null)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.AddPipeline(Array.Empty<string>(), true, name, readFilesPattern, writeFiles, processModules, transformModules);

        public static TBootstrapper AddPipeline<TBootstrapper>(
            this TBootstrapper bootstrapper,
            string name,
            string readFilesPattern,
            string destinationExtension,
            IEnumerable<IModule> processModules = null,
            IEnumerable<IModule> transformModules = null)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.AddPipeline(Array.Empty<string>(), true, name, readFilesPattern, destinationExtension, processModules, transformModules);

        public static TBootstrapper AddPipeline<TBootstrapper>(
            this TBootstrapper bootstrapper,
            string name,
            string readFilesPattern,
            Config<NormalizedPath> destinationPath,
            IEnumerable<IModule> processModules = null,
            IEnumerable<IModule> transformModules = null)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.AddPipeline(Array.Empty<string>(), true, name, readFilesPattern, destinationPath, processModules, transformModules);

        public static TBootstrapper AddPipeline<TBootstrapper>(
            this TBootstrapper bootstrapper,
            string name,
            params IModule[] processModules)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.AddPipeline(Array.Empty<string>(), true, name, (IEnumerable<IModule>)null, processModules, null, null);

        public static TBootstrapper AddPipeline<TBootstrapper>(
            this TBootstrapper bootstrapper,
            string name,
            string readFilesPattern,
            bool writeFiles,
            params IModule[] processModules)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.AddPipeline(Array.Empty<string>(), false, name, readFilesPattern, writeFiles, processModules, null);

        public static TBootstrapper AddPipeline<TBootstrapper>(
            this TBootstrapper bootstrapper,
            string name,
            string readFilesPattern,
            string destinationExtension,
            params IModule[] processModules)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.AddPipeline(Array.Empty<string>(), false, name, readFilesPattern, destinationExtension, processModules, null);

        public static TBootstrapper AddPipeline<TBootstrapper>(
            this TBootstrapper bootstrapper,
            string name,
            string readFilesPattern,
            Config<NormalizedPath> destinationPath,
            params IModule[] processModules)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.AddPipeline(Array.Empty<string>(), false, name, readFilesPattern, destinationPath, processModules, null);

        // With dependencies

        public static TBootstrapper AddPipeline<TBootstrapper>(
            this TBootstrapper bootstrapper,
            string name,
            IEnumerable<string> dependencies,
            IEnumerable<IModule> processModules)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.AddPipeline(dependencies, true, name, (IEnumerable<IModule>)null, processModules, null, null);

        public static TBootstrapper AddPipeline<TBootstrapper>(
            this TBootstrapper bootstrapper,
            string name,
            IEnumerable<string> dependencies,
            IEnumerable<IModule> inputModules = null,
            IEnumerable<IModule> processModules = null,
            IEnumerable<IModule> transformModules = null,
            IEnumerable<IModule> outputModules = null)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.AddPipeline(dependencies, true, name, inputModules, processModules, transformModules, outputModules);

        public static TBootstrapper AddPipeline<TBootstrapper>(
            this TBootstrapper bootstrapper,
            string name,
            IEnumerable<string> dependencies,
            string readFilesPattern,
            IEnumerable<IModule> processModules = null,
            IEnumerable<IModule> transformModules = null,
            IEnumerable<IModule> outputModules = null)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.AddPipeline(dependencies, true, name, readFilesPattern, processModules, transformModules, outputModules);

        public static TBootstrapper AddPipeline<TBootstrapper>(
            this TBootstrapper bootstrapper,
            string name,
            IEnumerable<string> dependencies,
            string readFilesPattern,
            bool writeFiles,
            IEnumerable<IModule> processModules = null,
            IEnumerable<IModule> transformModules = null)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.AddPipeline(dependencies, true, name, readFilesPattern, writeFiles, processModules, transformModules);

        public static TBootstrapper AddPipeline<TBootstrapper>(
            this TBootstrapper bootstrapper,
            string name,
            IEnumerable<string> dependencies,
            string readFilesPattern,
            string destinationExtension,
            IEnumerable<IModule> processModules = null,
            IEnumerable<IModule> transformModules = null)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.AddPipeline(dependencies, true, name, readFilesPattern, destinationExtension, processModules, transformModules);

        public static TBootstrapper AddPipeline<TBootstrapper>(
            this TBootstrapper bootstrapper,
            string name,
            IEnumerable<string> dependencies,
            string readFilesPattern,
            Config<NormalizedPath> destinationPath,
            IEnumerable<IModule> processModules = null,
            IEnumerable<IModule> transformModules = null)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.AddPipeline(dependencies, true, name, readFilesPattern, destinationPath, processModules, transformModules);

        public static TBootstrapper AddPipeline<TBootstrapper>(
            this TBootstrapper bootstrapper,
            string name,
            IEnumerable<string> dependencies,
            params IModule[] processModules)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.AddPipeline(dependencies, true, name, (IEnumerable<IModule>)null, processModules, null, null);

        public static TBootstrapper AddPipeline<TBootstrapper>(
            this TBootstrapper bootstrapper,
            string name,
            IEnumerable<string> dependencies,
            string readFilesPattern,
            bool writeFiles,
            params IModule[] processModules)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.AddPipeline(dependencies, false, name, readFilesPattern, writeFiles, processModules, null);

        public static TBootstrapper AddPipeline<TBootstrapper>(
            this TBootstrapper bootstrapper,
            string name,
            IEnumerable<string> dependencies,
            string readFilesPattern,
            string destinationExtension,
            params IModule[] processModules)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.AddPipeline(dependencies, false, name, readFilesPattern, destinationExtension, processModules, null);

        public static TBootstrapper AddPipeline<TBootstrapper>(
            this TBootstrapper bootstrapper,
            string name,
            IEnumerable<string> dependencies,
            string readFilesPattern,
            Config<NormalizedPath> destinationPath,
            params IModule[] processModules)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.AddPipeline(dependencies, false, name, readFilesPattern, destinationPath, processModules, null);

        // Serial

        public static TBootstrapper AddSerialPipeline<TBootstrapper>(
            this TBootstrapper bootstrapper,
            string name,
            IEnumerable<IModule> processModules)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.AddPipeline(null, false, name, (IEnumerable<IModule>)null, processModules, null, null);

        public static TBootstrapper AddSerialPipeline<TBootstrapper>(
            this TBootstrapper bootstrapper,
            string name,
            IEnumerable<IModule> inputModules = null,
            IEnumerable<IModule> processModules = null,
            IEnumerable<IModule> transformModules = null,
            IEnumerable<IModule> outputModules = null)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.AddPipeline(null, false, name, inputModules, processModules, transformModules, outputModules);

        public static TBootstrapper AddSerialPipeline<TBootstrapper>(
            this TBootstrapper bootstrapper,
            string name,
            string readFilesPattern,
            IEnumerable<IModule> processModules = null,
            IEnumerable<IModule> transformModules = null,
            IEnumerable<IModule> outputModules = null)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.AddPipeline(null, false, name, readFilesPattern, processModules, transformModules, outputModules);

        public static TBootstrapper AddSerialPipeline<TBootstrapper>(
            this TBootstrapper bootstrapper,
            string name,
            string readFilesPattern,
            bool writeFiles,
            IEnumerable<IModule> processModules = null,
            IEnumerable<IModule> transformModules = null)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.AddPipeline(null, false, name, readFilesPattern, writeFiles, processModules, transformModules);

        public static TBootstrapper AddSerialPipeline<TBootstrapper>(
            this TBootstrapper bootstrapper,
            string name,
            string readFilesPattern,
            string destinationExtension,
            IEnumerable<IModule> processModules = null,
            IEnumerable<IModule> transformModules = null)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.AddPipeline(null, false, name, readFilesPattern, destinationExtension, processModules, transformModules);

        public static TBootstrapper AddSerialPipeline<TBootstrapper>(
            this TBootstrapper bootstrapper,
            string name,
            string readFilesPattern,
            Config<NormalizedPath> destinationPath,
            IEnumerable<IModule> processModules = null,
            IEnumerable<IModule> transformModules = null)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.AddPipeline(null, false, name, readFilesPattern, destinationPath, processModules, transformModules);

        public static TBootstrapper AddSerialPipeline<TBootstrapper>(
            this TBootstrapper bootstrapper,
            string name,
            params IModule[] processModules)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.AddPipeline(null, false, name, (IEnumerable<IModule>)null, processModules, null, null);

        public static TBootstrapper AddSerialPipeline<TBootstrapper>(
            this TBootstrapper bootstrapper,
            string name,
            string readFilesPattern,
            bool writeFiles,
            params IModule[] processModules)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.AddPipeline(null, false, name, readFilesPattern, writeFiles, processModules, null);

        public static TBootstrapper AddSerialPipeline<TBootstrapper>(
            this TBootstrapper bootstrapper,
            string name,
            string readFilesPattern,
            string destinationExtension,
            params IModule[] processModules)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.AddPipeline(null, false, name, readFilesPattern, destinationExtension, processModules, null);

        public static TBootstrapper AddSerialPipeline<TBootstrapper>(
            this TBootstrapper bootstrapper,
            string name,
            string readFilesPattern,
            Config<NormalizedPath> destinationPath,
            params IModule[] processModules)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.AddPipeline(null, false, name, readFilesPattern, destinationPath, processModules, null);

        // Isolated

        public static TBootstrapper AddIsolatedPipeline<TBootstrapper>(
            this TBootstrapper bootstrapper,
            string name,
            IEnumerable<IModule> processModules)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.AddPipeline(null, true, name, (IEnumerable<IModule>)null, processModules, null, null);

        public static TBootstrapper AddIsolatedPipeline<TBootstrapper>(
            this TBootstrapper bootstrapper,
            string name,
            IEnumerable<IModule> inputModules = null,
            IEnumerable<IModule> processModules = null,
            IEnumerable<IModule> transformModules = null,
            IEnumerable<IModule> outputModules = null)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.AddPipeline(null, true, name, inputModules, processModules, transformModules, outputModules);

        public static TBootstrapper AddIsolatedPipeline<TBootstrapper>(
            this TBootstrapper bootstrapper,
            string name,
            string readFilesPattern,
            IEnumerable<IModule> processModules = null,
            IEnumerable<IModule> transformModules = null,
            IEnumerable<IModule> outputModules = null)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.AddPipeline(null, true, name, readFilesPattern, processModules, transformModules, outputModules);

        public static TBootstrapper AddIsolatedPipeline<TBootstrapper>(
            this TBootstrapper bootstrapper,
            string name,
            string readFilesPattern,
            bool writeFiles,
            IEnumerable<IModule> processModules = null,
            IEnumerable<IModule> transformModules = null)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.AddPipeline(null, true, name, readFilesPattern, writeFiles, processModules, transformModules);

        public static TBootstrapper AddIsolatedPipeline<TBootstrapper>(
            this TBootstrapper bootstrapper,
            string name,
            string readFilesPattern,
            string destinationExtension,
            IEnumerable<IModule> processModules = null,
            IEnumerable<IModule> transformModules = null)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.AddPipeline(null, true, name, readFilesPattern, destinationExtension, processModules, transformModules);

        public static TBootstrapper AddIsolatedPipeline<TBootstrapper>(
            this TBootstrapper bootstrapper,
            string name,
            string readFilesPattern,
            Config<NormalizedPath> destinationPath,
            IEnumerable<IModule> processModules = null,
            IEnumerable<IModule> transformModules = null)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.AddPipeline(null, true, name, readFilesPattern, destinationPath, processModules, transformModules);

        public static TBootstrapper AddIsolatedPipeline<TBootstrapper>(
            this TBootstrapper bootstrapper,
            string name,
            params IModule[] processModules)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.AddPipeline(null, true, name, (IEnumerable<IModule>)null, processModules, null, null);

        public static TBootstrapper AddIsolatedPipeline<TBootstrapper>(
            this TBootstrapper bootstrapper,
            string name,
            string readFilesPattern,
            bool writeFiles,
            params IModule[] processModules)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.AddPipeline(null, true, name, readFilesPattern, writeFiles, processModules, null);

        public static TBootstrapper AddIsolatedPipeline<TBootstrapper>(
            this TBootstrapper bootstrapper,
            string name,
            string readFilesPattern,
            string destinationExtension,
            params IModule[] processModules)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.AddPipeline(null, true, name, readFilesPattern, destinationExtension, processModules, null);

        public static TBootstrapper AddIsolatedPipeline<TBootstrapper>(
            this TBootstrapper bootstrapper,
            string name,
            string readFilesPattern,
            Config<NormalizedPath> destinationPath,
            params IModule[] processModules)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.AddPipeline(null, true, name, readFilesPattern, destinationPath, processModules, null);

        // Helpers for adding pipelines from modules

        private static TBootstrapper AddPipeline<TBootstrapper>(
            this TBootstrapper bootstrapper,
            IEnumerable<string> dependencies,
            bool isolated,
            string name,
            IEnumerable<IModule> inputModules,
            IEnumerable<IModule> processModules,
            IEnumerable<IModule> transformModules,
            IEnumerable<IModule> outputModules)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.BuildPipeline(name, builder =>
            {
                ConfigureBuilder(builder, dependencies, isolated)
                    .WithInputModules(inputModules)
                    .WithProcessModules(processModules)
                    .WithTransformModules(transformModules)
                    .WithOutputModules(outputModules);
            });

        private static TBootstrapper AddPipeline<TBootstrapper>(
            this TBootstrapper bootstrapper,
            IEnumerable<string> dependencies,
            bool isolated,
            string name,
            string readFilesPattern,
            IEnumerable<IModule> processModules,
            IEnumerable<IModule> transformModules,
            IEnumerable<IModule> outputModules)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.BuildPipeline(name, builder =>
            {
                ConfigureBuilder(builder, dependencies, isolated)
                    .WithProcessModules(processModules)
                    .WithTransformModules(transformModules)
                    .WithOutputModules(outputModules);
                if (readFilesPattern != null)
                {
                    builder.WithInputReadFiles(readFilesPattern);
                }
            });

        private static TBootstrapper AddPipeline<TBootstrapper>(
            this TBootstrapper bootstrapper,
            IEnumerable<string> dependencies,
            bool isolated,
            string name,
            string readFilesPattern,
            bool writeFiles,
            IEnumerable<IModule> processModules,
            IEnumerable<IModule> transformModules)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.BuildPipeline(name, builder =>
            {
                ConfigureBuilder(builder, dependencies, isolated)
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

        private static TBootstrapper AddPipeline<TBootstrapper>(
            this TBootstrapper bootstrapper,
            IEnumerable<string> dependencies,
            bool isolated,
            string name,
            string readFilesPattern,
            string destinationExtension,
            IEnumerable<IModule> processModules,
            IEnumerable<IModule> transformModules)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.BuildPipeline(name, builder =>
            {
                ConfigureBuilder(builder, dependencies, isolated)
                    .WithProcessModules(processModules)
                    .WithTransformModules(transformModules)
                    .WithOutputWriteFiles(destinationExtension);
                if (readFilesPattern != null)
                {
                    builder.WithInputReadFiles(readFilesPattern);
                }
            });

        private static TBootstrapper AddPipeline<TBootstrapper>(
            this TBootstrapper bootstrapper,
            IEnumerable<string> dependencies,
            bool isolated,
            string name,
            string readFilesPattern,
            Config<NormalizedPath> destinationPath,
            IEnumerable<IModule> processModules,
            IEnumerable<IModule> transformModules)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.BuildPipeline(name, builder =>
            {
                ConfigureBuilder(builder, dependencies, isolated)
                    .WithProcessModules(processModules)
                    .WithTransformModules(transformModules)
                    .WithOutputWriteFiles(destinationPath);
                if (readFilesPattern != null)
                {
                    builder.WithInputReadFiles(readFilesPattern);
                }
            });

        private static PipelineBuilder ConfigureBuilder(PipelineBuilder builder, IEnumerable<string> dependencies, bool isolated)
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
