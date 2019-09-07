using System;
using System.IO;
using System.Threading.Tasks;

namespace Statiq.Common
{
    public partial interface IExecutionContext
    {
        public IContentProvider GetContentProvider(IContentProviderFactory factory) => factory?.GetContentProvider();

        public IContentProvider GetContentProvider(Stream stream)
        {
            // Special case if this is a content stream (which implements IContentProviderFactory)
            if (stream is IContentProviderFactory factory)
            {
                return GetContentProvider(factory);
            }

            return stream == null ? null : new StreamContent(MemoryStreamFactory, stream);
        }

        public async Task<IContentProvider> GetContentProviderAsync(string content)
        {
            if (string.IsNullOrEmpty(content))
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
                return new FileContent(tempFile);
            }

            // Otherwise get a memory stream from the pool and use that
            return new StreamContent(MemoryStreamFactory, MemoryStreamFactory.GetStream(content));
        }
    }
}
