using System;
using System.IO;
using System.Threading.Tasks;

namespace Statiq.Common
{
    public partial interface IExecutionContext
    {
        public IContentProvider GetContentProvider(Stream stream) => GetContentProvider(stream, null);

        public IContentProvider GetContentProvider(Stream stream, string mediaType)
        {
            // Special case if this is a content stream
            if (stream is ContentStream contentStream)
            {
                return contentStream.GetContentProvider(mediaType);
            }

            return stream == null ? null : new StreamContent(MemoryStreamFactory, stream, mediaType);
        }

        public Task<IContentProvider> GetContentProviderAsync(string content) => GetContentProviderAsync(content, null);

        public async Task<IContentProvider> GetContentProviderAsync(string content, string mediaType)
        {
            if (content == null)
            {
                return null;
            }

            if (Settings.GetBool(Keys.UseStringContentFiles))
            {
                // Use a temp file for strings
                IFile tempFile = FileSystem.GetTempFile();
                if (!string.IsNullOrEmpty(content))
                {
                    await tempFile.WriteAllTextAsync(content);
                }
                return new FileContent(tempFile, mediaType);
            }

            // Otherwise get a memory stream from the pool and use that
            return new StreamContent(MemoryStreamFactory, MemoryStreamFactory.GetStream(content), mediaType);
        }
    }
}
