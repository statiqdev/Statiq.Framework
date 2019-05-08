using System.Collections.Generic;
using System.Threading.Tasks;
using Wyam.Common.Documents;
using Wyam.Common.Execution;
using Wyam.Common.IO;
using Wyam.Common.Meta;
using Wyam.Common.Shortcodes;
using Wyam.Common.Tracing;

namespace Wyam.Core.Shortcodes.IO
{
    /// <summary>
    /// Includes a file from the virtual file system.
    /// </summary>
    /// <remarks>
    /// The raw content of the file will be rendered where the shortcode appears.
    /// If the file does not exist nothing will be rendered.
    /// </remarks>
    /// <parameter>The path to the file to include.</parameter>
    public class Include : IShortcode
    {
        private FilePath _sourcePath;

        /// <inheritdoc />
        public async Task<IDocument> ExecuteAsync(KeyValuePair<string, string>[] args, string content, IDocument document, IExecutionContext context)
        {
            // Get the included path relative to the document
            FilePath includedPath = new FilePath(args.SingleValue());
            if (_sourcePath == null)
            {
                // Cache the source path for this shortcode instance since it'll be the same for all future shortcodes
                _sourcePath = document.FilePath("IncludeShortcodeSource", document.Source);
            }

            // Try to find the file relative to the current document path
            IFile includedFile = null;
            if (includedPath.IsRelative && _sourcePath != null)
            {
                includedFile = await context.FileSystem.GetFileAsync(_sourcePath.Directory.CombineFile(includedPath));
            }

            // If that didn't work, try relative to the input folder
            if (includedFile == null || !await includedFile.GetExistsAsync())
            {
                includedFile = await context.FileSystem.GetInputFileAsync(includedPath);
            }

            // Get the included file
            if (!await includedFile.GetExistsAsync())
            {
                Trace.Warning($"Included file {includedPath.FullPath} does not exist");
                return context.GetDocument();
            }

            // Set the currently included shortcode source so nested includes can use it
            return context.GetDocument(
                metadata: new MetadataItems
                {
                    { "IncludeShortcodeSource", includedFile.Path.FullPath }
                },
                await context.GetContentProviderAsync(includedFile));
        }
    }
}
