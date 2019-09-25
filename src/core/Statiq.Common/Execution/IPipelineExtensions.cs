using System.Collections.Generic;

namespace Statiq.Common
{
    public static class IPipelineExtensions
    {
        public static TPipeline WithInputModules<TPipeline>(this TPipeline pipeline, IEnumerable<IModule> modules)
            where TPipeline : IPipeline
        {
            pipeline.InputModules.AddRange(modules);
            return pipeline;
        }

        public static TPipeline WithInputModules<TPipeline>(this TPipeline pipeline, params IModule[] modules)
            where TPipeline : IPipeline
        {
            pipeline.InputModules.Add(modules);
            return pipeline;
        }

        public static TPipeline WithProcessModules<TPipeline>(this TPipeline pipeline, IEnumerable<IModule> modules)
            where TPipeline : IPipeline
        {
            pipeline.ProcessModules.AddRange(modules);
            return pipeline;
        }

        public static TPipeline WithProcessModules<TPipeline>(this TPipeline pipeline, params IModule[] modules)
            where TPipeline : IPipeline
        {
            pipeline.ProcessModules.Add(modules);
            return pipeline;
        }

        public static TPipeline WithTransformModules<TPipeline>(this TPipeline pipeline, IEnumerable<IModule> modules)
            where TPipeline : IPipeline
        {
            pipeline.TransformModules.AddRange(modules);
            return pipeline;
        }

        public static TPipeline WithTransformModules<TPipeline>(this TPipeline pipeline, params IModule[] modules)
            where TPipeline : IPipeline
        {
            pipeline.TransformModules.Add(modules);
            return pipeline;
        }

        public static TPipeline WithOutputModules<TPipeline>(this TPipeline pipeline, IEnumerable<IModule> modules)
            where TPipeline : IPipeline
        {
            pipeline.OutputModules.AddRange(modules);
            return pipeline;
        }

        public static TPipeline WithOutputModules<TPipeline>(this TPipeline pipeline, params IModule[] modules)
            where TPipeline : IPipeline
        {
            pipeline.OutputModules.Add(modules);
            return pipeline;
        }

        public static TPipeline WithDependencies<TPipeline>(this TPipeline pipeline, params string[] dependencies)
            where TPipeline : IPipeline
        {
            pipeline.Dependencies.AddRange(dependencies);
            return pipeline;
        }

        public static TPipeline WithDependencies<TPipeline>(this TPipeline pipeline, IEnumerable<string> dependencies)
            where TPipeline : IPipeline
        {
            pipeline.Dependencies.AddRange(dependencies);
            return pipeline;
        }

        public static TPipeline AsIsolated<TPipeline>(this TPipeline pipeline, bool isolated = true)
            where TPipeline : IPipeline
        {
            pipeline.Isolated = isolated;
            return pipeline;
        }

        public static TPipeline WithTrigger<TPipeline>(this TPipeline pipeline, PipelineTrigger trigger)
            where TPipeline : IPipeline
        {
            pipeline.Trigger = trigger;
            return pipeline;
        }

        public static TPipeline WithManualTrigger<TPipeline>(this TPipeline pipeline)
            where TPipeline : IPipeline
        {
            pipeline.Trigger = PipelineTrigger.Manual;
            return pipeline;
        }

        public static TPipeline WithManualOrDependencyTrigger<TPipeline>(this TPipeline pipeline)
            where TPipeline : IPipeline
        {
            pipeline.Trigger = PipelineTrigger.ManualOrDependency;
            return pipeline;
        }

        public static TPipeline WithAlwaysTrigger<TPipeline>(this TPipeline pipeline)
            where TPipeline : IPipeline
        {
            pipeline.Trigger = PipelineTrigger.Always;
            return pipeline;
        }
    }
}
