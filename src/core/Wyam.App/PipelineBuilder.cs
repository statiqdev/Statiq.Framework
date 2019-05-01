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

        public PipelineBuilder AsSerial()
        {
            _actions.Add(x => x.Dependencies.AddRange(_collection.Keys));
            return this;
        }

        // TODO: AddRead(string), AddWrite(string) to create ReadFiles/WriteFiles modules

        public PipelineBuilder WithReadModules(IEnumerable<IModule> modules)
        {
            _actions.Add(x => x.WithReadModules(modules));
            return this;
        }

        public PipelineBuilder WithReadModules(params IModule[] modules)
        {
            _actions.Add(x => x.WithReadModules(modules));
            return this;
        }

        public PipelineBuilder WithProcessModules(IEnumerable<IModule> modules)
        {
            _actions.Add(x => x.WithProcessModules(modules));
            return this;
        }

        public PipelineBuilder WithProcessModules(params IModule[] modules)
        {
            _actions.Add(x => x.WithProcessModules(modules));
            return this;
        }

        public PipelineBuilder WithRenderModules(IEnumerable<IModule> modules)
        {
            _actions.Add(x => x.WithRenderModules(modules));
            return this;
        }

        public PipelineBuilder WithRenderModules(params IModule[] modules)
        {
            _actions.Add(x => x.WithRenderModules(modules));
            return this;
        }

        public PipelineBuilder WithWriteModules(IEnumerable<IModule> modules)
        {
            _actions.Add(x => x.WithWriteModules(modules));
            return this;
        }

        public PipelineBuilder WithWriteModules(params IModule[] modules)
        {
            _actions.Add(x => x.WithWriteModules(modules));
            return this;
        }

        public PipelineBuilder WithDependencies(params string[] dependencies)
        {
            _actions.Add(x => x.WithDependencies(dependencies));
            return this;
        }

        public PipelineBuilder WithDependencies(IEnumerable<string> dependencies)
        {
            _actions.Add(x => x.WithDependencies(dependencies));
            return this;
        }

        public PipelineBuilder AsIsolated(bool isolated = true)
        {
            _actions.Add(x => x.AsIsolated(isolated));
            return this;
        }

        public PipelineBuilder AlwaysProcess(bool alwaysProcess = true)
        {
            _actions.Add(x => x.AlwaysProcess(alwaysProcess));
            return this;
        }
    }
}
