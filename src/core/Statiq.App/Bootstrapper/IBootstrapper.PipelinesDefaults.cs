using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Statiq.Common;

namespace Statiq.App
{
    public partial interface IBootstrapper
    {
        // Directly

        public IBootstrapper AddPipelines(
            Action<IPipelineCollection> action) =>
            ConfigureEngine(x => action(x.Pipelines));

        public IBootstrapper AddPipelines(
            Action<IReadOnlyConfigurationSettings, IPipelineCollection> action) =>
            ConfigureEngine(x => action(x.Settings, x.Pipelines));

        // By type

        public IBootstrapper AddPipeline(string name, IPipeline pipeline) =>
            ConfigureEngine(x => x.Pipelines.Add(name, pipeline));

        public IBootstrapper AddPipeline(IPipeline pipeline) =>
            ConfigureEngine(x => x.Pipelines.Add(pipeline));

        public IBootstrapper AddPipeline(string name, Func<IReadOnlyConfigurationSettings, IPipeline> pipelineFunc) =>
            ConfigureEngine(x => x.Pipelines.Add(name, pipelineFunc(x.Settings)));

        public IBootstrapper AddPipeline(Func<IReadOnlyConfigurationSettings, IPipeline> pipelineFunc) =>
            ConfigureEngine(x => x.Pipelines.Add(pipelineFunc(x.Settings)));

        public IBootstrapper AddPipeline(Type pipelineType)
        {
            _ = pipelineType ?? throw new ArgumentNullException(nameof(pipelineType));
            if (!typeof(IPipeline).IsAssignableFrom(pipelineType))
            {
                throw new ArgumentException("Provided type is not a pipeline");
            }
            return ConfigureServices(x => x.AddSingleton(typeof(IPipeline), pipelineType));
        }

        public IBootstrapper AddPipeline<TPipeline>()
            where TPipeline : IPipeline =>
            ConfigureServices(x => x.AddSingleton(typeof(IPipeline), typeof(TPipeline)));

        public IBootstrapper AddPipelines(Assembly assembly)
        {
            _ = assembly ?? throw new ArgumentNullException(nameof(assembly));
            return ConfigureServices(x =>
            {
                foreach (Type pipelineType in ClassCatalog.GetTypesAssignableTo<IPipeline>().Where(x => x.Assembly.Equals(assembly)))
                {
                    x.AddSingleton(typeof(IPipeline), pipelineType);
                }
            });
        }

        public IBootstrapper AddPipelines() => AddPipelines(Assembly.GetEntryAssembly());

        public IBootstrapper AddPipelines<TParent>() =>
            ConfigureServices(x =>
            {
                foreach (Type pipelineType in typeof(TParent).GetNestedTypes().Where(t => typeof(IPipeline).IsAssignableFrom(t)))
                {
                    x.AddSingleton(typeof(IPipeline), pipelineType);
                }
            });

        // Builder

