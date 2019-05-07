using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Wyam.Common.Documents;
using Wyam.Common.Documents.Content;
using Wyam.Common.IO;
using Wyam.Common.Meta;

namespace Wyam.Common.Execution
{
    public static class IExecutionContextGetContentExtensions
    {
        /// <summary>
        /// Gets a document content provider for string content.
        /// </summary>
        /// <param name="context">The current execution context.</param>
        /// <param name="content">The content.</param>
        /// <returns>A <see cref="IContentProvider"/> for use when creating a document.</returns>
        public static async Task<IContentProvider> GetContentAsync(this IExecutionContext context, string content)
        {
            if (string.IsNullOrEmpty(content))
            {
                return null;
            }

            if (context.Bool(Keys.UseStringContentFiles))
            {
                IFile tempFile = await context.FileSystem.GetTempFileAsync();
                if (!string.IsNullOrEmpty(content))
                {
                    await tempFile.WriteAllTextAsync(content);
                }
                return new StringFileContent(tempFile);
            }

            byte[] contentBytes = Encoding.UTF8.GetBytes(content);
            MemoryStream contentStream = context.MemoryStreamManager.GetStream(contentBytes, 0, contentBytes.Length);
            return new StreamContent(context, contentStream);
        }

        /// <summary>
        /// Gets a document content provider for stream content.
        /// If <paramref name="disposeStream"/> is true (which it is by default), the provided
        /// <see cref="Stream"/> will automatically be disposed when the document is disposed (I.e., the
        /// document takes ownership of the <see cref="Stream"/>).
        /// </summary>
        /// <param name="context">The current execution context.</param>
        /// <param name="contentStream">The stream that contains content.</param>
        /// <param name="disposeStream">If <c>true</c>, the provided <see cref="Stream"/> is disposed when no longer used by documents.</param>
        /// <param name="synchronized">If <c>true</c>, access to the provided stream will be synchronized so that only one caller can access it at a time.</param>
        /// <returns>A <see cref="IContentProvider"/> for use when creating a document.</returns>
        public static IContentProvider GetContent(
            this IExecutionContext context,
            Stream contentStream,
            bool disposeStream = true,
            bool synchronized = true) =>
            new StreamContent(context, contentStream, disposeStream, synchronized);

        /// <summary>
        /// Gets a document content provider for string content.
        /// </summary>
        /// <param name="context">The current execution context.</param>
        /// <param name="contentFile">The content.</param>
        /// <returns>A <see cref="IContentProvider"/> for use when creating a document.</returns>
        public static IContentProvider GetContent(this IExecutionContext context, IFile contentFile) => new FileContent(contentFile);
    }
}
