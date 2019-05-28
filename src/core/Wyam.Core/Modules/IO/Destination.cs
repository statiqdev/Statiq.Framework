using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Wyam.Common.Configuration;
using Wyam.Common.Documents;
using Wyam.Common.IO;
using Wyam.Common.Meta;
using Wyam.Common.Modules;
using Wyam.Common.Execution;
using Wyam.Common.Tracing;
using Wyam.Common;

namespace Wyam.Core.Modules.IO
{
    /// <summary>
    /// Sets the document destination.
    /// </summary>
    /// <remarks>
    /// This module is typically used before <see cref="WriteFiles"/> to set the
    /// <see cref="IDocument.Destination"/> prior to writing the document.
    /// If an extension is provided, this module will change the extension of the
    /// destination path for each input document.
    /// If the metadata keys <c>DestinationPath</c>, <c>DestinationFileName</c>,
    /// or <c>DestinationExtension</c> are set (in that order of precedence), the value
    /// will be used instead of the specified extension. For example, if you have a bunch
    /// of Razor .cshtml files that need to be rendered to .html files but one of them
    /// should be output as a .xml file instead, define the <c>DestinationExtension</c>
    /// metadata value in the front matter of the page.
    /// If a delegate is provided, it takes precedence over the metadata values (but can
    /// use them if needed).
    /// </remarks>
    /// <metadata cref="Keys.DestinationPath" usage="Input" />
    /// <metadata cref="Keys.DestinationFileName" usage="Input" />
    /// <metadata cref="Keys.DestinationExtension" usage="Input" />
    /// <category>Input/Output</category>
    // TODO: test absolute DestinationPath with empty constructor (existing and null destination)
    // TODO: test relative DestinationPath with empty constructor (existing and null destination)
    // TODO: test DestinationFileName with empty constructor and existing relative destination
    // TODO: test DestinationFileName with empty constructor and existing absolute destination
    // TODO: test DestinationFileName with empty constructor and existing null destination
    // TODO: test DestinationExtension with empty constructor and existing destination
    // TODO: test DestinationExtension with empty constructor and null destination (should still be null after)
    // TODO: tests for extension constructor
    // TODO: test FilePath.ChangeFileName() (and audit for other uses)
    public class Destination : IModule
    {
        private readonly DocumentConfig<FilePath> _destination;

        /// <summary>
        /// Sets the destination of input documents according to the metadata values for
        /// <c>DestinationPath</c>, <c>DestinationFileName</c>, or <c>DestinationExtension</c>
        /// (in that order of precedence). If none of those metadata values are set, the
        /// destination will remain unchanged.
        /// </summary>
        public Destination()
        {
            _destination = Config.FromDocument(GetPathFromMetadata);
        }

        /// <summary>
        /// Changes the destination extension of input documents to the specified extension.
        /// If <c>DestinationPath</c>, <c>DestinationFileName</c>, or <c>DestinationExtension</c>
        /// metadata values are set, those will take precedence.
        /// </summary>
        /// <param name="extension">The extension to set the destination to.</param>
        public Destination(string extension)
        {
            if (extension == null)
            {
                throw new ArgumentNullException(nameof(extension));
            }
            _destination = Config.FromDocument(
                doc => GetPathFromMetadata(doc) ?? doc.Destination?.ChangeExtension(extension));
        }

        /// <summary>
        /// Changes the destination of input documents to that of the delegate.
        /// </summary>
        /// <param name="destination">A delegate that returns a <see cref="FilePath"/> with the desired destination path.</param>
        public Destination(DocumentConfig<FilePath> destination)
        {
            _destination = destination ?? throw new ArgumentNullException(nameof(destination));
        }

        private FilePath GetPathFromMetadata(IDocument doc)
        {
            FilePath path = doc.FilePath(Keys.DestinationPath);
            if (path != null)
            {
                return path;
            }
            path = doc.FilePath(Keys.DestinationFileName);
            if (path != null)
            {
                return doc.Destination == null ? path : doc.Destination.ChangeFileName(path);
            }
            string extension = doc.String(Keys.DestinationExtension);
            if (!string.IsNullOrEmpty(extension) && doc.Destination != null)
            {
                return doc.Destination.ChangeExtension(extension);
            }
            return null;
        }

        /// <inheritdoc />
        public virtual async Task<IEnumerable<IDocument>> ExecuteAsync(IReadOnlyList<IDocument> inputs, IExecutionContext context) =>
            await inputs.ParallelSelectAsync(async input =>
            {
                FilePath destination = await _destination.GetValueAsync(input, context);
                return destination == null ? input : context.GetDocument(input, destination);
            });
    }
}
