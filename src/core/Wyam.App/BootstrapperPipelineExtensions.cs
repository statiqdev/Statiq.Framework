using System;
using System.Collections.Generic;
using System.Text;
using Wyam.Common.Configuration;
using Wyam.Common.Execution;
using Wyam.Common.Modules;

namespace Wyam.App
{
    public static class BootstrapperPipelineExtensions
    {
        // Add with process modules

        public static IBootstrapper AddPipeline(
            this IBootstrapper bootstrapper,
            string name,
            IModuleList processModules) =>
            bootstrapper.Configure<IEngine>(x => x.Pipelines.Add(name, processModules));

        public static IBootstrapper AddPipeline(
            this IBootstrapper bootstrapper,
            string name,
            params IModule[] processModules) =>
            bootstrapper.Configure<IEngine>(x => x.Pipelines.Add(name, processModules));

        // Add sequential

        public static IBootstrapper AddSequentialPipeline(
            this IBootstrapper bootstrapper,
            string name,
            IModuleList processModules) =>
            bootstrapper.Configure<IEngine>(x => x.Pipelines.AddSequential(name, processModules));

        public static IBootstrapper AddSequentialPipeline(
            this IBootstrapper bootstrapper,
            string name,
            params IModule[] processModules) =>
            bootstrapper.Configure<IEngine>(x => x.Pipelines.AddSequential(name, processModules));

        // TODO: Sequential with more than process modules (in IPipelineCollectionExtensions too)

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

        // Add with configuration delegates

        public static IBootstrapper AddPipeline(
            this IBootstrapper bootstrapper,
            string name,
            Action<IPipeline> configureAction) =>
            bootstrapper.Configure<IEngine>(x => configureAction(x.Pipelines.Add(name)));

        public static IBootstrapper AddPipeline(
            this IBootstrapper bootstrapper,
            string name,
            Action<IPipeline, IReadOnlySettings> configureAction) =>
            bootstrapper.Configure<IEngine>(x => configureAction(x.Pipelines.Add(name), x.Settings));

        public static IBootstrapper AddPipeline(
            this IBootstrapper bootstrapper,
            Func<IReadOnlySettings, IPipeline> pipelineFunc) =>
            bootstrapper.Configure<IEngine>(x =>
            {
                IPipeline pipeline = pipelineFunc(x.Settings);
                if (pipeline != null)
                {
                    x.Pipelines.Add(pipeline);
                }
            });

        public static IBootstrapper AddPipeline(
            this IBootstrapper bootstrapper,
            string name,
            Func<IReadOnlySettings, IPipeline> pipelineFunc) =>
            bootstrapper.Configure<IEngine>(x =>
            {
                IPipeline pipeline = pipelineFunc(x.Settings);
                if (pipeline != null)
                {
                    x.Pipelines.Add(name, pipeline);
                }
            });

        public static IBootstrapper AddPipeline<TPipeline>(
            this IBootstrapper bootstrapper,
            Action<IPipeline, IReadOnlySettings> action)
            where TPipeline : IPipeline =>
            bootstrapper.Configure<IEngine>(x => action(x.Pipelines.Add<TPipeline>(), x.Settings));

        public static IBootstrapper AddPipeline<TPipeline>(
            this IBootstrapper bootstrapper,
            string name,
            Action<IPipeline, IReadOnlySettings> action)
            where TPipeline : IPipeline =>
            bootstrapper.Configure<IEngine>(x => action(x.Pipelines.Add<TPipeline>(name), x.Settings));

        // TODO: AddPipelineWithRead(), AddPipelineWithReadAndWrite() (with just read, with variations on write paths), AddPipelineWithWrite()\
        // These have to be extensions to IBootstrapper in Wyam.App and not general IPipelineCollection extensions because Wyam.Common doesn't reference Wyam.Core where modules are defined

        // AddIsolatedPipeline()
    }
}
