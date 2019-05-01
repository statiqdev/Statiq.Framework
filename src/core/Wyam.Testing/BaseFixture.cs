using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Wyam.Common.Documents;
using Wyam.Common.Execution;
using Wyam.Common.Modules;
using Wyam.Testing.Documents;
using Wyam.Testing.Execution;
using Wyam.Testing.Tracing;
using Trace = Wyam.Common.Tracing.Trace;

namespace Wyam.Testing
{
    public abstract class BaseFixture
    {
        public static readonly IReadOnlyList<IDocument> EmptyDocuments = ImmutableArray<IDocument>.Empty;

        private readonly ConcurrentDictionary<string, TestTraceListener> _listeners =
            new ConcurrentDictionary<string, TestTraceListener>();

        public TestTraceListener Listener =>
            _listeners.TryGetValue(TestContext.CurrentContext.Test.ID, out TestTraceListener listener) ? listener : null;

        [SetUp]
        public void BaseSetUp()
        {
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

        /// <summary>
        /// A utility method to execute modules in serial. The resulting documents will be materialized before returning.
        /// A new <see cref="TestExecutionContext"/> will be created and an empty initial document collection will be used.
        /// </summary>
        /// <param name="modules">The modules to execute.</param>
        /// <returns>A materialized list of result documents from the last module.</returns>
        public static Task<IReadOnlyList<IDocument>> ExecuteAsync(params IModule[] modules) =>
            ExecuteAsync(new TestExecutionContext(), modules);

        /// <summary>
        /// A utility method to execute modules in serial. The resulting documents will be materialized before returning.
        /// An empty initial document collection will be used.
        /// </summary>
        /// <param name="context">The execution context to use.</param>
        /// <param name="modules">The modules to execute.</param>
        /// <returns>A materialized list of result documents from the last module.</returns>
        public static Task<IReadOnlyList<IDocument>> ExecuteAsync(IExecutionContext context, params IModule[] modules) =>
            ExecuteAsync(EmptyDocuments, context, modules);

        /// <summary>
        /// A utility method to execute modules in serial. The resulting documents will be materialized before returning.
        /// A new <see cref="TestExecutionContext"/> will be created.
        /// </summary>
        /// <param name="documents">The initial input documents.</param>
        /// <param name="modules">The modules to execute.</param>
        /// <returns>A materialized list of result documents from the last module.</returns>
        public static Task<IReadOnlyList<IDocument>> ExecuteAsync(IReadOnlyList<IDocument> documents, params IModule[] modules) =>
            ExecuteAsync(documents, new TestExecutionContext(), modules);

        /// <summary>
        /// A utility method to execute modules in serial. The resulting documents will be materialized before returning.
        /// </summary>
        /// <param name="documents">The initial input documents.</param>
        /// <param name="context">The execution context to use.</param>
        /// <param name="modules">The modules to execute.</param>
        /// <returns>A materialized list of result documents from the last module.</returns>
        public static async Task<IReadOnlyList<IDocument>> ExecuteAsync(IReadOnlyList<IDocument> documents, IExecutionContext context, params IModule[] modules)
        {
            foreach (IModule module in modules)
            {
                documents = (await module.ExecuteAsync(documents, context)).ToList();
            }
            return documents;
        }
    }
}
