using System.Collections.Generic;
using Wyam.Common.Modules;

namespace Wyam.Common.Execution
{
    public static class IPipelineExtensions
    {
        public static TPipeline WithReadModules<TPipeline>(this TPipeline pipeline, IEnumerable<IModule> modules)
            where TPipeline : IPipeline
        {
            pipeline.ReadModules.Add(modules);
            return pipeline;
        }

        public static TPipeline WithReadModules<TPipeline>(this TPipeline pipeline, params IModule[] modules)
            where TPipeline : IPipeline
        {
            pipeline.ReadModules.Add(modules);
            return pipeline;
        }

        public static TPipeline WithProcessModules<TPipeline>(this TPipeline pipeline, IEnumerable<IModule> modules)
            where TPipeline : IPipeline
        {
            pipeline.ProcessModules.Add(modules);
            return pipeline;
        }

        public static TPipeline WithProcessModules<TPipeline>(this TPipeline pipeline, params IModule[] modules)
            where TPipeline : IPipeline
        {
            pipeline.ProcessModules.Add(modules);
            return pipeline;
        }

        public static TPipeline WithRenderModules<TPipeline>(this TPipeline pipeline, IEnumerable<IModule> modules)
            where TPipeline : IPipeline
        {
            pipeline.RenderModules.Add(modules);
            return pipeline;
        }

        public static TPipeline WithRenderModules<TPipeline>(this TPipeline pipeline, params IModule[] modules)
            where TPipeline : IPipeline
        {
            pipeline.RenderModules.Add(modules);
            return pipeline;
        }

        public static TPipeline WithWriteModules<TPipeline>(this TPipeline pipeline, IEnumerable<IModule> modules)
            where TPipeline : IPipeline
        {
            pipeline.WriteModules.Add(modules);
            return pipeline;
        }

        public static TPipeline WithWriteModules<TPipeline>(this TPipeline pipeline, params IModule[] modules)
            where TPipeline : IPipeline
        {
            pipeline.WriteModules.Add(modules);
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

        public static TPipeline AlwaysProcess<TPipeline>(this TPipeline pipeline, bool alwaysProcess = true)
            where TPipeline : IPipeline
        {
            pipeline.AlwaysProcess = alwaysProcess;
            return pipeline;
        }
    }
}
