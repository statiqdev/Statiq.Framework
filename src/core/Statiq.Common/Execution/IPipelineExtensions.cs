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

        public static TPipeline AsDeployment<TPipeline>(this TPipeline pipeline, bool deployment = true)
            where TPipeline : IPipeline
        {
            pipeline.Deployment = deployment;
            return pipeline;
        }

        public static TPipeline WithExecutionPolicy<TPipeline>(this TPipeline pipeline, ExecutionPolicy policy)
            where TPipeline : IPipeline
        {
            pipeline.ExecutionPolicy = policy;
            return pipeline;
        }

        public static TPipeline NormallyExecute<TPipeline>(this TPipeline pipeline)
            where TPipeline : IPipeline
        {
            pipeline.ExecutionPolicy = ExecutionPolicy.Normal;
            return pipeline;
        }

        public static TPipeline ManuallyExecute<TPipeline>(this TPipeline pipeline)
            where TPipeline : IPipeline
        {
            pipeline.ExecutionPolicy = ExecutionPolicy.Manual;
            return pipeline;
        }

        public static TPipeline AlwaysExecute<TPipeline>(this TPipeline pipeline)
            where TPipeline : IPipeline
        {
            pipeline.ExecutionPolicy = ExecutionPolicy.Always;
            return pipeline;
        }
    }
}
