using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Statiq.Common;
using Statiq.Common.Content;
using Statiq.Common.Documents;
using Statiq.Common.Execution;
using Statiq.Common.Modules;
using Statiq.Common.Shortcodes;
using Statiq.Core.Shortcodes;

namespace Statiq.Core.Modules.Contents
{
    /// <summary>
    /// Renders shortcodes in the input documents.
    /// </summary>
    /// <category>Content</category>
    public class Shortcodes : IModule
    {
        private readonly string _startDelimiter;
        private readonly string _endDelimiter;

        /// <summary>
        /// Renders shortcodes in the input documents using the default start and end delimiters.
        /// </summary>
        /// <param name="preRender">
        /// Indicates if the module is being executed pre-template-rendering (<c>true</c>)
        /// or post-template-rendering (<c>false</c>). The default delimiters are different
        /// depending on when the module is executed.
        /// </param>
        public Shortcodes(bool preRender = false)
        {
            if (preRender)
            {
                _startDelimiter = ShortcodeParser.DefaultPreRenderStartDelimiter;
                _endDelimiter = ShortcodeParser.DefaultPreRenderEndDelimiter;
            }
            else
            {
                _startDelimiter = ShortcodeParser.DefaultPostRenderStartDelimiter;
                _endDelimiter = ShortcodeParser.DefaultPostRenderEndDelimiter;
            }
        }

