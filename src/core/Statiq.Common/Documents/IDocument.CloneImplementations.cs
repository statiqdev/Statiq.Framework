using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Statiq.Common;

namespace Statiq.Common
{
    public partial interface IDocument
    {
        /// <summary>
        /// Clones this document.
        /// </summary>
        /// <param name="destination">The new destination or <c>null</c> to keep the existing destination.</param>
        /// <param name="items">New metadata items.</param>
        /// <param name="contentProvider">The new content provider or <c>null</c> to keep the existing content provider.</param>
        /// <returns>A new document of the same type as this document.</returns>
        public IDocument Clone(
            FilePath destination,
            IEnumerable<KeyValuePair<string, object>> items,
            IContentProvider contentProvider = null) =>
            Clone(null, destination, items, contentProvider);

        /// <summary>
        /// Clones this document.
        /// </summary>
        /// <param name="source">The new source. If this document already contains a source, then it's used and this is ignored.</param>
        /// <param name="destination">The new destination or <c>null</c> to keep the existing destination.</param>
        /// <param name="contentProvider">The new content provider or <c>null</c> to keep the existing content provider.</param>
        /// <returns>A new document of the same type as this document.</returns>
        public IDocument Clone(
            FilePath source,
            FilePath destination,
            IContentProvider contentProvider = null) =>
            Clone(source, destination, null, contentProvider);

        /// <summary>
        /// Clones this document.
        /// </summary>
        /// <param name="destination">The new destination or <c>null</c> to keep the existing destination.</param>
        /// <param name="contentProvider">The new content provider or <c>null</c> to keep the existing content provider.</param>
        /// <returns>A new document of the same type as this document.</returns>
        public IDocument Clone(
            FilePath destination,
            IContentProvider contentProvider = null) =>
            Clone(null, destination, null, contentProvider);

        /// <summary>
        /// Clones this document.
        /// </summary>
        /// <param name="items">New metadata items or <c>null</c> not to add any new metadata.</param>
        /// <param name="contentProvider">The new content provider or <c>null</c> to keep the existing content provider.</param>
        /// <returns>A new document of the same type as this document.</returns>
        public IDocument Clone(
            IEnumerable<KeyValuePair<string, object>> items,
            IContentProvider contentProvider = null) =>
            Clone(null, null, items, contentProvider);

        /// <summary>
        /// Clones this document.
        /// </summary>
        /// <param name="contentProvider">The new content provider or <c>null</c> to keep the existing content provider.</param>
        /// <returns>A new document of the same type as this document.</returns>
        public IDocument Clone(IContentProvider contentProvider) =>
            Clone(null, null, null, contentProvider);
    }
}
