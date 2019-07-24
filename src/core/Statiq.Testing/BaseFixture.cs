using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Statiq.Common;
using Trace = Statiq.Common.Trace;

namespace Statiq.Testing
{
    [Parallelizable(ParallelScope.Children)]
    public abstract class BaseFixture
    {
        public static readonly IReadOnlyList<TestDocument> EmptyDocuments = ImmutableArray<TestDocument>.Empty;

        private readonly ConcurrentDictionary<string, TestTraceListener> _listeners =
            new ConcurrentDictionary<string, TestTraceListener>();

        public TestTraceListener Listener =>
            _listeners.TryGetValue(TestContext.CurrentContext.Test.ID, out TestTraceListener listener) ? listener : null;

        [SetUp]
        public void BaseSetUp()
        {
            NormalizedPath.PathComparisonType = System.StringComparison.OrdinalIgnoreCase;  // Normalize for tests
            Directory.SetCurrentDirectory(TestContext.CurrentContext.TestDirectory);
            TestTraceListener listener = new TestTraceListener(TestContext.CurrentContext.Test.ID);
            _listeners.AddOrUpdate(TestContext.CurrentContext.Test.ID, listener, (x, y) => listener);
            Trace.AddListener(Listener);
        }

        [TearDown]
        public void BaseTearDown()
        {
            RemoveListener();
        }

        public void RemoveListener()
        {
            TestTraceListener listener = Listener;
            if (listener != null)
            {
                Trace.RemoveListener(listener);
            }
        }

        public void ThrowOnTraceEventType(TraceEventType? traceEventType)
        {
            TestTraceListener listener = Listener;
            if (listener != null)
            {
                listener.ThrowTraceEventType = traceEventType;
            }
        }

        public static MemoryStream GetTestFileStream(string fileName) =>
            new MemoryStream(File.ReadAllBytes(Path.Combine(TestContext.CurrentContext.TestDirectory, fileName)));

        public static TestDocument[] GetDocuments(params string[] content) =>
            content.Select(x => new TestDocument(x)).ToArray();

        /// <summary>
        /// A utility method to execute modules in serial. The resulting documents will be materialized before returning.
        /// A new <see cref="TestExecutionContext"/> will be created and an empty initial document collection will be used.
        /// </summary>
        /// <param name="modules">The modules to execute.</param>
        /// <returns>A materialized list of result documents from the last module.</returns>
        public static Task<ImmutableArray<TestDocument>> ExecuteAsync(params IModule[] modules) =>
            ExecuteAsync(new TestExecutionContext(), modules);

        /// <summary>
        /// A utility method to execute modules in serial. The resulting documents will be materialized before returning.
        /// </summary>
        /// <param name="context">The execution context to use.</param>
        /// <param name="modules">The modules to execute.</param>
        /// <returns>A materialized list of result documents from the last module.</returns>
        public static async Task<ImmutableArray<TestDocument>> ExecuteAsync(TestExecutionContext context, params IModule[] modules) =>
            (await context.ExecuteAsync(modules, context.Inputs)).Cast<TestDocument>().ToImmutableArray();

        /// <summary>
        /// A utility method to execute modules in serial. The resulting documents will be materialized before returning.
        /// A new <see cref="TestExecutionContext"/> will be created.
        /// </summary>
        /// <param name="document">The initial input document.</param>
        /// <param name="modules">The modules to execute.</param>
        /// <returns>A materialized list of result documents from the last module.</returns>
        public static Task<ImmutableArray<TestDocument>> ExecuteAsync(TestDocument document, params IModule[] modules) =>
            ExecuteAsync(new TestExecutionContext(ImmutableArray.Create(document)), modules);

        /// <summary>
        /// A utility method to execute modules in serial. The resulting documents will be materialized before returning.
        /// A new <see cref="TestExecutionContext"/> will be created.
        /// </summary>
        /// <param name="documents">The initial input documents.</param>
        /// <param name="modules">The modules to execute.</param>
        /// <returns>A materialized list of result documents from the last module.</returns>
        public static Task<ImmutableArray<TestDocument>> ExecuteAsync(IEnumerable<TestDocument> documents, params IModule[] modules) =>
            ExecuteAsync(new TestExecutionContext(documents), modules);

        /// <summary>
        /// A utility method to execute modules in serial. The resulting documents will be materialized before returning.
        /// A new <see cref="TestExecutionContext"/> will be created.
        /// </summary>
        /// <param name="document">The initial input document.</param>
        /// <param name="context">The execution context to use.</param>
        /// <param name="modules">The modules to execute.</param>
        /// <returns>A materialized list of result documents from the last module.</returns>
        public static async Task<ImmutableArray<TestDocument>> ExecuteAsync(TestDocument document, TestExecutionContext context, params IModule[] modules) =>
            (await context.ExecuteAsync(modules, document?.Yield())).Cast<TestDocument>().ToImmutableArray();

        /// <summary>
        /// A utility method to execute modules in serial. The resulting documents will be materialized before returning.
        /// A new <see cref="TestExecutionContext"/> will be created.
        /// </summary>
        /// <param name="documents">The initial input documents.</param>
        /// <param name="context">The execution context to use.</param>
        /// <param name="modules">The modules to execute.</param>
        /// <returns>A materialized list of result documents from the last module.</returns>
        public static async Task<ImmutableArray<TestDocument>> ExecuteAsync(IEnumerable<TestDocument> documents, TestExecutionContext context, params IModule[] modules) =>
            (await context.ExecuteAsync(modules, documents)).Cast<TestDocument>().ToImmutableArray();
    }
}
