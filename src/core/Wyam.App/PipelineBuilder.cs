using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wyam.Common.Configuration;
using Wyam.Common.Execution;
using Wyam.Common.Modules;
using Wyam.Common.Util;
using Wyam.Core.Execution;

namespace Wyam.App
{
    public class PipelineBuilder
    {
        private readonly List<Action<IPipeline>> _actions = new List<Action<IPipeline>>();

        private readonly IPipelineCollection _collection;

        internal PipelineBuilder(IPipelineCollection collection, IReadOnlySettings settings)
        {
            _collection = collection;
            Settings = settings;
        }

        public IReadOnlySettings Settings { get; }

        public IPipeline Build()
        {
            IPipeline pipeline = new Pipeline();
            foreach (Action<IPipeline> action in _actions)
            {
                action(pipeline);
            }
            return pipeline;
        }

        public PipelineBuilder AddRead(IEnumerable<IModule> modules)
        {
            _actions.Add(x => x.Read.Add(modules));
            return this;
        }

        public PipelineBuilder AddRead(params IModule[] modules)
        {
            _actions.Add(x => x.Read.Add(modules));
            return this;
        }

        public PipelineBuilder AddProcess(IEnumerable<IModule> modules)
        {
            _actions.Add(x => x.Process.Add(modules));
            return this;
        }

        public PipelineBuilder AddProcess(params IModule[] modules)
        {
            _actions.Add(x => x.Process.Add(modules));
            return this;
        }

        public PipelineBuilder AddRender(IEnumerable<IModule> modules)
        {
            _actions.Add(x => x.Render.Add(modules));
            return this;
        }

        public PipelineBuilder AddRender(params IModule[] modules)
        {
            _actions.Add(x => x.Render.Add(modules));
            return this;
        }

        public PipelineBuilder AddWrite(IEnumerable<IModule> modules)
        {
            _actions.Add(x => x.Write.Add(modules));
            return this;
        }

        public PipelineBuilder AddWrite(params IModule[] modules)
        {
            _actions.Add(x => x.Write.Add(modules));
            return this;
        }

        public PipelineBuilder AddDependencies(params IPipeline[] dependencies)
        {
            _actions.Add(x => x.Dependencies.AddRange(dependencies));
            return this;
        }

        public PipelineBuilder AddDependencies(IEnumerable<IPipeline> dependencies)
        {
            _actions.Add(x => x.Dependencies.AddRange(dependencies));
            return this;
        }

        public PipelineBuilder AddDependencies(params string[] dependencies)
        {
            _actions.Add(x => x.Dependencies.AddRange(dependencies.Select(d => _collection[d])));
            return this;
        }

        public PipelineBuilder AddDependencies(IEnumerable<string> dependencies)
        {
            _actions.Add(x => x.Dependencies.AddRange(dependencies.Select(d => _collection[d])));
            return this;
        }

        public PipelineBuilder AsSerial()
        {
            _actions.Add(x => x.Dependencies.AddRange(_collection.Values));
            return this;
        }

        public PipelineBuilder AsIsolated(bool isolated = true)
        {
            _actions.Add(x => x.Isolated = isolated);
            return this;
        }

        public PipelineBuilder AlwaysProcess(bool alwaysProcess = true)
        {
            _actions.Add(x => x.AlwaysProcess = alwaysProcess);
            return this;
        }

        // TODO: AddRead(string), AddWrite(string)
    }
}
