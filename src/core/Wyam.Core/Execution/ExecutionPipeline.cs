using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using ConcurrentCollections;
using Wyam.Common;
using Wyam.Common.Documents;
using Wyam.Common.IO;
using Wyam.Common.Modules;
using Wyam.Common.Execution;
using Wyam.Common.Tracing;
using Wyam.Common.Util;
using Wyam.Core.Caching;
using Wyam.Core.Documents;
using Wyam.Core.Meta;

namespace Wyam.Core.Execution
{
    internal class ExecutionPipeline : IPipeline, IDisposable
    {
        private readonly ConcurrentHashSet<FilePath> _documentSources = new ConcurrentHashSet<FilePath>();
        private readonly IModuleList _modules;
        private ConcurrentBag<IDocument> _clonedDocuments = new ConcurrentBag<IDocument>();
        private bool _disposed;

        public string Name { get; }

        public ExecutionPipeline(string name, params IModule[] modules)
            : this(name, new ModuleList(modules))
        {
        }

        public ExecutionPipeline(string name, IModuleList modules)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException(nameof(name));
            }
            Name = name;
            _modules = modules ?? new ModuleList();
        }

        // This is the main execute method called by the engine
        public async Task ExecuteAsync(Engine engine, Guid executionId, IServiceProvider serviceProvider)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(ExecutionPipeline));
            }

            // Setup for pipeline execution
            _documentSources.Clear();
            ResetClonedDocuments();

            // Execute all modules in the pipeline
            IReadOnlyList<IDocument> resultDocuments;
            using (ExecutionContext context = new ExecutionContext(engine, executionId, this, serviceProvider))
            {
                ImmutableArray<IDocument> inputs = new[] { engine.DocumentFactory.GetDocument(context) }.ToImmutableArray();
                resultDocuments = await ExecuteAsync(context, _modules, inputs);
            }

            // Dispose documents that aren't part of the final collection for this pipeline,
            // but don't dispose any documents that are referenced directly or indirectly from the final ones
            HashSet<IDocument> flattenedResultDocuments = new HashSet<IDocument>();
            FlattenResultDocuments(resultDocuments, flattenedResultDocuments);
            Parallel.ForEach(_clonedDocuments.Where(x => !flattenedResultDocuments.Contains(x)), x => x.Dispose());
            _clonedDocuments = new ConcurrentBag<IDocument>(flattenedResultDocuments);
        }

        // This executes the specified modules with the specified input documents
        public async Task<IReadOnlyList<IDocument>> ExecuteAsync(ExecutionContext context, IEnumerable<IModule> modules, ImmutableArray<IDocument> inputDocuments)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(ExecutionPipeline));
            }

            ImmutableArray<IDocument> resultDocuments = ImmutableArray<IDocument>.Empty;
            foreach (IModule module in modules.Where(x => x != null))
            {
                string moduleName = module.GetType().Name;
                System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();
                using (Trace.WithIndent().Verbose("Executing module {0} with {1} input document(s)", moduleName, inputDocuments.Length))
                {
                    try
                    {
                        // Execute the module
                        using (ExecutionContext moduleContext = context.Clone(module))
                        {
                            IEnumerable<IDocument> moduleResult = await module.ExecuteAsync(inputDocuments, moduleContext);
                            resultDocuments = moduleResult?.Where(x => x != null).ToImmutableArray() ?? ImmutableArray<IDocument>.Empty;
                        }

                        // Set results in engine and trace
                        context.Engine.DocumentCollection.Set(Name, resultDocuments);
                        stopwatch.Stop();
                        Trace.Verbose(
                            "Executed module {0} in {1} ms resulting in {2} output document(s)",
                            moduleName,
                            stopwatch.ElapsedMilliseconds,
                            resultDocuments.Length);
                        inputDocuments = resultDocuments;
                    }
                    catch (Exception)
                    {
                        Trace.Error("Error while executing module {0}", moduleName);
                        resultDocuments = ImmutableArray<IDocument>.Empty;
                        context.Engine.DocumentCollection.Set(Name, resultDocuments);
                        throw;
                    }
                }
            }

            // Set the document collection result one more time just to be sure it matches the result documents
            context.Engine.DocumentCollection.Set(Name, resultDocuments);
            return resultDocuments;
        }

        private void FlattenResultDocuments(IEnumerable<IDocument> documents, HashSet<IDocument> flattenedResultDocuments)
        {
            foreach (IDocument document in documents)
            {
                if (document == null || !flattenedResultDocuments.Add(document))
                {
                    continue;
                }

                FlattenResultDocuments(
                    document.Keys.SelectMany(x =>
                    {
                        object value = document.GetRaw(x);
                        IEnumerable<IDocument> children = value as IEnumerable<IDocument>;
                        if (children == null && value is IDocument)
                        {
                            children = new[] { (IDocument)value };
                        }
                        return children ?? Enumerable.Empty<IDocument>();
                    }),
                    flattenedResultDocuments);
            }
        }

        public void AddClonedDocument(IDocument document) => _clonedDocuments.Add(document);

        public void AddDocumentSource(FilePath source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            if (!_documentSources.Add(source))
            {
                throw new ArgumentException("Document source must be unique within the pipeline: " + source);
            }
        }

        public void ResetClonedDocuments()
        {
            Parallel.ForEach(_clonedDocuments, x => x.Dispose());
            _clonedDocuments = new ConcurrentBag<IDocument>();
        }

        public void Dispose()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(ExecutionPipeline));
            }
            _disposed = true;

            // Clean up the documents
            ResetClonedDocuments();

            // Clean up the modules
            DisposeModules(_modules);
        }

        private void DisposeModules(IEnumerable<IModule> modules)
        {
            foreach (IModule module in modules)
            {
                (module as IDisposable)?.Dispose();
                IEnumerable<IModule> childModules = module as IEnumerable<IModule>;
                if (childModules != null)
                {
                    DisposeModules(childModules);
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public IEnumerator<IModule> GetEnumerator() => _modules.GetEnumerator();

        public void Add(IModule item) => _modules.Add(item);

        public void Clear() => _modules.Clear();

        public bool Contains(IModule item) => _modules.Contains(item);

        public void CopyTo(IModule[] array, int arrayIndex) => _modules.CopyTo(array, arrayIndex);

        public bool Remove(IModule item) => _modules.Remove(item);

        public int Count => _modules.Count;

        public void Add(params IModule[] modules) => _modules.Add(modules);

        public void Insert(int index, params IModule[] modules) => _modules.Insert(index, modules);

        public bool IsReadOnly => _modules.IsReadOnly;

        public int IndexOf(IModule item) => _modules.IndexOf(item);

        public void Insert(int index, IModule item) => _modules.Insert(index, item);

        public void RemoveAt(int index) => _modules.RemoveAt(index);

        public IModule this[int index]
        {
            get { return _modules[index]; }
            set { _modules[index] = value; }
        }
    }
}
