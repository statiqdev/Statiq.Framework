using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Statiq.Common;
using Statiq.Core;

namespace Statiq.App
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
            _actions.Add(x => x.InputModules.Add(new ReadFiles((DocumentConfig<IEnumerable<string>>)patterns)));
            return this;
        }

        public PipelineBuilder WithInputReadFiles(IEnumerable<string> patterns)
        {
            if (patterns != null)
            {
                _actions.Add(x => x.InputModules.Add(new ReadFiles((DocumentConfig<IEnumerable<string>>)patterns)));
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

        public PipelineBuilder WithOutputWriteFiles(DocumentConfig<FilePath> path)
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
                _actions.Add(x => x.WithTransformModules(modules));
            }
            return this;
        }

        public PipelineBuilder WithTransformModules(params IModule[] modules)
        {
            _actions.Add(x => x.WithTransformModules(modules));
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

        public PipelineBuilder AlwaysProcess(bool alwaysProcess = true)
        {
            _actions.Add(x => x.AlwaysProcess(alwaysProcess));
            return this;
        }

        public PipelineBuilder WithInputDelegate(Action<IReadOnlyList<IDocument>, IExecutionContext> action)
        {
            _actions.Add(x => x.WithInputDelegate(action));
            return this;
        }

        public PipelineBuilder WithInputDelegate(Func<IReadOnlyList<IDocument>, IExecutionContext, Task> func)
        {
            _actions.Add(x => x.WithInputDelegate(func));
            return this;
        }

        public PipelineBuilder WithInputDelegate(Func<IReadOnlyList<IDocument>, IExecutionContext, Task<object>> func)
        {
            _actions.Add(x => x.WithInputDelegate(func));
            return this;
        }

        public PipelineBuilder WithProcessDelegate(Action<IReadOnlyList<IDocument>, IExecutionContext> action)
        {
            _actions.Add(x => x.WithProcessDelegate(action));
            return this;
        }

        public PipelineBuilder WithProcessDelegate(Func<IReadOnlyList<IDocument>, IExecutionContext, Task> func)
        {
            _actions.Add(x => x.WithProcessDelegate(func));
            return this;
        }

        public PipelineBuilder WithProcessDelegate(Func<IReadOnlyList<IDocument>, IExecutionContext, Task<object>> func)
        {
            _actions.Add(x => x.WithProcessDelegate(func));
            return this;
        }

        public PipelineBuilder WithTransformDelegate(Action<IReadOnlyList<IDocument>, IExecutionContext> action)
        {
            _actions.Add(x => x.WithTransformDelegate(action));
            return this;
        }

        public PipelineBuilder WithTransformDelegate(Func<IReadOnlyList<IDocument>, IExecutionContext, Task> func)
        {
            _actions.Add(x => x.WithTransformDelegate(func));
            return this;
        }

        public PipelineBuilder WithTransformDelegate(Func<IReadOnlyList<IDocument>, IExecutionContext, Task<object>> func)
        {
            _actions.Add(x => x.WithTransformDelegate(func));
            return this;
        }

        public PipelineBuilder WithOutputDelegate(Action<IReadOnlyList<IDocument>, IExecutionContext> action)
        {
            _actions.Add(x => x.WithOutputDelegate(action));
            return this;
        }

        public PipelineBuilder WithOutputDelegate(Func<IReadOnlyList<IDocument>, IExecutionContext, Task> func)
        {
            _actions.Add(x => x.WithOutputDelegate(func));
            return this;
        }

        public PipelineBuilder WithOutputDelegate(Func<IReadOnlyList<IDocument>, IExecutionContext, Task<object>> func)
        {
            _actions.Add(x => x.WithOutputDelegate(func));
            return this;
        }
    }
}
