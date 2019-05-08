using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Wyam.Common.Content;
using Wyam.Common.Documents;
using Wyam.Common.IO;
using Wyam.Common.Meta;
using Wyam.Common.Execution;
using Wyam.Core.Content;
using Wyam.Core.Modules;
using Wyam.Core.Meta;

namespace Wyam.Core.Documents
{
    internal class DocumentFactory : IDocumentFactory
    {
        private readonly MetadataDictionary _settings;

        public DocumentFactory(MetadataDictionary settings)
        {
            _settings = settings;
        }

        public async Task<IDocument> GetDocumentAsync(
            IExecutionContext context,
            IDocument sourceDocument,
            FilePath source,
            IEnumerable<KeyValuePair<string, object>> metadata,
            object content)
        {
            IContentProvider contentProvider = await GetContentProviderAsync(context, content);
            if (sourceDocument == null || ModuleExtensions.AsNewDocumentModules.Contains(context.Module))
            {
                return new Document(_settings, source, contentProvider, metadata);
            }
            return new Document((Document)sourceDocument, source, contentProvider, metadata);
        }

        private static async Task<IContentProvider> GetContentProviderAsync(IExecutionContext context, object content)
        {
            switch (content)
            {
                case null:
                    return null;
                case IContentProvider contentProvider:
                    return contentProvider;
                case ContentStream contentStream:
                    return contentStream.GetContentProvider();  // This will also dispose the writable stream
                case Stream stream:
                    return new StreamContent(context, stream);
                case IFile file:
                    return new FileContent(file);
                case Document document:
                    return document.ContentProvider;
            }

            // This wasn't one of the known content types, so treat it as a string
            string contentString = content as string ?? content.ToString();

            if (string.IsNullOrEmpty(contentString))
            {
                return null;
            }

            if (context.Bool(Keys.UseStringContentFiles))
            {
                // Use a temp file for strings
                IFile tempFile = await context.FileSystem.GetTempFileAsync();
                if (!string.IsNullOrEmpty(contentString))
                {
                    await tempFile.WriteAllTextAsync(contentString);
                }
                return new TempFileContent(tempFile);
            }

            // Otherwise get a memory stream from the pool and use that
            return new StreamContent(context, context.MemoryStreamManager.GetStream(contentString));
        }
    }
}
