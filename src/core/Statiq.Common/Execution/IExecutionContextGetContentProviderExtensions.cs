using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Statiq.Common.Content;
using Statiq.Common.IO;
using Statiq.Common.Meta;

namespace Statiq.Common.Execution
{
    public static class IExecutionContextGetContentProviderExtensions
    {
        public static IContentProvider GetContentProvider(this IExecutionContext context, IContentProviderFactory factory)
        {
            _ = context ?? throw new ArgumentNullException(nameof(context));
            return factory?.GetContentProvider();
        }

        public static IContentProvider GetContentProvider(this IExecutionContext context, Stream stream)
        {
            _ = context ?? throw new ArgumentNullException(nameof(context));

            // Special case if this is a content stream (which implements IContentProviderFactory)
            if (stream is IContentProviderFactory factory)
            {
                return context.GetContentProvider(factory);
            }

            return stream == null ? null : new StreamContent(context.MemoryStreamFactory, stream);
        }

        public static async Task<IContentProvider> GetContentProviderAsync(this IExecutionContext context, string content)
        {
            _ = context ?? throw new ArgumentNullException(nameof(context));

            if (string.IsNullOrEmpty(content))
            {
                return null;
            }

            if (context.Bool(Keys.UseStringContentFiles))
            {
                // Use a temp file for strings
                IFile tempFile = await context.FileSystem.GetTempFileAsync();
                if (!string.IsNullOrEmpty(content))
                {
                    await tempFile.WriteAllTextAsync(content);
                }
                return new FileContent(tempFile);
            }

            // Otherwise get a memory stream from the pool and use that
            return new StreamContent(context.MemoryStreamFactory, context.MemoryStreamFactory.GetStream(content));
        }
    }
}
