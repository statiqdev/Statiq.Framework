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
        // Directly

        public static Bootstrapper AddPipelines(
            this Bootstrapper bootstrapper,
            Action<IPipelineCollection> action) =>
            bootstrapper.ConfigureEngine(x => action(x.Pipelines));

        public static Bootstrapper AddPipelines(
            this Bootstrapper bootstrapper,
            Action<IReadOnlyConfigurationSettings, IPipelineCollection> action) =>
            bootstrapper.ConfigureEngine(x => action(x.Settings, x.Pipelines));

        // By type

        public static Bootstrapper AddPipeline(this Bootstrapper bootstrapper, string name, IPipeline pipeline) =>
            bootstrapper.ConfigureEngine(x => x.Pipelines.Add(name, pipeline));

        public static Bootstrapper AddPipeline(this Bootstrapper bootstrapper, IPipeline pipeline) =>
            bootstrapper.ConfigureEngine(x => x.Pipelines.Add(pipeline));

        public static Bootstrapper AddPipeline(this Bootstrapper bootstrapper, string name, Func<IReadOnlyConfigurationSettings, IPipeline> pipelineFunc) =>
            bootstrapper.ConfigureEngine(x => x.Pipelines.Add(name, pipelineFunc(x.Settings)));

        public static Bootstrapper AddPipeline(this Bootstrapper bootstrapper, Func<IReadOnlyConfigurationSettings, IPipeline> pipelineFunc) =>
            bootstrapper.ConfigureEngine(x => x.Pipelines.Add(pipelineFunc(x.Settings)));

        public static Bootstrapper AddPipeline(this Bootstrapper bootstrapper, Type pipelineType)
        {
            _ = pipelineType ?? throw new ArgumentNullException(nameof(pipelineType));
            if (!typeof(IPipeline).IsAssignableFrom(pipelineType))
            {
                throw new ArgumentException("Provided type is not a pipeline");
            }
            return bootstrapper.ConfigureServices(x => x.AddSingleton(typeof(IPipeline), pipelineType));
        }

        public static Bootstrapper AddPipeline<TPipeline>(this Bootstrapper bootstrapper)
            where TPipeline : IPipeline =>
            bootstrapper.ConfigureServices(x => x.AddSingleton(typeof(IPipeline), typeof(TPipeline)));

        public static Bootstrapper AddPipelines(this Bootstrapper bootstrapper, Assembly assembly)
        {
            _ = assembly ?? throw new ArgumentNullException(nameof(assembly));
            return bootstrapper.ConfigureServices(x =>
            {
                foreach (Type pipelineType in bootstrapper.ClassCatalog.GetTypesAssignableTo<IPipeline>().Where(x => x.Assembly.Equals(assembly)))
                {
                    x.AddSingleton(typeof(IPipeline), pipelineType);
                }
            });
        }

        public static Bootstrapper AddPipelines(this Bootstrapper bootstrapper) => bootstrapper.AddPipelines(Assembly.GetEntryAssembly());

        public static Bootstrapper AddPipelines<TParent>(this Bootstrapper bootstrapper) =>
            bootstrapper.ConfigureServices(x =>
            {
                foreach (Type pipelineType in typeof(TParent).GetNestedTypes().Where(t => typeof(IPipeline).IsAssignableFrom(t)))
                {
                    x.AddSingleton(typeof(IPipeline), pipelineType);
                }
            });

        // Builder

        public static Bootstrapper BuildPipeline(this Bootstrapper bootstrapper, string name, Action<PipelineBuilder> buildAction) =>
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

        public static Bootstrapper AddPipeline(this Bootstrapper bootstrapper, string name, IEnumerable<IModule> processModules) =>
            bootstrapper.AddPipeline(Array.Empty<string>(), true, name, (IEnumerable<IModule>)null, processModules, null, null);

        public static Bootstrapper AddPipeline(
            this Bootstrapper bootstrapper,
            string name,
            IEnumerable<IModule> inputModules = null,
            IEnumerable<IModule> processModules = null,
            IEnumerable<IModule> transformModules = null,
            IEnumerable<IModule> outputModules = null) =>
            bootstrapper.AddPipeline(Array.Empty<string>(), true, name, inputModules, processModules, transformModules, outputModules);

        public static Bootstrapper AddPipeline(
            this Bootstrapper bootstrapper,
            string name,
            string readFilesPattern,
            IEnumerable<IModule> processModules = null,
            IEnumerable<IModule> transformModules = null,
            IEnumerable<IModule> outputModules = null) =>
            bootstrapper.AddPipeline(Array.Empty<string>(), true, name, readFilesPattern, processModules, transformModules, outputModules);

        public static Bootstrapper AddPipeline(
            this Bootstrapper bootstrapper,
            string name,
            string readFilesPattern,
            bool writeFiles,
            IEnumerable<IModule> processModules = null,
            IEnumerable<IModule> transformModules = null) =>
            bootstrapper.AddPipeline(Array.Empty<string>(), true, name, readFilesPattern, writeFiles, processModules, transformModules);

        public static Bootstrapper AddPipeline(
            this Bootstrapper bootstrapper,
            string name,
            string readFilesPattern,
            string destinationExtension,
            IEnumerable<IModule> processModules = null,
            IEnumerable<IModule> transformModules = null) =>
            bootstrapper.AddPipeline(Array.Empty<string>(), true, name, readFilesPattern, destinationExtension, processModules, transformModules);

        public static Bootstrapper AddPipeline(
            this Bootstrapper bootstrapper,
            string name,
            string readFilesPattern,
            Config<FilePath> destinationPath,
            IEnumerable<IModule> processModules = null,
            IEnumerable<IModule> transformModules = null) =>
            bootstrapper.AddPipeline(Array.Empty<string>(), true, name, readFilesPattern, destinationPath, processModules, transformModules);

        public static Bootstrapper AddPipeline(
            this Bootstrapper bootstrapper,
            string name,
            params IModule[] processModules) =>
            bootstrapper.AddPipeline(Array.Empty<string>(), true, name, (IEnumerable<IModule>)null, processModules, null, null);

        public static Bootstrapper AddPipeline(
            this Bootstrapper bootstrapper,
            string name,
            string readFilesPattern,
            bool writeFiles,
            params IModule[] processModules) =>
            bootstrapper.AddPipeline(Array.Empty<string>(), false, name, readFilesPattern, writeFiles, processModules, null);

        public static Bootstrapper AddPipeline(
            this Bootstrapper bootstrapper,
            string name,
            string readFilesPattern,
            string destinationExtension,
            params IModule[] processModules) =>
            bootstrapper.AddPipeline(Array.Empty<string>(), false, name, readFilesPattern, destinationExtension, processModules, null);

        public static Bootstrapper AddPipeline(
            this Bootstrapper bootstrapper,
            string name,
            string readFilesPattern,
            Config<FilePath> destinationPath,
            params IModule[] processModules) =>
            bootstrapper.AddPipeline(Array.Empty<string>(), false, name, readFilesPattern, destinationPath, processModules, null);

        // With dependencies

        public static Bootstrapper AddPipeline(
            this Bootstrapper bootstrapper,
            string name,
            IEnumerable<string> dependencies,
            IEnumerable<IModule> processModules) =>
            bootstrapper.AddPipeline(dependencies, true, name, (IEnumerable<IModule>)null, processModules, null, null);

        public static Bootstrapper AddPipeline(
            this Bootstrapper bootstrapper,
            string name,
            IEnumerable<string> dependencies,
            IEnumerable<IModule> inputModules = null,
            IEnumerable<IModule> processModules = null,
            IEnumerable<IModule> transformModules = null,
            IEnumerable<IModule> outputModules = null) =>
            bootstrapper.AddPipeline(dependencies, true, name, inputModules, processModules, transformModules, outputModules);

        public static Bootstrapper AddPipeline(
            this Bootstrapper bootstrapper,
            string name,
            IEnumerable<string> dependencies,
            string readFilesPattern,
            IEnumerable<IModule> processModules = null,
            IEnumerable<IModule> transformModules = null,
            IEnumerable<IModule> outputModules = null) =>
            bootstrapper.AddPipeline(dependencies, true, name, readFilesPattern, processModules, transformModules, outputModules);

        public static Bootstrapper AddPipeline(
            this Bootstrapper bootstrapper,
            string name,
            IEnumerable<string> dependencies,
            string readFilesPattern,
            bool writeFiles,
            IEnumerable<IModule> processModules = null,
            IEnumerable<IModule> transformModules = null) =>
            bootstrapper.AddPipeline(dependencies, true, name, readFilesPattern, writeFiles, processModules, transformModules);

        public static Bootstrapper AddPipeline(
            this Bootstrapper bootstrapper,
            string name,
            IEnumerable<string> dependencies,
            string readFilesPattern,
            string destinationExtension,
            IEnumerable<IModule> processModules = null,
            IEnumerable<IModule> transformModules = null) =>
            bootstrapper.AddPipeline(dependencies, true, name, readFilesPattern, destinationExtension, processModules, transformModules);

        public static Bootstrapper AddPipeline(
            this Bootstrapper bootstrapper,
            string name,
            IEnumerable<string> dependencies,
            string readFilesPattern,
            Config<FilePath> destinationPath,
            IEnumerable<IModule> processModules = null,
            IEnumerable<IModule> transformModules = null) =>
            bootstrapper.AddPipeline(dependencies, true, name, readFilesPattern, destinationPath, processModules, transformModules);

        public static Bootstrapper AddPipeline(
            this Bootstrapper bootstrapper,
            string name,
            IEnumerable<string> dependencies,
            params IModule[] processModules) =>
            bootstrapper.AddPipeline(dependencies, true, name, (IEnumerable<IModule>)null, processModules, null, null);

        public static Bootstrapper AddPipeline(
            this Bootstrapper bootstrapper,
            string name,
            IEnumerable<string> dependencies,
            string readFilesPattern,
            bool writeFiles,
            params IModule[] processModules) =>
            bootstrapper.AddPipeline(dependencies, false, name, readFilesPattern, writeFiles, processModules, null);

        public static Bootstrapper AddPipeline(
            this Bootstrapper bootstrapper,
            string name,
            IEnumerable<string> dependencies,
            string readFilesPattern,
            string destinationExtension,
            params IModule[] processModules) =>
            bootstrapper.AddPipeline(dependencies, false, name, readFilesPattern, destinationExtension, processModules, null);

        public static Bootstrapper AddPipeline(
            this Bootstrapper bootstrapper,
            string name,
            IEnumerable<string> dependencies,
            string readFilesPattern,
            Config<FilePath> destinationPath,
            params IModule[] processModules) =>
            bootstrapper.AddPipeline(dependencies, false, name, readFilesPattern, destinationPath, processModules, null);

        // Serial

        public static Bootstrapper AddSerialPipeline(
            this Bootstrapper bootstrapper,
            string name,
            IEnumerable<IModule> processModules) =>
            bootstrapper.AddPipeline(null, false, name, (IEnumerable<IModule>)null, processModules, null, null);

        public static Bootstrapper AddSerialPipeline(
            this Bootstrapper bootstrapper,
            string name,
            IEnumerable<IModule> inputModules = null,
            IEnumerable<IModule> processModules = null,
            IEnumerable<IModule> transformModules = null,
            IEnumerable<IModule> outputModules = null) =>
            bootstrapper.AddPipeline(null, false, name, inputModules, processModules, transformModules, outputModules);

        public static Bootstrapper AddSerialPipeline(
            this Bootstrapper bootstrapper,
            string name,
            string readFilesPattern,
            IEnumerable<IModule> processModules = null,
            IEnumerable<IModule> transformModules = null,
            IEnumerable<IModule> outputModules = null) =>
            bootstrapper.AddPipeline(null, false, name, readFilesPattern, processModules, transformModules, outputModules);

        public static Bootstrapper AddSerialPipeline(
            this Bootstrapper bootstrapper,
            string name,
            string readFilesPattern,
            bool writeFiles,
            IEnumerable<IModule> processModules = null,
            IEnumerable<IModule> transformModules = null) =>
            bootstrapper.AddPipeline(null, false, name, readFilesPattern, writeFiles, processModules, transformModules);

        public static Bootstrapper AddSerialPipeline(
            this Bootstrapper bootstrapper,
            string name,
            string readFilesPattern,
            string destinationExtension,
            IEnumerable<IModule> processModules = null,
            IEnumerable<IModule> transformModules = null) =>
            bootstrapper.AddPipeline(null, false, name, readFilesPattern, destinationExtension, processModules, transformModules);

        public static Bootstrapper AddSerialPipeline(
            this Bootstrapper bootstrapper,
            string name,
            string readFilesPattern,
            Config<FilePath> destinationPath,
            IEnumerable<IModule> processModules = null,
            IEnumerable<IModule> transformModules = null) =>
            bootstrapper.AddPipeline(null, false, name, readFilesPattern, destinationPath, processModules, transformModules);

        public static Bootstrapper AddSerialPipeline(
            this Bootstrapper bootstrapper,
            string name,
            params IModule[] processModules) =>
            bootstrapper.AddPipeline(null, false, name, (IEnumerable<IModule>)null, processModules, null, null);

        public static Bootstrapper AddSerialPipeline(
            this Bootstrapper bootstrapper,
            string name,
            string readFilesPattern,
            bool writeFiles,
            params IModule[] processModules) =>
            bootstrapper.AddPipeline(null, false, name, readFilesPattern, writeFiles, processModules, null);

        public static Bootstrapper AddSerialPipeline(
            this Bootstrapper bootstrapper,
            string name,
            string readFilesPattern,
            string destinationExtension,
            params IModule[] processModules) =>
            bootstrapper.AddPipeline(null, false, name, readFilesPattern, destinationExtension, processModules, null);

        public static Bootstrapper AddSerialPipeline(
            this Bootstrapper bootstrapper,
            string name,
            string readFilesPattern,
            Config<FilePath> destinationPath,
            params IModule[] processModules) =>
            bootstrapper.AddPipeline(null, false, name, readFilesPattern, destinationPath, processModules, null);

        // Isolated

        public static Bootstrapper AddIsolatedPipeline(
            this Bootstrapper bootstrapper,
            string name,
            IEnumerable<IModule> processModules) =>
            bootstrapper.AddPipeline(null, true, name, (IEnumerable<IModule>)null, processModules, null, null);

        public static Bootstrapper AddIsolatedPipeline(
            this Bootstrapper bootstrapper,
            string name,
            IEnumerable<IModule> inputModules = null,
            IEnumerable<IModule> processModules = null,
            IEnumerable<IModule> transformModules = null,
            IEnumerable<IModule> outputModules = null) =>
            bootstrapper.AddPipeline(null, true, name, inputModules, processModules, transformModules, outputModules);

        public static Bootstrapper AddIsolatedPipeline(
            this Bootstrapper bootstrapper,
            string name,
            string readFilesPattern,
            IEnumerable<IModule> processModules = null,
            IEnumerable<IModule> transformModules = null,
            IEnumerable<IModule> outputModules = null) =>
            bootstrapper.AddPipeline(null, true, name, readFilesPattern, processModules, transformModules, outputModules);

        public static Bootstrapper AddIsolatedPipeline(
            this Bootstrapper bootstrapper,
            string name,
            string readFilesPattern,
            bool writeFiles,
            IEnumerable<IModule> processModules = null,
            IEnumerable<IModule> transformModules = null) =>
            bootstrapper.AddPipeline(null, true, name, readFilesPattern, writeFiles, processModules, transformModules);

        public static Bootstrapper AddIsolatedPipeline(
            this Bootstrapper bootstrapper,
            string name,
            string readFilesPattern,
            string destinationExtension,
            IEnumerable<IModule> processModules = null,
            IEnumerable<IModule> transformModules = null) =>
            bootstrapper.AddPipeline(null, true, name, readFilesPattern, destinationExtension, processModules, transformModules);

        public static Bootstrapper AddIsolatedPipeline(
            this Bootstrapper bootstrapper,
            string name,
            string readFilesPattern,
            Config<FilePath> destinationPath,
            IEnumerable<IModule> processModules = null,
            IEnumerable<IModule> transformModules = null) =>
            bootstrapper.AddPipeline(null, true, name, readFilesPattern, destinationPath, processModules, transformModules);

        public static Bootstrapper AddIsolatedPipeline(
            this Bootstrapper bootstrapper,
            string name,
            params IModule[] processModules) =>
            bootstrapper.AddPipeline(null, true, name, (IEnumerable<IModule>)null, processModules, null, null);

        public static Bootstrapper AddIsolatedPipeline(
            this Bootstrapper bootstrapper,
            string name,
            string readFilesPattern,
            bool writeFiles,
            params IModule[] processModules) =>
            bootstrapper.AddPipeline(null, true, name, readFilesPattern, writeFiles, processModules, null);

        public static Bootstrapper AddIsolatedPipeline(
            this Bootstrapper bootstrapper,
            string name,
            string readFilesPattern,
            string destinationExtension,
            params IModule[] processModules) =>
            bootstrapper.AddPipeline(null, true, name, readFilesPattern, destinationExtension, processModules, null);

        public static Bootstrapper AddIsolatedPipeline(
            this Bootstrapper bootstrapper,
            string name,
            string readFilesPattern,
            Config<FilePath> destinationPath,
            params IModule[] processModules) =>
            bootstrapper.AddPipeline(null, true, name, readFilesPattern, destinationPath, processModules, null);

        // Helpers for adding pipelines from modules

        private static Bootstrapper AddPipeline(
            this Bootstrapper bootstrapper,
            IEnumerable<string> dependencies,
            bool isolated,
            string name,
            IEnumerable<IModule> inputModules,
            IEnumerable<IModule> processModules,
            IEnumerable<IModule> transformModules,
            IEnumerable<IModule> outputModules) =>
            bootstrapper.BuildPipeline(name, builder =>
            {
                ConfigureBuilder(builder, dependencies, isolated)
                    .WithInputModules(inputModules)
                    .WithProcessModules(processModules)
                    .WithTransformModules(transformModules)
                    .WithOutputModules(outputModules);
            });

        private static Bootstrapper AddPipeline(
            this Bootstrapper bootstrapper,
            IEnumerable<string> dependencies,
            bool isolated,
            string name,
            string readFilesPattern,
            IEnumerable<IModule> processModules,
            IEnumerable<IModule> transformModules,
            IEnumerable<IModule> outputModules) =>
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

        private static Bootstrapper AddPipeline(
            this Bootstrapper bootstrapper,
            IEnumerable<string> dependencies,
            bool isolated,
            string name,
            string readFilesPattern,
            bool writeFiles,
            IEnumerable<IModule> processModules,
            IEnumerable<IModule> transformModules) =>
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

        private static Bootstrapper AddPipeline(
            this Bootstrapper bootstrapper,
            IEnumerable<string> dependencies,
            bool isolated,
            string name,
            string readFilesPattern,
            string destinationExtension,
            IEnumerable<IModule> processModules,
            IEnumerable<IModule> transformModules) =>
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

        private static Bootstrapper AddPipeline(
            this Bootstrapper bootstrapper,
            IEnumerable<string> dependencies,
            bool isolated,
            string name,
            string readFilesPattern,
            Config<FilePath> destinationPath,
            IEnumerable<IModule> processModules,
            IEnumerable<IModule> transformModules) =>
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
