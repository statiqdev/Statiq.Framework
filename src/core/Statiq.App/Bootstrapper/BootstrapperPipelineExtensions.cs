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

        // Normal

        public static TBootstrapper AddPipeline<TBootstrapper>(this TBootstrapper bootstrapper, string name, IEnumerable<IModule> processModules)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.AddPipeline(null, PipelineType.Normal, name, (IEnumerable<IModule>)null, processModules, null, null);

        public static TBootstrapper AddPipeline<TBootstrapper>(
            this TBootstrapper bootstrapper,
            string name,
            IEnumerable<IModule> inputModules = null,
            IEnumerable<IModule> processModules = null,
            IEnumerable<IModule> transformModules = null,
            IEnumerable<IModule> outputModules = null)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.AddPipeline(null, PipelineType.Normal, name, inputModules, processModules, transformModules, outputModules);

        public static TBootstrapper AddPipeline<TBootstrapper>(
            this TBootstrapper bootstrapper,
            string name,
            string readFilesPattern,
            IEnumerable<IModule> processModules = null,
            IEnumerable<IModule> transformModules = null,
            IEnumerable<IModule> outputModules = null)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.AddPipeline(null, PipelineType.Normal, name, readFilesPattern, processModules, transformModules, outputModules);

        public static TBootstrapper AddPipeline<TBootstrapper>(
            this TBootstrapper bootstrapper,
            string name,
            string readFilesPattern,
            bool writeFiles,
            IEnumerable<IModule> processModules = null,
            IEnumerable<IModule> transformModules = null)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.AddPipeline(null, PipelineType.Normal, name, readFilesPattern, writeFiles, processModules, transformModules);

        public static TBootstrapper AddPipeline<TBootstrapper>(
            this TBootstrapper bootstrapper,
            string name,
            string readFilesPattern,
            string destinationExtension,
            IEnumerable<IModule> processModules = null,
            IEnumerable<IModule> transformModules = null)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.AddPipeline(null, PipelineType.Normal, name, readFilesPattern, destinationExtension, processModules, transformModules);

        public static TBootstrapper AddPipeline<TBootstrapper>(
            this TBootstrapper bootstrapper,
            string name,
            string readFilesPattern,
            Config<NormalizedPath> destinationPath,
            IEnumerable<IModule> processModules = null,
            IEnumerable<IModule> transformModules = null)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.AddPipeline(null, PipelineType.Normal, name, readFilesPattern, destinationPath, processModules, transformModules);

        public static TBootstrapper AddPipeline<TBootstrapper>(
            this TBootstrapper bootstrapper,
            string name,
            params IModule[] processModules)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.AddPipeline(null, PipelineType.Normal, name, (IEnumerable<IModule>)null, processModules, null, null);

        public static TBootstrapper AddPipeline<TBootstrapper>(
            this TBootstrapper bootstrapper,
            string name,
            string readFilesPattern,
            bool writeFiles,
            params IModule[] processModules)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.AddPipeline(null, PipelineType.Normal, name, readFilesPattern, writeFiles, processModules, null);

        public static TBootstrapper AddPipeline<TBootstrapper>(
            this TBootstrapper bootstrapper,
            string name,
            string readFilesPattern,
            string destinationExtension,
            params IModule[] processModules)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.AddPipeline(null, PipelineType.Normal, name, readFilesPattern, destinationExtension, processModules, null);

        public static TBootstrapper AddPipeline<TBootstrapper>(
            this TBootstrapper bootstrapper,
            string name,
            string readFilesPattern,
            Config<NormalizedPath> destinationPath,
            params IModule[] processModules)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.AddPipeline(null, PipelineType.Normal, name, readFilesPattern, destinationPath, processModules, null);

        public static TBootstrapper AddPipeline<TBootstrapper>(
            this TBootstrapper bootstrapper,
            string name,
            IEnumerable<string> dependencies,
            IEnumerable<IModule> processModules)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.AddPipeline(dependencies, PipelineType.Normal, name, (IEnumerable<IModule>)null, processModules, null, null);

        public static TBootstrapper AddPipeline<TBootstrapper>(
            this TBootstrapper bootstrapper,
            string name,
            IEnumerable<string> dependencies,
            IEnumerable<IModule> inputModules = null,
            IEnumerable<IModule> processModules = null,
            IEnumerable<IModule> transformModules = null,
            IEnumerable<IModule> outputModules = null)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.AddPipeline(dependencies, PipelineType.Normal, name, inputModules, processModules, transformModules, outputModules);

        public static TBootstrapper AddPipeline<TBootstrapper>(
            this TBootstrapper bootstrapper,
            string name,
            IEnumerable<string> dependencies,
            string readFilesPattern,
            IEnumerable<IModule> processModules = null,
            IEnumerable<IModule> transformModules = null,
            IEnumerable<IModule> outputModules = null)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.AddPipeline(dependencies, PipelineType.Normal, name, readFilesPattern, processModules, transformModules, outputModules);

        public static TBootstrapper AddPipeline<TBootstrapper>(
            this TBootstrapper bootstrapper,
            string name,
            IEnumerable<string> dependencies,
            string readFilesPattern,
            bool writeFiles,
            IEnumerable<IModule> processModules = null,
            IEnumerable<IModule> transformModules = null)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.AddPipeline(dependencies, PipelineType.Normal, name, readFilesPattern, writeFiles, processModules, transformModules);

        public static TBootstrapper AddPipeline<TBootstrapper>(
            this TBootstrapper bootstrapper,
            string name,
            IEnumerable<string> dependencies,
            string readFilesPattern,
            string destinationExtension,
            IEnumerable<IModule> processModules = null,
            IEnumerable<IModule> transformModules = null)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.AddPipeline(dependencies, PipelineType.Normal, name, readFilesPattern, destinationExtension, processModules, transformModules);

        public static TBootstrapper AddPipeline<TBootstrapper>(
            this TBootstrapper bootstrapper,
            string name,
            IEnumerable<string> dependencies,
            string readFilesPattern,
            Config<NormalizedPath> destinationPath,
            IEnumerable<IModule> processModules = null,
            IEnumerable<IModule> transformModules = null)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.AddPipeline(dependencies, PipelineType.Normal, name, readFilesPattern, destinationPath, processModules, transformModules);

        public static TBootstrapper AddPipeline<TBootstrapper>(
            this TBootstrapper bootstrapper,
            string name,
            IEnumerable<string> dependencies,
            params IModule[] processModules)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.AddPipeline(dependencies, PipelineType.Normal, name, (IEnumerable<IModule>)null, processModules, null, null);

        public static TBootstrapper AddPipeline<TBootstrapper>(
            this TBootstrapper bootstrapper,
            string name,
            IEnumerable<string> dependencies,
            string readFilesPattern,
            bool writeFiles,
            params IModule[] processModules)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.AddPipeline(dependencies, PipelineType.Normal, name, readFilesPattern, writeFiles, processModules, null);

        public static TBootstrapper AddPipeline<TBootstrapper>(
            this TBootstrapper bootstrapper,
            string name,
            IEnumerable<string> dependencies,
            string readFilesPattern,
            string destinationExtension,
            params IModule[] processModules)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.AddPipeline(dependencies, PipelineType.Normal, name, readFilesPattern, destinationExtension, processModules, null);

        public static TBootstrapper AddPipeline<TBootstrapper>(
            this TBootstrapper bootstrapper,
            string name,
            IEnumerable<string> dependencies,
            string readFilesPattern,
            Config<NormalizedPath> destinationPath,
            params IModule[] processModules)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.AddPipeline(dependencies, PipelineType.Normal, name, readFilesPattern, destinationPath, processModules, null);

        // Deployment

        public static TBootstrapper AddDeploymentPipeline<TBootstrapper>(this TBootstrapper bootstrapper, string name, IEnumerable<IModule> outputModules)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.AddPipeline(null, PipelineType.Deployment, name, (IEnumerable<IModule>)null, null, null, outputModules);

        public static TBootstrapper AddDeploymentPipeline<TBootstrapper>(
            this TBootstrapper bootstrapper,
            string name,
            IEnumerable<IModule> inputModules = null,
            IEnumerable<IModule> processModules = null,
            IEnumerable<IModule> transformModules = null,
            IEnumerable<IModule> outputModules = null)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.AddPipeline(null, PipelineType.Deployment, name, inputModules, processModules, transformModules, outputModules);

        public static TBootstrapper AddDeploymentPipeline<TBootstrapper>(
            this TBootstrapper bootstrapper,
            string name,
            params IModule[] outputModules)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.AddPipeline(null, PipelineType.Deployment, name, (IEnumerable<IModule>)null, null, null, outputModules);

        public static TBootstrapper AddDeploymentPipeline<TBootstrapper>(
            this TBootstrapper bootstrapper,
            string name,
            IEnumerable<string> dependencies,
            IEnumerable<IModule> outputModules)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.AddPipeline(dependencies, PipelineType.Deployment, name, (IEnumerable<IModule>)null, null, null, outputModules);

        public static TBootstrapper AddDeploymentPipeline<TBootstrapper>(
            this TBootstrapper bootstrapper,
            string name,
            IEnumerable<string> dependencies,
            IEnumerable<IModule> inputModules = null,
            IEnumerable<IModule> processModules = null,
            IEnumerable<IModule> transformModules = null,
            IEnumerable<IModule> outputModules = null)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.AddPipeline(dependencies, PipelineType.Deployment, name, inputModules, processModules, transformModules, outputModules);

        public static TBootstrapper AddDeploymentPipeline<TBootstrapper>(
            this TBootstrapper bootstrapper,
            string name,
            IEnumerable<string> dependencies,
            params IModule[] outputModules)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.AddPipeline(dependencies, PipelineType.Deployment, name, (IEnumerable<IModule>)null, null, null, outputModules);

        // Serial

        public static TBootstrapper AddSerialPipeline<TBootstrapper>(
            this TBootstrapper bootstrapper,
            string name,
            IEnumerable<IModule> processModules)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.AddPipeline(null, PipelineType.Serial, name, (IEnumerable<IModule>)null, processModules, null, null);

        public static TBootstrapper AddSerialPipeline<TBootstrapper>(
            this TBootstrapper bootstrapper,
            string name,
            IEnumerable<IModule> inputModules = null,
            IEnumerable<IModule> processModules = null,
            IEnumerable<IModule> transformModules = null,
            IEnumerable<IModule> outputModules = null)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.AddPipeline(null, PipelineType.Serial, name, inputModules, processModules, transformModules, outputModules);

        public static TBootstrapper AddSerialPipeline<TBootstrapper>(
            this TBootstrapper bootstrapper,
            string name,
            string readFilesPattern,
            IEnumerable<IModule> processModules = null,
            IEnumerable<IModule> transformModules = null,
            IEnumerable<IModule> outputModules = null)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.AddPipeline(null, PipelineType.Serial, name, readFilesPattern, processModules, transformModules, outputModules);

        public static TBootstrapper AddSerialPipeline<TBootstrapper>(
            this TBootstrapper bootstrapper,
            string name,
            string readFilesPattern,
            bool writeFiles,
            IEnumerable<IModule> processModules = null,
            IEnumerable<IModule> transformModules = null)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.AddPipeline(null, PipelineType.Serial, name, readFilesPattern, writeFiles, processModules, transformModules);

        public static TBootstrapper AddSerialPipeline<TBootstrapper>(
            this TBootstrapper bootstrapper,
            string name,
            string readFilesPattern,
            string destinationExtension,
            IEnumerable<IModule> processModules = null,
            IEnumerable<IModule> transformModules = null)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.AddPipeline(null, PipelineType.Serial, name, readFilesPattern, destinationExtension, processModules, transformModules);

        public static TBootstrapper AddSerialPipeline<TBootstrapper>(
            this TBootstrapper bootstrapper,
            string name,
            string readFilesPattern,
            Config<NormalizedPath> destinationPath,
            IEnumerable<IModule> processModules = null,
            IEnumerable<IModule> transformModules = null)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.AddPipeline(null, PipelineType.Serial, name, readFilesPattern, destinationPath, processModules, transformModules);

        public static TBootstrapper AddSerialPipeline<TBootstrapper>(
            this TBootstrapper bootstrapper,
            string name,
            params IModule[] processModules)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.AddPipeline(null, PipelineType.Serial, name, (IEnumerable<IModule>)null, processModules, null, null);

        public static TBootstrapper AddSerialPipeline<TBootstrapper>(
            this TBootstrapper bootstrapper,
            string name,
            string readFilesPattern,
            bool writeFiles,
            params IModule[] processModules)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.AddPipeline(null, PipelineType.Serial, name, readFilesPattern, writeFiles, processModules, null);

        public static TBootstrapper AddSerialPipeline<TBootstrapper>(
            this TBootstrapper bootstrapper,
            string name,
            string readFilesPattern,
            string destinationExtension,
            params IModule[] processModules)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.AddPipeline(null, PipelineType.Serial, name, readFilesPattern, destinationExtension, processModules, null);

        public static TBootstrapper AddSerialPipeline<TBootstrapper>(
            this TBootstrapper bootstrapper,
            string name,
            string readFilesPattern,
            Config<NormalizedPath> destinationPath,
            params IModule[] processModules)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.AddPipeline(null, PipelineType.Serial, name, readFilesPattern, destinationPath, processModules, null);

        // Isolated

        public static TBootstrapper AddIsolatedPipeline<TBootstrapper>(
            this TBootstrapper bootstrapper,
            string name,
            IEnumerable<IModule> processModules)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.AddPipeline(null, PipelineType.Isolated, name, (IEnumerable<IModule>)null, processModules, null, null);

        public static TBootstrapper AddIsolatedPipeline<TBootstrapper>(
            this TBootstrapper bootstrapper,
            string name,
            IEnumerable<IModule> inputModules = null,
            IEnumerable<IModule> processModules = null,
            IEnumerable<IModule> transformModules = null,
            IEnumerable<IModule> outputModules = null)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.AddPipeline(null, PipelineType.Isolated, name, inputModules, processModules, transformModules, outputModules);

        public static TBootstrapper AddIsolatedPipeline<TBootstrapper>(
            this TBootstrapper bootstrapper,
            string name,
            string readFilesPattern,
            IEnumerable<IModule> processModules = null,
            IEnumerable<IModule> transformModules = null,
            IEnumerable<IModule> outputModules = null)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.AddPipeline(null, PipelineType.Isolated, name, readFilesPattern, processModules, transformModules, outputModules);

        public static TBootstrapper AddIsolatedPipeline<TBootstrapper>(
            this TBootstrapper bootstrapper,
            string name,
            string readFilesPattern,
            bool writeFiles,
            IEnumerable<IModule> processModules = null,
            IEnumerable<IModule> transformModules = null)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.AddPipeline(null, PipelineType.Isolated, name, readFilesPattern, writeFiles, processModules, transformModules);

        public static TBootstrapper AddIsolatedPipeline<TBootstrapper>(
            this TBootstrapper bootstrapper,
            string name,
            string readFilesPattern,
            string destinationExtension,
            IEnumerable<IModule> processModules = null,
            IEnumerable<IModule> transformModules = null)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.AddPipeline(null, PipelineType.Isolated, name, readFilesPattern, destinationExtension, processModules, transformModules);

        public static TBootstrapper AddIsolatedPipeline<TBootstrapper>(
            this TBootstrapper bootstrapper,
            string name,
            string readFilesPattern,
            Config<NormalizedPath> destinationPath,
            IEnumerable<IModule> processModules = null,
            IEnumerable<IModule> transformModules = null)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.AddPipeline(null, PipelineType.Isolated, name, readFilesPattern, destinationPath, processModules, transformModules);

        public static TBootstrapper AddIsolatedPipeline<TBootstrapper>(
            this TBootstrapper bootstrapper,
            string name,
            params IModule[] processModules)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.AddPipeline(null, PipelineType.Isolated, name, (IEnumerable<IModule>)null, processModules, null, null);

        public static TBootstrapper AddIsolatedPipeline<TBootstrapper>(
            this TBootstrapper bootstrapper,
            string name,
            string readFilesPattern,
            bool writeFiles,
            params IModule[] processModules)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.AddPipeline(null, PipelineType.Isolated, name, readFilesPattern, writeFiles, processModules, null);

        public static TBootstrapper AddIsolatedPipeline<TBootstrapper>(
            this TBootstrapper bootstrapper,
            string name,
            string readFilesPattern,
            string destinationExtension,
            params IModule[] processModules)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.AddPipeline(null, PipelineType.Isolated, name, readFilesPattern, destinationExtension, processModules, null);

        public static TBootstrapper AddIsolatedPipeline<TBootstrapper>(
            this TBootstrapper bootstrapper,
            string name,
            string readFilesPattern,
            Config<NormalizedPath> destinationPath,
            params IModule[] processModules)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.AddPipeline(null, PipelineType.Isolated, name, readFilesPattern, destinationPath, processModules, null);

        // Helpers for adding pipelines from modules

        private static TBootstrapper AddPipeline<TBootstrapper>(
            this TBootstrapper bootstrapper,
            IEnumerable<string> dependencies,
            PipelineType pipelineType,
            string name,
            IEnumerable<IModule> inputModules,
            IEnumerable<IModule> processModules,
            IEnumerable<IModule> transformModules,
            IEnumerable<IModule> outputModules)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.BuildPipeline(name, builder =>
            {
                ConfigureBuilder(builder, dependencies, pipelineType)
                    .WithInputModules(inputModules)
                    .WithProcessModules(processModules)
                    .WithPostProcessModules(transformModules)
                    .WithOutputModules(outputModules);
            });

        private static TBootstrapper AddPipeline<TBootstrapper>(
            this TBootstrapper bootstrapper,
            IEnumerable<string> dependencies,
            PipelineType pipelineType,
            string name,
            string readFilesPattern,
            IEnumerable<IModule> processModules,
            IEnumerable<IModule> transformModules,
            IEnumerable<IModule> outputModules)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.BuildPipeline(name, builder =>
            {
                ConfigureBuilder(builder, dependencies, pipelineType)
                    .WithProcessModules(processModules)
                    .WithPostProcessModules(transformModules)
                    .WithOutputModules(outputModules);
                if (readFilesPattern != null)
                {
                    builder.WithInputReadFiles(readFilesPattern);
                }
            });

        private static TBootstrapper AddPipeline<TBootstrapper>(
            this TBootstrapper bootstrapper,
            IEnumerable<string> dependencies,
            PipelineType pipelineType,
            string name,
            string readFilesPattern,
            bool writeFiles,
            IEnumerable<IModule> processModules,
            IEnumerable<IModule> transformModules)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.BuildPipeline(name, builder =>
            {
                ConfigureBuilder(builder, dependencies, pipelineType)
                    .WithProcessModules(processModules)
                    .WithPostProcessModules(transformModules);
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
            PipelineType pipelineType,
            string name,
            string readFilesPattern,
            string destinationExtension,
            IEnumerable<IModule> processModules,
            IEnumerable<IModule> transformModules)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.BuildPipeline(name, builder =>
            {
                ConfigureBuilder(builder, dependencies, pipelineType)
                    .WithProcessModules(processModules)
                    .WithPostProcessModules(transformModules)
                    .WithOutputWriteFiles(destinationExtension);
                if (readFilesPattern != null)
                {
                    builder.WithInputReadFiles(readFilesPattern);
                }
            });

        private static TBootstrapper AddPipeline<TBootstrapper>(
            this TBootstrapper bootstrapper,
            IEnumerable<string> dependencies,
            PipelineType pipelineType,
            string name,
            string readFilesPattern,
            Config<NormalizedPath> destinationPath,
            IEnumerable<IModule> processModules,
            IEnumerable<IModule> transformModules)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.BuildPipeline(name, builder =>
            {
                ConfigureBuilder(builder, dependencies, pipelineType)
                    .WithProcessModules(processModules)
                    .WithPostProcessModules(transformModules)
                    .WithOutputWriteFiles(destinationPath);
                if (readFilesPattern != null)
                {
                    builder.WithInputReadFiles(readFilesPattern);
                }
            });

        private static PipelineBuilder ConfigureBuilder(PipelineBuilder builder, IEnumerable<string> dependencies, PipelineType pipelineType)
        {
            if (dependencies != null)
            {
                builder.WithDependencies(dependencies);
            }
            if (pipelineType == PipelineType.Isolated)
            {
                builder.AsIsolated();
            }
            else if (pipelineType == PipelineType.Serial)
            {
                builder.AsSerial();
            }
            else if (pipelineType == PipelineType.Deployment)
            {
                builder.AsDeployment();
            }
            return builder;
        }

        private enum PipelineType
        {
            Normal,
            Isolated,
            Serial,
            Deployment
        }
    }
}
