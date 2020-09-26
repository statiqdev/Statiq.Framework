using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Statiq.Common
{
    /// <summary>
    /// The engine is the primary entry point for the generation process.
    /// </summary>
    public interface IEngine : IConfigurable, IExecutionState
    {
        /// <summary>
        /// Gets the state of the application when it was run.
        /// </summary>
        new ApplicationState ApplicationState { get; }

        /// <summary>
        /// Gets global events and event handlers.
        /// </summary>
        new IEventCollection Events { get; }

        /// <summary>
        /// Gets the configuration and settings.
        /// </summary>
        new ISettings Settings { get; }

        /// <summary>
        /// Gets the file system.
        /// </summary>
        new IFileSystem FileSystem { get; }

        /// <summary>
        /// Gets the shortcodes.
        /// </summary>
        new IShortcodeCollection Shortcodes { get; }

        /// <summary>
        /// Executes pipeline phases and modules in serial.
        /// </summary>
        /// <remarks>
        /// Setting this to <c>true</c> will disable most (but not all) concurrency and is useful for debugging.
        /// </remarks>
        new bool SerialExecution { get; set; }

        /// <summary>
        /// Gets the pipelines.
        /// </summary>
        new IPipelineCollection Pipelines { get; }

        new ILogger Logger { get; }

        /// <summary>
        /// A collection of validators.
        /// </summary>
        IValidatorCollection Validators { get; }

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
