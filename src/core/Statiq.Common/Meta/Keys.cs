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
        /// Indicates whether to hide index pages by default when generating links.
        /// </summary>
        /// <type><see cref="bool"/></type>
        public const string LinkHideIndexPages = nameof(LinkHideIndexPages);

        /// <summary>
        /// Indicates whether to hide ".html" and ".htm" extensions by default when generating links.
        /// </summary>
        /// <type><see cref="bool"/></type>
        public const string LinkHideExtensions = nameof(LinkHideExtensions);

        /// <summary>
        /// Indicates that links should always be rendered in lowercase.
        /// </summary>
        /// <type><see cref="bool"/></type>
        public const string LinkLowercase = nameof(LinkLowercase);

        /// <summary>
        /// This will cause temporary backing files to be created for string document content
        /// instead of storing that content in memory.
        /// </summary>
        public const string UseStringContentFiles = nameof(UseStringContentFiles);

        /// <summary>
        /// Indicates whether caching should be used.
        /// </summary>
        /// <type><see cref="bool"/></type>
        public const string UseCache = nameof(UseCache);

        /// <summary>
        /// Indicates whether to clean the output path on each execution.
        /// </summary>
        /// <type><see cref="bool"/></type>
        public const string CleanOutputPath = nameof(CleanOutputPath);

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

        // Destination

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
        /// <type><see cref="FilePath"/></type>
        public const string DestinationFileName = nameof(DestinationFileName);

        /// <summary>
        /// The path to use when setting the destination of a document.
        /// The specified path can be either relative to the output path or absolute.
        /// </summary>
        /// <type><see cref="FilePath"/></type>
        public const string DestinationPath = nameof(DestinationPath);

        // Paginate

        /// <summary>
        /// Contains all the documents for the current page.
        /// </summary>
        /// <type><c>IEnumerable&lt;IDocument&gt;</c></type>
        public const string PageDocuments = nameof(PageDocuments);

        /// <summary>
        /// The index of the current page (1 based).
        /// </summary>
        /// <type><see cref="int"/></type>
        public const string CurrentPage = nameof(CurrentPage);

        /// <summary>
        /// The total number of pages.
        /// </summary>
        /// <type><see cref="int"/></type>
        public const string TotalPages = nameof(TotalPages);

        /// <summary>
        /// The total number of items across all pages.
        /// </summary>
        /// <type><see cref="int"/></type>
        public const string TotalItems = nameof(TotalItems);

        /// <summary>
        /// Whether there is another page after this one.
        /// </summary>
        /// <type><see cref="bool"/></type>
        public const string HasNextPage = nameof(HasNextPage);

        /// <summary>
        /// Whether there is another page before this one.
        /// </summary>
        /// <type><see cref="bool"/></type>
        public const string HasPreviousPage = nameof(HasPreviousPage);

        // GroupBy

        /// <summary>
        /// Contains all the documents for the current group.
        /// </summary>
        /// <type><c>IEnumerable&lt;IDocument&gt;</c></type>
        public const string GroupDocuments = nameof(GroupDocuments);

        /// <summary>
        /// The key for the current group.
        /// </summary>
        /// <type><see cref="object"/></type>
        public const string GroupKey = nameof(GroupKey);

        // Index

        /// <summary>
        /// The one-based index of the current document relative to other documents in the pipeline.
        /// </summary>
        /// <type><see cref="int"/></type>
        public const string Index = nameof(Index);

        // Sitemap

        /// <summary>
        /// Contains a document-specific sitemap item for use when generating a sitemap.
        /// </summary>
        /// <type><see cref="SitemapItem"/></type>
        public const string SitemapItem = nameof(SitemapItem);

        // Download

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

        // Tree

        /// <summary>
        /// The parent of this node or <c>null</c> if it is a root.
        /// </summary>
        /// <type><see cref="IDocument"/></type>
        public const string Parent = nameof(Parent);

        /// <summary>
        /// All the children of this node.
        /// </summary>
        /// <type><see cref="IReadOnlyCollection{IDocument}"/></type>
        public const string Children = nameof(Children);

        /// <summary>
        /// The previous sibling, that is the previous node in the children
        /// collection of the parent or <c>null</c> if this is the first node in the collection or the parent is null.
        /// </summary>
        /// <type><see cref="IDocument"/></type>
        public const string PreviousSibling = nameof(PreviousSibling);

        /// <summary>
        /// The next sibling, that is the next node in the children collection
        /// of the parent or <c>null</c> if this is the last node in the collection or the parent is null.
        /// </summary>
        /// <type><see cref="IDocument"/></type>
        public const string NextSibling = nameof(NextSibling);

        /// <summary>
        /// The next node in the tree using a depth-first
        /// search or <c>null</c> if this was the last node.
        /// </summary>
        /// <type><see cref="IDocument"/></type>
        public const string Next = nameof(Next);

        /// <summary>
        /// The previous node in the tree using a depth-first
        /// search or <c>null</c> if this was the first node.
        /// </summary>
        /// <type><see cref="IDocument"/></type>
        public const string Previous = nameof(Previous);

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

        // Title

        /// <summary>
        /// The calculated title of the document.
        /// </summary>
        /// <type><see cref="string"/></type>
        public const string Title = nameof(Title);

        // RedirectFrom

        /// <summary>
        /// The path(s) where the document should be redirected from.
        /// </summary>
        /// <type><see cref="FilePath"/></type>
        public const string RedirectFrom = nameof(RedirectFrom);

        /// <summary>
        /// Entirely disables cache modules, both during runtime and the persistent cache.
        /// </summary>
        public const string DisableCache = nameof(DisableCache);

        /// <summary>
        /// Resets the cache when set (you generally won't set this as an initial setting, otherwise
        /// the cache will always be reinitialized.
        /// </summary>
        public const string ResetCache = nameof(ResetCache);
    }
}
