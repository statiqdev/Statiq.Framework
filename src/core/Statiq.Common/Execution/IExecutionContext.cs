using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Statiq.Common.Configuration;
using Statiq.Common.Content;
using Statiq.Common.Documents;
using Statiq.Common.IO;
using Statiq.Common.JavaScript;
using Statiq.Common.Meta;
using Statiq.Common.Modules;
using Statiq.Common.Shortcodes;

namespace Statiq.Common.Execution
{
    /// <summary>
    /// All of the information that represents a given build. Also implements
    /// <see cref="IMetadata"/> to expose the global metadata.
    /// </summary>
    public interface IExecutionContext : IMetadata, ITypeConverter
    {
        /// <summary>
        /// Uniquly identifies the current execution cycle. This can be used to initialize and/or
        /// reset static data for a module on new generations (I.e., due to watching).
        /// For example, cache data could be cleared when this changes between runs.
        /// </summary>
        Guid ExecutionId { get; }

        /// <summary>
        /// Gets the raw bytes for dynamically compiled assemblies (such as the configuration script).
        /// </summary>
        IReadOnlyCollection<byte[]> DynamicAssemblies { get; }

        /// <summary>
        /// Gets a set of namespaces that should be brought into scope for modules that perform dynamic compilation.
        /// </summary>
        IReadOnlyCollection<string> Namespaces { get; }

        /// <summary>
        /// Provides pooled memory streams (via the RecyclableMemoryStream library).
        /// </summary>
        IMemoryStreamFactory MemoryStreamFactory { get; }

        /// <summary>
        /// Gets the name of the currently executing pipeline.
        /// </summary>
        string PipelineName { get; }

        /// <summary>
        /// Gets the name of the currently executing pipeline phase.
        /// </summary>
        Phase Phase { get; }

        /// <summary>
        /// Gets the currently executing module.
        /// </summary>
        IModule Module { get; }

        /// <summary>
        /// Gets the current file system.
        /// </summary>
        IReadOnlyFileSystem FileSystem { get; }

        /// <summary>
        /// Gets the current settings metadata.
        /// </summary>
        IReadOnlySettings Settings { get; }

        /// <summary>
        /// Gets the available shortcodes.
        /// </summary>
        IReadOnlyShortcodeCollection Shortcodes { get; }

        /// <summary>
        /// Gets the collection of all previously processed documents.
        /// </summary>
        IDocumentCollection Documents { get; }

        /// <summary>
        /// Gets the dependency injection service provider. A new scope is
        /// created for each pipeline phase.
        /// </summary>
        IServiceProvider Services { get; }

        /// <summary>
        /// Gets any input that was passed to the application (for example, on stdin via piping).
        /// </summary>
        /// <value>
        /// The application input.
        /// </value>
        string ApplicationInput { get; }

        /// <summary>
        /// Creates a <see cref="HttpClient"/> instance that should be used for all HTTP communication.
        /// </summary>
        /// <returns>A new <see cref="HttpClient"/> instance.</returns>
        HttpClient CreateHttpClient();

        /// <summary>
        /// Creates a new <see cref="HttpClient"/> instance that uses a custom message handler.
        /// </summary>
        /// <param name="handler">The message handler to use for this client.</param>
        /// <returns>A new <see cref="HttpClient"/> instance.</returns>
        HttpClient CreateHttpClient(HttpMessageHandler handler);

        /// <summary>
        /// Gets a <see cref="Stream"/> that can be used for document content. If <paramref name="content"/>
        /// is not null, the stream is initialized with the specified content. It is prefered to use
        /// this method to obtain a stream over creating your own if the source of the content does
        /// not already provide one. The returned streams are optimized for memory usage and performance.
        /// Instances of the returned stream should be disposed when writing is complete.
        /// </summary>
        /// <remarks>The position is set to the beginning of the stream when returned.</remarks>
        /// <param name="content">Content to initialize the stream with.</param>
        /// <returns>A stream for document content.</returns>
        Task<Stream> GetContentStreamAsync(string content = null);

        /// <summary>
        /// Gets a new document. Modules should use this method of obtaining documents during execution instead of instantiating them directly when possible so that the
        /// default document type can be replaced by swapping out the document factory.
        /// </summary>
        /// <param name="source">The source (if the original document contains a source, then this is ignored and the original document's source is used instead).</param>
        /// <param name="destination">The destination.</param>
        /// <param name="items">The metadata items.</param>
        /// <param name="contentProvider">The content provider.</param>
        /// <returns>The cloned or new document.</returns>
        IDocument GetDocument(
            FilePath source,
            FilePath destination,
            IEnumerable<KeyValuePair<string, object>> items,
            IContentProvider contentProvider = null);

        /// <summary>
        /// Executes the specified modules with the specified input documents and returns the result documents.
        /// </summary>
        /// <param name="modules">The modules to execute.</param>
        /// <param name="inputs">The input documents.</param>
        /// <returns>The result documents from the executed modules.</returns>
        Task<ImmutableArray<IDocument>> ExecuteAsync(IEnumerable<IModule> modules, IEnumerable<IDocument> inputs);

        /// <summary>
        /// Gets a new <see cref="IJavaScriptEnginePool"/>. The returned engine pool should be disposed
        /// when no longer needed.
        /// </summary>
        /// <param name="initializer">
        /// The code to run when a new engine is created. This should configure
        /// the environment and set up any required JavaScript libraries.
        /// </param>
        /// <param name="startEngines">The number of engines to initially start when a pool is created.</param>
        /// <param name="maxEngines">The maximum number of engines that will be created in the pool.</param>
        /// <param name="maxUsagesPerEngine">The maximum number of times an engine can be reused before it is disposed.</param>
        /// <param name="engineTimeout">
        /// The default timeout to use when acquiring an engine from the pool (defaults to 5 seconds).
        /// If an engine can not be acquired in this timeframe, an exception will be thrown.
        /// </param>
        /// <returns>A new JavaScript engine pool.</returns>
        IJavaScriptEnginePool GetJavaScriptEnginePool(
            Action<IJavaScriptEngine> initializer = null,
            int startEngines = 10,
            int maxEngines = 25,
            int maxUsagesPerEngine = 100,
            TimeSpan? engineTimeout = null);
    }
}