        /// <summary>
        /// Renders shortcodes in the input documents using custom start and end delimiters.
        /// </summary>
        /// <param name="startDelimiter">The shortcode start delimiter.</param>
        /// <param name="endDelimiter">The shortcode end delimiter.</param>
        public Shortcodes(string startDelimiter, string endDelimiter)
        {
            _startDelimiter = startDelimiter;
            _endDelimiter = endDelimiter;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<IDocument>> ExecuteAsync(IReadOnlyList<IDocument> inputs, IExecutionContext context) =>
            await inputs.ParallelSelectAsync(context, async input => await ProcessShortcodesAsync(input, context) ?? input);

        // The inputStream will be disposed if this returns a result document but will not otherwise
        private async Task<IDocument> ProcessShortcodesAsync(IDocument input, IExecutionContext context)
        {
            // Parse the input stream looking for shortcodes
            ShortcodeParser parser = new ShortcodeParser(_startDelimiter, _endDelimiter, context.Shortcodes);
            List<ShortcodeLocation> locations;
            using (Stream inputStream = await input.GetStreamAsync())
            {
                locations = parser.Parse(inputStream);
            }

            // Return the original document if we didn't find any
            if (locations.Count == 0)
            {
                return null;
            }

            // Otherwise, create a shortcode instance for each named shortcode
            ImmutableDictionary<string, IShortcode> shortcodes =
                locations
                    .Select(x => x.Name)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToImmutableDictionary(x => x, x => context.Shortcodes.CreateInstance(x), StringComparer.OrdinalIgnoreCase);

            // Execute each of the shortcodes in order
            List<InsertingStreamLocation> insertingLocations = new List<InsertingStreamLocation>();
            foreach (ShortcodeLocation location in locations)
            {
                InsertingStreamLocation insertingLocation = await ExecuteShortcodesAsync(input, context, location, shortcodes);
                insertingLocations.Add(insertingLocation);

                // Accumulate metadata for the following shortcodes
                if (insertingLocation.Document != null)
                {
                    input = context.GetDocument(input, insertingLocation.Document);
                }
            }

            // Dispose any shortcodes that implement IDisposable
            foreach (IDisposable disposableShortcode
                in shortcodes.Values.Select(x => x as IDisposable).Where(x => x != null))
            {
                disposableShortcode.Dispose();
            }

            // Construct a new stream with the shortcode results inserted
            // We have to use all TextWriter/TextReaders over the streams to ensure consistent encoding
            Stream resultStream = context.MemoryStreamFactory.GetStream();
            char[] buffer = new char[4096];
            using (TextWriter writer = new StreamWriter(resultStream, Encoding.UTF8, 4096, true))
            {
                // The input stream will get disposed when the reader is
                using (TextReader reader = new StreamReader(await input.GetStreamAsync()))
                {
                    int position = 0;
                    int length = 0;
                    foreach (InsertingStreamLocation insertingLocation in insertingLocations)
                    {
                        // Copy up to the start of this shortcode
                        length = insertingLocation.FirstIndex - position;
                        Read(reader, writer, length, ref buffer);
                        position += length;

                        // Copy the shortcode content to the result stream
                        if (insertingLocation.Document != null)
                        {
                            // This will dispose the shortcode content stream when done
                            using (TextReader insertingReader = new StreamReader(await insertingLocation.Document.GetStreamAsync()))
                            {
                                Read(insertingReader, writer, null, ref buffer);
                            }

                            // Merge shortcode metadata with the current document
                            input = context.GetDocument(input, insertingLocation.Document);
                        }

                        // Skip the shortcode text
                        length = insertingLocation.LastIndex - insertingLocation.FirstIndex + 1;
                        Read(reader, null, length, ref buffer);
                        position += length;
                    }

                    // Copy remaining
                    Read(reader, writer, null, ref buffer);
                }
            }
            return context.GetDocument(input, context.GetContentProvider(resultStream));
        }

        private async Task<InsertingStreamLocation> ExecuteShortcodesAsync(
            IDocument input,
            IExecutionContext context,
            ShortcodeLocation location,
            ImmutableDictionary<string, IShortcode> shortcodes)
        {
            // Execute the shortcode
            IDocument shortcodeResult = await shortcodes[location.Name].ExecuteAsync(location.Arguments, location.Content, input, context);
            IDocument mergedResult = null;

            if (shortcodeResult != null)
            {
                if (shortcodeResult.HasContent)
                {
                    // Merge output metadata with the current input document
                    // Creating a new document is the easiest way to ensure all the metadata from shortcodes gets accumulated correctly
                    mergedResult = context.GetDocument(input, shortcodeResult, shortcodeResult.ContentProvider);

                    // Don't process nested shortcodes if it's the raw shortcode
                    if (!location.Name.Equals(nameof(Core.Shortcodes.Contents.Raw), StringComparison.OrdinalIgnoreCase))
                    {
                        // Recursively parse shortcodes
                        IDocument nestedResult = await ProcessShortcodesAsync(mergedResult, context);
                        if (nestedResult != null)
                        {
                            mergedResult = nestedResult;
                        }
                    }
                    return new InsertingStreamLocation(location.FirstIndex, location.LastIndex, mergedResult);
                }
                else
                {
                    // Don't copy the content provider from the input document if the shortcode doesn't have content
                    mergedResult = context.GetDocument(input, shortcodeResult, NullContent.Provider);
                }
            }

            return new InsertingStreamLocation(location.FirstIndex, location.LastIndex, mergedResult);
        }

        // writer = null to just skip length in reader
        // length = null to read to the end of reader
        private static void Read(TextReader reader, TextWriter writer, int? length, ref char[] buffer)
        {
            while (!length.HasValue || length > 0)
            {
                int count = reader.ReadBlock(buffer, 0, !length.HasValue || length > buffer.Length ? buffer.Length : length.Value);
                if (count > 0)
                {
                    if (length.HasValue)
                    {
                        length -= count;
                    }
                    writer?.Write(buffer, 0, count);
                    writer?.Flush();
                }
                else
                {
                    break;
                }
            }
        }

        private class InsertingStreamLocation
        {
            public InsertingStreamLocation(int firstIndex, int lastIndex, IDocument document)
            {
                FirstIndex = firstIndex;
                LastIndex = lastIndex;
                Document = document;
            }

            public int FirstIndex { get; }
            public int LastIndex { get; }
            public IDocument Document { get; }
        }
    }
}
