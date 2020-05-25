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
            // Special case if this is a content stream
            if (stream is ContentStream contentStream)
            {
                return contentStream.GetContentProvider(mediaType);
            }

            return stream == null ? null : new StreamContent(executionContext.MemoryStreamFactory, stream, mediaType);
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

        public static Task<IContentProvider> GetContentProviderAsync(
            this IExecutionContext executionContext,
            string content) =>
            executionContext.GetContentProviderAsync(content, null);

        public static async Task<IContentProvider> GetContentProviderAsync(
            this IExecutionContext executionContext,
            string content,
            string mediaType)
        {
            if (content == null)
            {
                return null;
            }

            if (executionContext.Settings.GetBool(Keys.UseStringContentFiles))
            {
                // Use a temp file for strings
                IFile tempFile = executionContext.FileSystem.GetTempFile();
                if (!string.IsNullOrEmpty(content))
                {
                    await tempFile.WriteAllTextAsync(content);
                }
                return new FileContent(tempFile, mediaType);
            }

            // Otherwise get a memory stream from the pool and use that
            byte[] contentBytes = Encoding.UTF8.GetBytes(content);
            return new MemoryContent(contentBytes, mediaType);
        }
    }
}
