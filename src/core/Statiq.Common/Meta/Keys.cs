using System;
using System.Collections.Generic;
using System.Globalization;

namespace Statiq.Common
{
    /// <summary>
    /// Common metadata keys for modules in the core library.
    /// </summary>
    public static class Keys
    {
        // Settings

        /// <summary>
        /// The host to use when generating links.
        /// </summary>
        /// <type><see cref="string"/></type>
        public const string Host = nameof(Host);

        /// <summary>
        /// Indicates if generated links should use HTTPS instead of HTTP as the scheme.
        /// </summary>
        /// <type><see cref="bool"/></type>
        public const string LinksUseHttps = nameof(LinksUseHttps);

        /// <summary>
        /// The default root path to use when generating links
        /// (for example, <code>"/virtual/directory"</code>).
        /// Note that you may also want to use the <code>--virtual-dir</code>
        /// argument on the command line when using this setting so that
        /// the preview server serves the site at the same path as the generated links
        /// (for example, <code>--virtual-dir "/virtual/directory"</code>).
        /// </summary>
        /// <type><see cref="string"/></type>
        public const string LinkRoot = nameof(LinkRoot);

        /// <summary>
        /// Indicates whether to hide index pages (as defined by <see cref="IndexFileName"/>) by default when generating links.
        /// </summary>
        /// <type><see cref="bool"/></type>
        public const string LinkHideIndexPages = nameof(LinkHideIndexPages);

        /// <summary>
        /// Indicates whether to hide <see cref="PageFileExtensions"/> (usually ".html" and ".htm" extensions) by default when generating links.
        /// </summary>
        /// <type><see cref="bool"/></type>
        public const string LinkHideExtensions = nameof(LinkHideExtensions);

        /// <summary>
        /// Indicates that links should always be rendered in lowercase.
        /// </summary>
        /// <type><see cref="bool"/></type>
        public const string LinkLowercase = nameof(LinkLowercase);

        /// <summary>
        /// Set to <c>true</c> to fail generation on all warnings and errors. The generation will finish but
        /// an exception will be thrown if any warnings or errors occurred during generation.
        /// </summary>
        public const string Strict = nameof(Strict);

        /// <summary>
        /// This will cause temporary backing files to be created for string document content
        /// instead of storing that content in memory.
        /// </summary>
        /// <type><see cref="bool"/></type>
        public const string UseStringContentFiles = nameof(UseStringContentFiles);

        /// <summary>
        /// Indicates whether caching should be used.
        /// </summary>
        /// <type><see cref="bool"/></type>
        public const string UseCache = nameof(UseCache);

        /// <summary>
        /// The index file name to use for link generation, tree creation, etc. (defaults to "index.html").
        /// </summary>
        /// <type><see cref="string"/></type>
        public const string IndexFileName = nameof(IndexFileName);

        /// <summary>
        /// The file extensions of "pages" (used by <see cref="IExecutionState.OutputPages"/>
        /// to filter output documents (defaults to "htm" and "html").
        /// </summary>
        /// <type>string[]</type>
        public const string PageFileExtensions = nameof(PageFileExtensions);

        /// <summary>
        /// Indicates whether to clean the output path on each execution.
        /// </summary>
        /// <type><see cref="bool"/></type>
        [Obsolete("Use CleanMode instead")]
        public const string CleanOutputPath = nameof(CleanOutputPath);

        /// <summary>
        /// Indicates how to clean the output path on each execution.
        /// </summary>
        /// <type><see cref="CleanMode"/></type>
        public const string CleanMode = nameof(CleanMode);

        /// <summary>
        /// Indicates the culture to use for reading and interpreting dates as input.
        /// </summary>
        /// <type><see cref="string"/> or <see cref="CultureInfo"/></type>
        public const string DateTimeInputCulture = nameof(DateTimeInputCulture);

        /// <summary>
        /// Indicates the culture to use for displaying dates in output.
        /// </summary>
        /// <type><see cref="string"/> or <see cref="CultureInfo"/></type>
        public const string DateTimeDisplayCulture = nameof(DateTimeDisplayCulture);

        /// <summary>
        /// Sets a semantic version range of Statiq Framework that must be used.
        /// </summary>
        public const string MinimumStatiqFrameworkVersion = nameof(MinimumStatiqFrameworkVersion);

        /// <summary>
        /// Specifies analyzers and log levels as "[analyzer]=[log level]" (log level is optional, "All" to set all analyzers).
        /// </summary>
        public const string Analyzers = nameof(Analyzers);

        /// <summary>
        /// The log level at which failures should occur (defaults to LogLevel.Error).
        /// </summary>
        public const string FailureLogLevel = nameof(FailureLogLevel);

        // Document