        public IBootstrapper BuildPipeline(string name, Action<PipelineBuilder> buildAction) =>
            ConfigureEngine(x =>
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

        public IBootstrapper AddPipeline(string name, IEnumerable<IModule> processModules) =>
            AddPipeline(Array.Empty<string>(), true, name, (IEnumerable<IModule>)null, processModules, null, null);

        public IBootstrapper AddPipeline(
            string name,
            IEnumerable<IModule> inputModules = null,
            IEnumerable<IModule> processModules = null,
            IEnumerable<IModule> transformModules = null,
            IEnumerable<IModule> outputModules = null) =>
            AddPipeline(Array.Empty<string>(), true, name, inputModules, processModules, transformModules, outputModules);

        public IBootstrapper AddPipeline(
            string name,
            string readFilesPattern,
            IEnumerable<IModule> processModules = null,
            IEnumerable<IModule> transformModules = null,
            IEnumerable<IModule> outputModules = null) =>
            AddPipeline(Array.Empty<string>(), true, name, readFilesPattern, processModules, transformModules, outputModules);

        public IBootstrapper AddPipeline(
            string name,
            string readFilesPattern,
            bool writeFiles,
            IEnumerable<IModule> processModules = null,
            IEnumerable<IModule> transformModules = null) =>
            AddPipeline(Array.Empty<string>(), true, name, readFilesPattern, writeFiles, processModules, transformModules);

        public IBootstrapper AddPipeline(
            string name,
            string readFilesPattern,
            string destinationExtension,
            IEnumerable<IModule> processModules = null,
            IEnumerable<IModule> transformModules = null) =>
            AddPipeline(Array.Empty<string>(), true, name, readFilesPattern, destinationExtension, processModules, transformModules);

        public IBootstrapper AddPipeline(
            string name,
            string readFilesPattern,
            Config<FilePath> destinationPath,
            IEnumerable<IModule> processModules = null,
            IEnumerable<IModule> transformModules = null) =>
            AddPipeline(Array.Empty<string>(), true, name, readFilesPattern, destinationPath, processModules, transformModules);

        public IBootstrapper AddPipeline(
            string name,
            params IModule[] processModules) =>
            AddPipeline(Array.Empty<string>(), true, name, (IEnumerable<IModule>)null, processModules, null, null);

        public IBootstrapper AddPipeline(
            string name,
            string readFilesPattern,
            bool writeFiles,
            params IModule[] processModules) =>
            AddPipeline(Array.Empty<string>(), false, name, readFilesPattern, writeFiles, processModules, null);

        public IBootstrapper AddPipeline(
            string name,
            string readFilesPattern,
            string destinationExtension,
            params IModule[] processModules) =>
            AddPipeline(Array.Empty<string>(), false, name, readFilesPattern, destinationExtension, processModules, null);

        public IBootstrapper AddPipeline(
            string name,
            string readFilesPattern,
            Config<FilePath> destinationPath,
            params IModule[] processModules) =>
            AddPipeline(Array.Empty<string>(), false, name, readFilesPattern, destinationPath, processModules, null);

        // With dependencies

        public IBootstrapper AddPipeline(
            string name,
            IEnumerable<string> dependencies,
            IEnumerable<IModule> processModules) =>
            AddPipeline(dependencies, true, name, (IEnumerable<IModule>)null, processModules, null, null);

        public IBootstrapper AddPipeline(
            string name,
            IEnumerable<string> dependencies,
            IEnumerable<IModule> inputModules = null,
            IEnumerable<IModule> processModules = null,
            IEnumerable<IModule> transformModules = null,
            IEnumerable<IModule> outputModules = null) =>
            AddPipeline(dependencies, true, name, inputModules, processModules, transformModules, outputModules);

        public IBootstrapper AddPipeline(
            string name,
            IEnumerable<string> dependencies,
            string readFilesPattern,
            IEnumerable<IModule> processModules = null,
            IEnumerable<IModule> transformModules = null,
            IEnumerable<IModule> outputModules = null) =>
            AddPipeline(dependencies, true, name, readFilesPattern, processModules, transformModules, outputModules);

        public IBootstrapper AddPipeline(
            string name,
            IEnumerable<string> dependencies,
            string readFilesPattern,
            bool writeFiles,
            IEnumerable<IModule> processModules = null,
            IEnumerable<IModule> transformModules = null) =>
            AddPipeline(dependencies, true, name, readFilesPattern, writeFiles, processModules, transformModules);

        public IBootstrapper AddPipeline(
            string name,
            IEnumerable<string> dependencies,
            string readFilesPattern,
            string destinationExtension,
            IEnumerable<IModule> processModules = null,
            IEnumerable<IModule> transformModules = null) =>
            AddPipeline(dependencies, true, name, readFilesPattern, destinationExtension, processModules, transformModules);

        public IBootstrapper AddPipeline(
            string name,
            IEnumerable<string> dependencies,
            string readFilesPattern,
            Config<FilePath> destinationPath,
            IEnumerable<IModule> processModules = null,
            IEnumerable<IModule> transformModules = null) =>
            AddPipeline(dependencies, true, name, readFilesPattern, destinationPath, processModules, transformModules);

        public IBootstrapper AddPipeline(
            string name,
            IEnumerable<string> dependencies,
            params IModule[] processModules) =>
            AddPipeline(dependencies, true, name, (IEnumerable<IModule>)null, processModules, null, null);

        public IBootstrapper AddPipeline(
            string name,
            IEnumerable<string> dependencies,
            string readFilesPattern,
            bool writeFiles,
            params IModule[] processModules) =>
            AddPipeline(dependencies, false, name, readFilesPattern, writeFiles, processModules, null);

        public IBootstrapper AddPipeline(
            string name,
            IEnumerable<string> dependencies,
            string readFilesPattern,
            string destinationExtension,
            params IModule[] processModules) =>
            AddPipeline(dependencies, false, name, readFilesPattern, destinationExtension, processModules, null);

        public IBootstrapper AddPipeline(
            string name,
            IEnumerable<string> dependencies,
            string readFilesPattern,
            Config<FilePath> destinationPath,
            params IModule[] processModules) =>
            AddPipeline(dependencies, false, name, readFilesPattern, destinationPath, processModules, null);

        // Serial

        public IBootstrapper AddSerialPipeline(
            string name,
            IEnumerable<IModule> processModules) =>
            AddPipeline(null, false, name, (IEnumerable<IModule>)null, processModules, null, null);

        public IBootstrapper AddSerialPipeline(
            string name,
            IEnumerable<IModule> inputModules = null,
            IEnumerable<IModule> processModules = null,
            IEnumerable<IModule> transformModules = null,
            IEnumerable<IModule> outputModules = null) =>
            AddPipeline(null, false, name, inputModules, processModules, transformModules, outputModules);

        public IBootstrapper AddSerialPipeline(
            string name,
            string readFilesPattern,
            IEnumerable<IModule> processModules = null,
            IEnumerable<IModule> transformModules = null,
            IEnumerable<IModule> outputModules = null) =>
            AddPipeline(null, false, name, readFilesPattern, processModules, transformModules, outputModules);

        public IBootstrapper AddSerialPipeline(
            string name,
            string readFilesPattern,
            bool writeFiles,
            IEnumerable<IModule> processModules = null,
            IEnumerable<IModule> transformModules = null) =>
            AddPipeline(null, false, name, readFilesPattern, writeFiles, processModules, transformModules);

        public IBootstrapper AddSerialPipeline(
            string name,
            string readFilesPattern,
            string destinationExtension,
            IEnumerable<IModule> processModules = null,
            IEnumerable<IModule> transformModules = null) =>
            AddPipeline(null, false, name, readFilesPattern, destinationExtension, processModules, transformModules);

        public IBootstrapper AddSerialPipeline(
            string name,
            string readFilesPattern,
            Config<FilePath> destinationPath,
            IEnumerable<IModule> processModules = null,
            IEnumerable<IModule> transformModules = null) =>
            AddPipeline(null, false, name, readFilesPattern, destinationPath, processModules, transformModules);

        public IBootstrapper AddSerialPipeline(
            string name,
            params IModule[] processModules) =>
            AddPipeline(null, false, name, (IEnumerable<IModule>)null, processModules, null, null);

        public IBootstrapper AddSerialPipeline(
            string name,
            string readFilesPattern,
            bool writeFiles,
            params IModule[] processModules) =>
            AddPipeline(null, false, name, readFilesPattern, writeFiles, processModules, null);

        public IBootstrapper AddSerialPipeline(
            string name,
            string readFilesPattern,
            string destinationExtension,
            params IModule[] processModules) =>
            AddPipeline(null, false, name, readFilesPattern, destinationExtension, processModules, null);

        public IBootstrapper AddSerialPipeline(
            string name,
            string readFilesPattern,
            Config<FilePath> destinationPath,
            params IModule[] processModules) =>
            AddPipeline(null, false, name, readFilesPattern, destinationPath, processModules, null);

        // Isolated

        public IBootstrapper AddIsolatedPipeline(
            string name,
            IEnumerable<IModule> processModules) =>
            AddPipeline(null, true, name, (IEnumerable<IModule>)null, processModules, null, null);

        public IBootstrapper AddIsolatedPipeline(
            string name,
            IEnumerable<IModule> inputModules = null,
            IEnumerable<IModule> processModules = null,
            IEnumerable<IModule> transformModules = null,
            IEnumerable<IModule> outputModules = null) =>
            AddPipeline(null, true, name, inputModules, processModules, transformModules, outputModules);

        public IBootstrapper AddIsolatedPipeline(
            string name,
            string readFilesPattern,
            IEnumerable<IModule> processModules = null,
            IEnumerable<IModule> transformModules = null,
            IEnumerable<IModule> outputModules = null) =>
            AddPipeline(null, true, name, readFilesPattern, processModules, transformModules, outputModules);

        public IBootstrapper AddIsolatedPipeline(
            string name,
            string readFilesPattern,
            bool writeFiles,
            IEnumerable<IModule> processModules = null,
            IEnumerable<IModule> transformModules = null) =>
            AddPipeline(null, true, name, readFilesPattern, writeFiles, processModules, transformModules);

        public IBootstrapper AddIsolatedPipeline(
            string name,
            string readFilesPattern,
            string destinationExtension,
            IEnumerable<IModule> processModules = null,
            IEnumerable<IModule> transformModules = null) =>
            AddPipeline(null, true, name, readFilesPattern, destinationExtension, processModules, transformModules);

        public IBootstrapper AddIsolatedPipeline(
            string name,
            string readFilesPattern,
            Config<FilePath> destinationPath,
            IEnumerable<IModule> processModules = null,
            IEnumerable<IModule> transformModules = null) =>
            AddPipeline(null, true, name, readFilesPattern, destinationPath, processModules, transformModules);

        public IBootstrapper AddIsolatedPipeline(
            string name,
            params IModule[] processModules) =>
            AddPipeline(null, true, name, (IEnumerable<IModule>)null, processModules, null, null);

        public IBootstrapper AddIsolatedPipeline(
            string name,
            string readFilesPattern,
            bool writeFiles,
            params IModule[] processModules) =>
            AddPipeline(null, true, name, readFilesPattern, writeFiles, processModules, null);

        public IBootstrapper AddIsolatedPipeline(
            string name,
            string readFilesPattern,
            string destinationExtension,
            params IModule[] processModules) =>
            AddPipeline(null, true, name, readFilesPattern, destinationExtension, processModules, null);

        public IBootstrapper AddIsolatedPipeline(
            string name,
            string readFilesPattern,
            Config<FilePath> destinationPath,
            params IModule[] processModules) =>
            AddPipeline(null, true, name, readFilesPattern, destinationPath, processModules, null);

        // Helpers for adding pipelines from modules

        private IBootstrapper AddPipeline(
            IEnumerable<string> dependencies,
            bool isolated,
            string name,
            IEnumerable<IModule> inputModules,
            IEnumerable<IModule> processModules,
            IEnumerable<IModule> transformModules,
            IEnumerable<IModule> outputModules) =>
            BuildPipeline(name, builder =>
            {
                ConfigureBuilder(builder, dependencies, isolated)
                    .WithInputModules(inputModules)
                    .WithProcessModules(processModules)
                    .WithTransformModules(transformModules)
                    .WithOutputModules(outputModules);
            });

        private IBootstrapper AddPipeline(
            IEnumerable<string> dependencies,
            bool isolated,
            string name,
            string readFilesPattern,
            IEnumerable<IModule> processModules,
            IEnumerable<IModule> transformModules,
            IEnumerable<IModule> outputModules) =>
            BuildPipeline(name, builder =>
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

        private IBootstrapper AddPipeline(
            IEnumerable<string> dependencies,
            bool isolated,
            string name,
            string readFilesPattern,
            bool writeFiles,
            IEnumerable<IModule> processModules,
            IEnumerable<IModule> transformModules) =>
            BuildPipeline(name, builder =>
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

        private IBootstrapper AddPipeline(
            IEnumerable<string> dependencies,
            bool isolated,
            string name,
            string readFilesPattern,
            string destinationExtension,
            IEnumerable<IModule> processModules,
            IEnumerable<IModule> transformModules) =>
            BuildPipeline(name, builder =>
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

        private IBootstrapper AddPipeline(
            IEnumerable<string> dependencies,
            bool isolated,
            string name,
            string readFilesPattern,
            Config<FilePath> destinationPath,
            IEnumerable<IModule> processModules,
            IEnumerable<IModule> transformModules) =>
            BuildPipeline(name, builder =>
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
