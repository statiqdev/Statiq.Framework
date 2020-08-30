using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Statiq.Common
{
    public static class IExecutionContextGetContentProviderExtensions
    {
        public static IContentProvider GetContentProvider(
            this IExecutionContext executionContext,
            Stream stream) =>
            executionContext.GetContentProvider(stream, null);

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

            // Copy the stream to a buffer and use that for the content
            if (stream.Position != 0)
            {
                stream.Position = 0;
            }
            byte[] buffer = new byte[stream.Length];
            using (MemoryStream bufferStream = new MemoryStream(buffer))
            {
                stream.CopyTo(bufferStream);
            }
            return new MemoryContent(buffer, mediaType);
        }

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

        public static Task<IContentProvider> GetContentProviderAsync(
            this IExecutionContext executionContext,
            string content) =>
            executionContext.GetContentProviderAsync(content, null);

        public static async Task<IContentProvider> GetContentProviderAsync(
            this IExecutionContext executionContext,
            string content,
            string mediaType)
        {
            if (content is null)
            {
                return new NullContent();
            }

            if (executionContext.Settings.GetBool(Keys.UseStringContentFiles))
            {
                // Use a temp file for strings
                IFile tempFile = executionContext.FileSystem.GetTempFile();
                if (!string.IsNullOrEmpty(content))
                {
                    await tempFile.WriteAllTextAsync(content, cancellationToken: executionContext.CancellationToken);
                }
                return new FileContent(tempFile, mediaType);
            }

            // Otherwise get a memory stream from the pool and use that
            byte[] contentBytes = Encoding.UTF8.GetBytes(content);
            return new MemoryContent(contentBytes, mediaType);
        }
    }
}
