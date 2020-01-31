using System;
using System.Threading;

namespace Statiq.Common
{
    /// <summary>
    /// A read-only interface to the <see cref="IEngine"/> used during an execution.
    /// </summary>
    public interface IExecutionState : IDocumentFactory
    {
        /// <summary>
        /// Uniquely identifies the current execution cycle. This can be used to initialize and/or
        /// reset static data for a module on new generations (I.e., due to watching).
        /// For example, cache data could be cleared when this changes between runs.
        /// </summary>
        Guid ExecutionId { get; }

        /// <summary>
        /// Gets a cancellation token that will be canceled when processing should stop.
        /// </summary>
        CancellationToken CancellationToken { get; }

        /// <summary>
        /// Gets the state of the application when it was run.
        /// </summary>
        IReadOnlyApplicationState ApplicationState { get; }

        /// <summary>
        ///  Indicates that the engine is executing pipeline phases and modules in serial.
        /// </summary>
        bool SerialExecution { get; }

        /// <summary>
        /// Gets global events and event handlers.
        /// </summary>
        IReadOnlyEventCollection Events { get; }

        /// <summary>
        /// Gets the current file system.
        /// </summary>
        IReadOnlyFileSystem FileSystem { get; }

        /// <summary>
        /// The application configuration as metadata.
        /// </summary>
        IReadOnlyConfigurationSettings Settings { get; }

        /// <summary>
        /// Gets the available shortcodes.
        /// </summary>
        IReadOnlyShortcodeCollection Shortcodes { get; }

        /// <summary>
        /// Gets a set of namespaces that should be brought into scope for modules that perform dynamic compilation.
        /// </summary>
        INamespacesCollection Namespaces { get; }

        /// <summary>
        /// Provides pooled memory streams (via the RecyclableMemoryStream library).
        /// </summary>
        IMemoryStreamFactory MemoryStreamFactory { get; }

        /// <summary>
        /// Gets the collection of outputs from all previously processed documents.
        /// </summary>
        IPipelineOutputs Outputs { get; }

        /// <summary>
        /// Gets the dependency injection service provider.
        /// </summary>
        IServiceProvider Services { get; }
    }
}
