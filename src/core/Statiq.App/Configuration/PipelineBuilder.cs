using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Statiq.Common;
using Statiq.Core;

namespace Statiq.App
{
    public class PipelineBuilder
    {
        private readonly List<Action<IPipeline>> _actions = new List<Action<IPipeline>>();

        private readonly IPipelineCollection _collection;

        internal PipelineBuilder(IPipelineCollection collection, IReadOnlyConfigurationSettings settings)
        {
            _collection = collection;
            Settings = settings;
        }

        public IReadOnlyConfigurationSettings Settings { get; }

        internal IPipeline Build()
        {
            if (_actions.Count == 0)
            {
                return null;
            }

            IPipeline pipeline = new Pipeline();
            foreach (Action<IPipeline> action in _actions)
            {
                action(pipeline);
            }
            return pipeline;
        }

        public PipelineBuilder WithInputReadFiles(params string[] patterns)
        {
            _actions.Add(x => x.InputModules.Add(new ReadFiles((Config<IEnumerable<string>>)patterns)));
            return this;
        }

        public PipelineBuilder WithInputReadFiles(IEnumerable<string> patterns)
        {
            if (patterns != null)
            {
                _actions.Add(x => x.InputModules.Add(new ReadFiles((Config<IEnumerable<string>>)patterns)));
            }
            return this;
        }

        public PipelineBuilder WithOutputWriteFiles()
        {
            _actions.Add(x => x.OutputModules.Add(new WriteFiles()));
            return this;
        }

        public PipelineBuilder WithOutputWriteFiles(string extension)
        {
            _actions.Add(x => x.OutputModules.Add(
                new SetDestination(extension),
                new WriteFiles()));
            return this;
        }

        public PipelineBuilder WithOutputWriteFiles(Config<NormalizedPath> path)
        {
            _actions.Add(x => x.OutputModules.Add(
                new SetDestination(path),
                new WriteFiles()));
            return this;
        }

        public PipelineBuilder AsSerial()
        {
            // Make sure not to add isolated pipelines as dependencies
            _actions.Add(x => x.Dependencies.AddRange(_collection.Where(p => !p.Value.Isolated).Select(p => p.Key)));
            return this;
        }

        public PipelineBuilder WithInputModules(IEnumerable<IModule> modules)
        {
            if (modules != null)
            {
                _actions.Add(x => x.WithInputModules(modules));
            }
            return this;
        }

        public PipelineBuilder WithInputModules(params IModule[] modules)
        {
            _actions.Add(x => x.WithInputModules(modules));
            return this;
        }

        public PipelineBuilder WithProcessModules(IEnumerable<IModule> modules)
        {
            if (modules != null)
            {
                _actions.Add(x => x.WithProcessModules(modules));
            }
            return this;
        }

        public PipelineBuilder WithProcessModules(params IModule[] modules)
        {
            _actions.Add(x => x.WithProcessModules(modules));
            return this;
        }

        public PipelineBuilder WithTransformModules(IEnumerable<IModule> modules)
        {
            if (modules != null)
            {
                _actions.Add(x => x.WithPostProcessModules(modules));
            }
            return this;
        }

        public PipelineBuilder WithTransformModules(params IModule[] modules)
        {
            _actions.Add(x => x.WithPostProcessModules(modules));
            return this;
        }

        public PipelineBuilder WithOutputModules(IEnumerable<IModule> modules)
        {
            if (modules != null)
            {
                _actions.Add(x => x.WithOutputModules(modules));
            }
            return this;
        }

        public PipelineBuilder WithOutputModules(params IModule[] modules)
        {
            _actions.Add(x => x.WithOutputModules(modules));
            return this;
        }

        public PipelineBuilder WithInputConfig(Config<object> config)
        {
            _actions.Add(x => x.WithInputConfig(config));
            return this;
        }

        public PipelineBuilder WithProcessConfig(Config<object> config)
        {
            _actions.Add(x => x.WithProcessConfig(config));
            return this;
        }

        public PipelineBuilder WithTransformConfig(Config<object> config)
        {
            _actions.Add(x => x.WithTransformConfig(config));
            return this;
        }

        public PipelineBuilder WithOutputConfig(Config<object> config)
        {
            _actions.Add(x => x.WithOutputConfig(config));
            return this;
        }

        public PipelineBuilder WithDependencies(params string[] dependencies)
        {
            _actions.Add(x => x.WithDependencies(dependencies));
            return this;
        }

        public PipelineBuilder WithDependencies(IEnumerable<string> dependencies)
        {
            if (dependencies != null)
            {
                _actions.Add(x => x.WithDependencies(dependencies));
            }
            return this;
        }

        public PipelineBuilder AsIsolated(bool isolated = true)
        {
            _actions.Add(x => x.AsIsolated(isolated));
            return this;
        }

        public PipelineBuilder AsDeployment(bool deployment = true)
        {
            _actions.Add(x => x.AsDeployment(deployment));
            return this;
        }

        public PipelineBuilder WithExecutionPolicy(ExecutionPolicy policy)
        {
            _actions.Add(x => x.WithExecutionPolicy(policy));
            return this;
        }

        public PipelineBuilder ManuallyExecute()
        {
            _actions.Add(x => x.ManuallyExecute());
            return this;
        }

        public PipelineBuilder AlwaysExecute()
        {
            _actions.Add(x => x.AlwaysExecute());
            return this;
        }
    }
}
