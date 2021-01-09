using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Statiq.Common
{
    public static class IExecutionContextGetContentProviderExtensions
    {
        /// <summary>
        /// creates a content provider from a <see cref="Stream"/> by reading the stream into
        /// a buffer and then providing that buffer when content is requested.
        /// </summary>
        /// <param name="executionContext">The execution context.</param>
        /// <param name="stream">A seekable stream from which to buffer content.</param>
        public static IContentProvider GetContentProvider(
            this IExecutionContext executionContext,
            Stream stream) =>
            executionContext.GetContentProvider(stream, null);

        /// <summary>
        /// creates a content provider from a <see cref="Stream"/> by reading the stream into
        /// a buffer and then providing that buffer when content is requested.
        /// </summary>
        /// <param name="executionContext">The execution context.</param>
        /// <param name="stream">A seekable stream from which to buffer content.</param>
        /// <param name="mediaType">The media type of the content provider.</param>
        public static IContentProvider GetContentProvider(
            this IExecutionContext executionContext,
            Stream stream,
            string mediaType)
        {
            // Return null if the stream is null
            if (stream is null)
            {
                return new NullContent();
            }

            // Special case if this is a content stream
            if (stream is ContentStream contentStream)
            {
                return contentStream.GetContentProvider(mediaType);
            }

            // Special case if this is a string stream
            if (stream is StringStream stringStream)
            {
                return new StringContent(stringStream.String, mediaType);
            }

            // Special case if this is a memory stream
            if (stream is MemoryStream memoryStream && memoryStream.TryGetBuffer(out ArraySegment<byte> segment))
            {
                return new MemoryContent(segment.Array, segment.Offset, segment.Count, mediaType);
            }

            // Copy the stream to a buffer and use that for the content
            if (stream.CanSeek)
            {
                stream.Position = 0;
            }
            using (MemoryStream bufferStream = stream.CanSeek ? new MemoryStream((int)stream.Length) : executionContext.MemoryStreamFactory.GetStream())
            {
                stream.CopyTo(bufferStream);

                // First try getting a buffer segment
                if (bufferStream.TryGetBuffer(out ArraySegment<byte> bufferSegment))
                {
                    return new MemoryContent(bufferSegment.Array, bufferSegment.Offset, bufferSegment.Count, mediaType);
                }

                // If that fails, copy it to an array
                byte[] buffer = bufferStream.ToArray();
                return new MemoryContent(buffer, mediaType);
            }
        }

        /// <summary>
        /// Creates a content provider from a delegate that returns a <see cref="Stream"/>
        /// to use on each content request. A new stream should be returned on each request
        /// since it may be read concurrently.
        /// </summary>
        public static IContentProvider GetContentProvider(
            this IExecutionContext executionContext,
            Func<Stream> getStream) =>
            executionContext.GetContentProvider(getStream, null);

        /// <summary>
        /// Creates a content provider from a delegate that returns a <see cref="Stream"/>
        /// to use on each content request. A new stream should be returned on each request
        /// since it may be read concurrently.
        /// </summary>
        public static IContentProvider GetContentProvider(
            this IExecutionContext executionContext,
            Func<Stream> getStream,
            string mediaType) =>
            new DelegateContent(getStream, mediaType);

        public static IContentProvider GetContentProvider(
            this IExecutionContext executionContext,
            byte[] buffer) =>
            executionContext.GetContentProvider(buffer, null);

        public static IContentProvider GetContentProvider(
            this IExecutionContext executionContext,
            byte[] buffer,
            string mediaType) =>
            new MemoryContent(buffer, mediaType);

        public static IContentProvider GetContentProvider(
            this IExecutionContext executionContext,
            byte[] buffer,
            int index,
            int count) =>
            executionContext.GetContentProvider(buffer, index, count, null);

        public static IContentProvider GetContentProvider(
            this IExecutionContext executionContext,
            byte[] buffer,
            int index,
            int count,
            string mediaType) =>
            new MemoryContent(buffer, index, count, mediaType);

        public static IContentProvider GetContentProvider(
            this IExecutionContext executionContext,
            string content) =>
            executionContext.GetContentProvider(content, null);

        public static IContentProvider GetContentProvider(
            this IExecutionContext executionContext,
            string content,
            string mediaType) => content is null ? new NullContent() : (IContentProvider)new StringContent(content, mediaType);
    }
}
