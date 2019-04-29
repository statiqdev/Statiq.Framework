using System;
using System.Collections.Generic;
using System.Text;
using Wyam.Common.Modules;
using Wyam.Common.Util;

namespace Wyam.Common.Execution
{
    public static class IPipelineExtensions
    {
        public static IPipeline WithRead(this IPipeline pipeline, IEnumerable<IModule> modules)
        {
            pipeline.Read.Add(modules);
            return pipeline;
        }

        public static IPipeline WithRead(this IPipeline pipeline, params IModule[] modules)
        {
            pipeline.Read.Add(modules);
            return pipeline;
        }

        public static IPipeline WithProcess(this IPipeline pipeline, IEnumerable<IModule> modules)
        {
            pipeline.Process.Add(modules);
            return pipeline;
        }

        public static IPipeline WithProcess(this IPipeline pipeline, params IModule[] modules)
        {
            pipeline.Process.Add(modules);
            return pipeline;
        }

        public static IPipeline WithRender(this IPipeline pipeline, IEnumerable<IModule> modules)
        {
            pipeline.Render.Add(modules);
            return pipeline;
        }

        public static IPipeline WithRender(this IPipeline pipeline, params IModule[] modules)
        {
            pipeline.Render.Add(modules);
            return pipeline;
        }

        public static IPipeline WithWrite(this IPipeline pipeline, IEnumerable<IModule> modules)
        {
            pipeline.Write.Add(modules);
            return pipeline;
        }

        public static IPipeline WithWrite(this IPipeline pipeline, params IModule[] modules)
        {
            pipeline.Write.Add(modules);
            return pipeline;
        }

        public static IPipeline WithDependencies(this IPipeline pipeline, params IPipeline[] dependencies)
        {
            pipeline.Dependencies.AddRange(dependencies);
            return pipeline;
        }

        public static IPipeline WithDependencies(this IPipeline pipeline, IEnumerable<IPipeline> dependencies)
        {
            pipeline.Dependencies.AddRange(dependencies);
            return pipeline;
        }

        // TODO: WithDependencies(string names)

        // TODO: WithRead(string), WithWrite(string) (should these go in Wyam.Core - no access to modules here)

        public static IPipeline AsIsolated(this IPipeline pipeline, bool isolated = true)
        {
            pipeline.Isolated = isolated;
            return pipeline;
        }

        public static IPipeline AlwaysProcess(this IPipeline pipeline, bool alwaysProcess = true)
        {
            pipeline.AlwaysProcess = alwaysProcess;
            return pipeline;
        }
    }
}
