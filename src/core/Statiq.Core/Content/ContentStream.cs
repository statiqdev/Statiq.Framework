using System.IO;
using Statiq.Common.Content;
using Statiq.Common.Execution;
using Statiq.Common.Util;
using Statiq.Core.Documents;

namespace Statiq.Core.Content
{
    /// <summary>
    /// A special writable <see cref="Stream"/> that can be used when creating new content
    /// that provides an appropriate <see cref="IContentProvider"/>.
    /// Instances of this stream should be disposed when writing is complete.
    /// </summary>
    internal class ContentStream : DelegatingStream, IContentProviderFactory
    {
        private readonly IContentProvider _contentProvider;
        private bool _disposeStream;

        public ContentStream(IContentProvider contentProvider, Stream stream, bool disposeStream)
            : base(stream)
        {
            _contentProvider = contentProvider;
            _disposeStream = disposeStream;
        }

        protected override void Dispose(bool disposing)
        {
            if (_disposeStream)
            {
                Stream.Dispose();
                _disposeStream = false;
            }
        }

        /// <summary>
        /// Gets the content provider and disposes the underlying writable stream (if not already).
        /// </summary>
        /// <returns>The content provider to use with a document.</returns>
        public IContentProvider GetContentProvider()
        {
            Dispose();
            return _contentProvider;
        }
    }
}
