using System;
using System.IO;
using Statiq.Common;

namespace Statiq.Common
{
    /// <summary>
    /// A special writable <see cref="Stream"/> that can be used when creating new content
    /// that provides an appropriate <see cref="IContentProvider"/>.
    /// Instances of this stream should be disposed when writing is complete.
    /// </summary>
    internal class ContentStream : DelegatingStream, IContentProviderFactory
    {
        private readonly Func<string, IContentProvider> _contentProviderFactory;
        private bool _disposeStream;

        public ContentStream(Func<string, IContentProvider> contentProviderFactory, Stream stream, bool disposeStream)
            : base(stream)
        {
            _contentProviderFactory = contentProviderFactory;
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
        public IContentProvider GetContentProvider(string mediaType)
        {
            Dispose();
            return _contentProviderFactory(mediaType);
        }

        public IContentProvider GetContentProvider() => GetContentProvider(null);
    }
}
