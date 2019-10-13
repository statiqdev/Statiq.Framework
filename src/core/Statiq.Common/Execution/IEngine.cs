using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Statiq.Common
{
    /// <summary>
    /// The engine is the primary entry point for the generation process.
    /// </summary>
    public interface IEngine : IConfigurable, IDocumentFactory
    {
        /// <summary>
        /// Gets the state of the application when it was run.
        /// </summary>
        ApplicationState ApplicationState { get; }

        /// <summary>
        /// The application configuration.
        /// </summary>
        IConfiguration Configuration { get; }

        /// <summary>
        /// Gets global events and event handlers.
        /// </summary>
        IEventCollection Events { get; }

        /// <summary>
        /// Gets the dependency injection service provider.
        /// </summary>
        IServiceProvider Services { get; }

        /// <summary>
        /// Gets the file system.
        /// </summary>
        IFileSystem FileSystem { get; }

        /// <summary>
        /// Gets the settings.
        /// </summary>
        ISettings Settings { get; }

        /// <summary>
        /// Gets the pipelines.
        /// </summary>
        IPipelineCollection Pipelines { get; }

        /// <summary>
        /// Gets the shortcodes.
        /// </summary>
        IShortcodeCollection Shortcodes { get; }

        /// <summary>
        /// Gets the namespaces that should be brought in scope by modules that support dynamic compilation.
        /// </summary>
        INamespacesCollection Namespaces { get; }

        /// <summary>
        /// Gets a collection of all the raw assemblies that should be referenced by modules
        /// that support dynamic compilation (such as configuration assemblies).
        /// </summary>
        IRawAssemblyCollection DynamicAssemblies { get; }

        /// <summary>
        /// Provides pooled memory streams (via the RecyclableMemoryStream library).
        /// </summary>
        IMemoryStreamFactory MemoryStreamFactory { get; }

        /// <summary>
        /// Executes pipeline phases and modules in serial.
        /// </summary>
        /// <remarks>
        /// Setting this to <c>true</c> will disable most (but not all) concurrency and is useful for debugging.
        /// </remarks>
        bool SerialExecution { get; set; }

        /// <summary>
        /// Sets the default document type produced by this engine (and resulting <see cref="IExecutionContext"/> contexts).
        /// </summary>
        /// <remarks>
        /// To use a custom document type, derive the document type from <see cref="Document{TDocument}"/> and then call
        /// this method on the engine before execution to set the custom document type for calls to <c>CreateDocument</c>
        /// and <c>CloneOrCreateDocument</c>.
        /// </remarks>
        /// <typeparam name="TDocument">The default document type.</typeparam>
        void SetDefaultDocumentType<TDocument>()
            where TDocument : FactoryDocument, IDocument, new();
    }
}