        /// <summary>
        /// All the children of this node.
        /// </summary>
        /// <type><see cref="IReadOnlyCollection{IDocument}"/></type>
        public const string Children = nameof(Children);

        // SetDestination

        /// <summary>
        /// The extension to use when setting the destination of a document.
        /// </summary>
        /// <type><see cref="string"/></type>
        public const string DestinationExtension = nameof(DestinationExtension);

        /// <summary>
        /// The file name to use when setting the destination of a document.
        /// The destination will be set to the given file name at the same
        /// relative path.
        /// </summary>
        /// <type><see cref="NormalizedPath"/></type>
        public const string DestinationFileName = nameof(DestinationFileName);

        /// <summary>
        /// The path to use when setting the destination of a document.
        /// The specified path can be either relative to the output path or absolute.
        /// </summary>
        /// <type><see cref="NormalizedPath"/></type>
        public const string DestinationPath = nameof(DestinationPath);

        // GroupDocuments

        /// <summary>
        /// The key for the current group.
        /// </summary>
        /// <type><see cref="object"/></type>
        public const string GroupKey = nameof(GroupKey);

        // AddIndexs

        /// <summary>
        /// The one-based index of the current document relative to other documents in the pipeline.
        /// </summary>
        /// <type><see cref="int"/></type>
        public const string Index = nameof(Index);

        // OrderDocuments

        /// <summary>
        /// A loose ordering key used by <c>OrderDocuments</c>.
        /// </summary>
        /// <type><see cref="int"/></type>
        public const string Order = nameof(Order);

        // GenerateSitemap

        /// <summary>
        /// Contains a document-specific sitemap item for use when generating a sitemap.
        /// </summary>
        /// <type><see cref="SitemapItem"/></type>
        public const string SitemapItem = nameof(SitemapItem);

        // ReadWeb

        /// <summary>
        /// The URI where the document was downloaded from.
        /// </summary>
        /// <type><see cref="Uri"/></type>
        public const string SourceUri = nameof(SourceUri);

        /// <summary>
        /// The web headers of the document.
        /// </summary>
        /// <type><c>Dictionary&lt;string, string&gt;</c></type>
        public const string SourceHeaders = nameof(SourceHeaders);

        // CreateTree

        /// <summary>
        /// The path that represents this node in the tree.
        /// </summary>
        /// <type><see cref="Array"/></type>
        public const string TreePath = nameof(TreePath);

        /// <summary>
        /// Gets set on documents that were created as a placeholder for tree roots.
        /// </summary>
        /// <type><see cref="bool"/></type>
        public const string TreePlaceholder = nameof(TreePlaceholder);

        // AddTitle

        /// <summary>
        /// The calculated title of the document.
        /// </summary>
        /// <type><see cref="string"/></type>
        public const string Title = nameof(Title);

        // PaginateDocuments

        public const string Next = nameof(Next);

        public const string Previous = nameof(Previous);

        public const string TotalPages = nameof(TotalPages);

        public const string TotalItems = nameof(TotalItems);

        // GenerateRedirects

        /// <summary>
        /// The path(s) where the document should be redirected from.
        /// </summary>
        /// <type><see cref="NormalizedPath"/></type>
        public const string RedirectFrom = nameof(RedirectFrom);

        /// <summary>
        /// Replaces the default body of meta-refresh redirect HTML files with the specified body
        /// (it will be included raw so don't escape HTML).
        /// </summary>
        public const string RedirectBody = nameof(RedirectBody);

        /// <summary>
        /// Entirely disables cache modules, both during runtime and the persistent cache.
        /// </summary>
        public const string DisableCache = nameof(DisableCache);

        /// <summary>
        /// Resets the cache when set (you generally won't set this as an initial setting, otherwise
        /// the cache will always be reinitialized.
        /// </summary>
        public const string ResetCache = nameof(ResetCache);

        /// <summary>
        /// Indicates that the current metadata object, document, or metadata values should
        /// be excluded from script evaluation typically triggered by a <c>=></c> prefix.
        /// If this value is <c>true</c> no metadata values will be evaluated. If it's a
        /// <see cref="IEnumerable{String}"/> then the indicated metadata values will be
        /// excluded from evaluation.
        /// </summary>
        public const string ExcludeFromEvaluation = nameof(ExcludeFromEvaluation);

        /// <summary>
        /// Holds the current value when enumerating values as documents using the EnumerateValues module.
        /// </summary>
        public const string Current = nameof(Current);

        /// <summary>
        /// The default key used by the EnumerateValues module to locate values to enumerate
        /// within a document.
        /// </summary>
        public const string Enumerate = nameof(Enumerate);

        /// <summary>
        /// Used to indicate that the original input should be output along with the enumerated values
        /// when using the EnumerateValues module.
        /// </summary>
        public const string EnumerateWithInput = nameof(EnumerateWithInput);
    }
}
