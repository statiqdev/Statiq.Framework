using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Statiq.Common;

namespace Statiq.Core
{
    /// <summary>
    /// Includes a file from the virtual file system.
    /// </summary>
    /// <remarks>
    /// The raw content of the file will be rendered where the shortcode appears.
    /// If the file does not exist nothing will be rendered.
    /// </remarks>
    /// <example>
    /// <para>Example usage to show the contents of test-include.html in the output</para>
    /// <para>
    /// <code>
    /// &lt;?# Include "test-include.html" /?&gt;
    /// </code>
    /// </para>
    /// <para>
    /// If the included file contains Markdown syntax, you can even include it before the Markdown engine runs with a slight syntax change:
    /// </para>
    /// <para>
    /// <code>
    /// &lt;?! Include "test-include.md" /?&gt;?
    /// </code>
    /// </para>
    /// </example>
    /// <parameter>The path to the file to include.</parameter>
    public class IncludeShortcode : SyncShortcode
    {
        private NormalizedPath _sourcePath = NormalizedPath.Null;

        /// <inheritdoc />
        public override IDocument Execute(KeyValuePair<string, string>[] args, string content, IDocument document, IExecutionContext context)
        {
            // Get the included path relative to the document
            NormalizedPath includedPath = new NormalizedPath(args.SingleValue());
            if (_sourcePath.IsNull)
            {
                // Cache the source path for this shortcode instance since it'll be the same for all future shortcodes
                _sourcePath = document.GetPath("IncludeShortcodeSource", document.Source);
            }

            // Try to find the file relative to the current document path
            IFile includedFile = null;
            if (includedPath.IsRelative && !_sourcePath.IsNull)
            {
                includedFile = context.FileSystem.GetFile(_sourcePath.ChangeFileName(includedPath));
            }

            // If that didn't work, try relative to the input folder
            if (includedFile?.Exists != true)
            {
                includedFile = context.FileSystem.GetInputFile(includedPath);
            }

            // Get the included file
            if (!includedFile.Exists)
            {
                context.LogWarning($"Included file {includedPath.FullPath} does not exist");
                return context.CreateDocument();
            }

            // Set the currently included shortcode source so nested includes can use it
            return context.CreateDocument(
                new MetadataItems
                {
                    { "IncludeShortcodeSource", includedFile.Path.FullPath }
                },
                includedFile.GetContentProvider());
        }
    }
}
