using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Wyam.Common.Configuration;
using Wyam.Common.Documents;
using Wyam.Common.IO;
using Wyam.Common.Meta;
using Wyam.Common.Modules;
using Wyam.Common.Execution;
using Wyam.Common.Tracing;
using Wyam.Common;

namespace Wyam.Core.Modules.IO
{
    /// <summary>
    /// Writes the content of each input document to the file system.
    /// </summary>
    /// <remarks>
    /// Writes files to the location specified by <see cref="IDocument.Destination"/>.
    /// If the destination path is relative, the document will be written to the output
    /// folder at the relative location. If the destination path is absolute, the document
    /// will be written to the absolute location. Use the <see cref="Destination"/> module
    /// to set the document destination prior to using this module.
    /// </remarks>
    /// <category>Input/Output</category>
    public class WriteFiles : IModule
    {
        private bool _ignoreEmptyContent = true;
        private bool _append;
        private DocumentConfig<bool> _predicate = true;

        /// <summary>
        /// Ignores documents with empty content, which is the default behavior.
        /// </summary>
        /// <param name="ignoreEmptyContent">If set to <c>true</c>, documents with empty content will be ignored.</param>
        /// <returns>The current module instance.</returns>
        public WriteFiles IgnoreEmptyContent(bool ignoreEmptyContent = true)
        {
            _ignoreEmptyContent = ignoreEmptyContent;
            return this;
        }

        /// <summary>
        /// Appends content to each file instead of overwriting them.
        /// </summary>
        /// <param name="append">Appends to existing files if set to <c>true</c>.</param>
        /// <returns>The current module instance.</returns>
        public WriteFiles Append(bool append = true)
        {
            _append = append;
            return this;
        }

        /// <summary>
        /// Specifies a predicate that must be satisfied for the file to be written.
        /// </summary>
        /// <param name="predicate">A predicate that returns <c>true</c> if the file should be written.</param>
        /// <returns>The current module instance.</returns>
        public WriteFiles Where(DocumentConfig<bool> predicate)
        {
            _predicate = _predicate.CombineWith(predicate);
            return this;
        }

        /// <summary>
        /// Checks whether the input document should be processed.
        /// </summary>
        /// <param name="input">The input document to check/</param>
        /// <param name="context">The execution context.</param>
        /// <returns><c>true</c> if the input document should be processed, <c>false</c> otherwise.</returns>
        protected Task<bool> ShouldProcessAsync(IDocument input, IExecutionContext context) => _predicate.GetValueAsync(input, context);

        /// <inheritdoc />
        public virtual async Task<IEnumerable<IDocument>> ExecuteAsync(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            // Get the output file path for each file in sequence and set up action chains
            // Value = input source string(s) (for reporting a warning if not appending), write action
            Dictionary<FilePath, Tuple<List<string>, Func<Task>>> writesBySource = new Dictionary<FilePath, Tuple<List<string>, Func<Task>>>();
            await context.ForEachAsync(inputs, WriteFilesAsync);

            // Display a warning for any duplicated outputs if not appending
            if (!_append)
            {
                foreach (KeyValuePair<FilePath, Tuple<List<string>, Func<Task>>> kvp in writesBySource.Where(x => x.Value.Item1.Count > 1))
                {
                    string inputSources = Environment.NewLine + "  " + string.Join(Environment.NewLine + "  ", kvp.Value.Item1);
                    Trace.Warning($"Multiple documents output to {kvp.Key} (this probably wasn't intended):{inputSources}");
                }
            }

            // Run the write actions in parallel
            await writesBySource.Values.ParallelForEachAsync(async x => await x.Item2());

            // Return the input documents
            return inputs;

            async Task WriteFilesAsync(IDocument input)
            {
                if (await ShouldProcessAsync(input, context) && input.Destination != null)
                {
                    if (writesBySource.TryGetValue(input.Destination, out Tuple<List<string>, Func<Task>> value))
                    {
                        // This output source was already seen so nest the previous write action in a new one
                        value.Item1.Add(input.Source.ToDisplayString());
                        Func<Task> previousWrite = value.Item2;
                        value = new Tuple<List<string>, Func<Task>>(
                            value.Item1,
                            async () =>
                            {
                                // Complete the previous write, then do the next one
                                await previousWrite();
                                await WriteAsync(input, context, input.Destination);
                            });
                    }
                    else
                    {
                        value = new Tuple<List<string>, Func<Task>>(
                            new List<string> { input.Source.ToDisplayString() },
                            async () => await WriteAsync(input, context, input.Destination));
                    }
                    writesBySource[input.Destination] = value;
                }
            }
        }

        private async Task WriteAsync(IDocument input, IExecutionContext context, FilePath outputPath)
        {
            IFile outputFile = await context.FileSystem.GetOutputFileAsync(outputPath);
            if (outputFile != null)
            {
                using (Stream inputStream = await input.GetStreamAsync())
                {
                    if (_ignoreEmptyContent && inputStream.Length == 0)
                    {
                        return;
                    }

                    using (Stream outputStream = _append ? await outputFile.OpenAppendAsync() : await outputFile.OpenWriteAsync())
                    {
                        await inputStream.CopyToAsync(outputStream);
                        if (!_append)
                        {
                            outputStream.SetLength(inputStream.Length);
                        }
                    }
                }
                Trace.Verbose($"Wrote file {outputFile.Path.FullPath} from {input.Source.ToDisplayString()}");
            }
        }
    }
}
