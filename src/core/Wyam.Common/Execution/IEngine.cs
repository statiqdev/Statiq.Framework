using Wyam.Common.Configuration;
using Wyam.Common.Documents;
using Wyam.Common.IO;
using Wyam.Common.Shortcodes;

namespace Wyam.Common.Execution
{
    /// <summary>
    /// The engine is the primary entry point for the generation process.
    /// </summary>
    public interface IEngine : IConfigurable
    {
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
        IMemoryStreamManager MemoryStreamManager { get; }

        /// <summary>
        /// Gets or sets the application input.
        /// </summary>
        string ApplicationInput { get; set; }

        /// <summary>
        /// Gets or sets the document factory.
        /// </summary>
        IDocumentFactory DocumentFactory { get; set; }
    }
}
