using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Statiq.Common;

namespace Statiq.Core
{
    /// <summary>
    /// Filters the current sequence of documents by source.
    /// </summary>
    /// <remarks>
    /// This module filters documents using "or" logic. If you want to also apply
    /// "and" conditions, place additional <see cref="FilterSources"/> modules
    /// after this one.
    /// </remarks>
    /// <category>Control</category>
    public class FilterSources : IModule
    {
        private IEnumerable<string> _patterns;

        /// <summary>
        /// Filters input document by using globbing pattern(s) on the document source.
        /// </summary>
        /// <param name="patterns">The globbing patterns to apply.</param>
        public FilterSources(params string[] patterns)
            : this((IEnumerable<string>)patterns)
        {
        }

        /// <summary>
        /// Filters input document by using globbing pattern(s) on the document source.
        /// </summary>
        /// <param name="patterns">The globbing patterns to apply.</param>
        public FilterSources(IEnumerable<string> patterns)
        {
            _patterns = patterns ?? throw new ArgumentNullException(nameof(patterns));
        }

        /// <inheritdoc />
        public async Task<IEnumerable<IDocument>> ExecuteAsync(IExecutionContext context)
        {
            DocumentFileProvider fileProvider = new DocumentFileProvider(context.Inputs);
            IEnumerable<IDirectory> directories = await
                (await context.FileSystem.GetInputDirectoriesAsync())
                .SelectAsync(async x => await fileProvider.GetDirectoryAsync(x.Path));
            IEnumerable<IFile> matches = await directories
                .SelectManyAsync(async x => await Globber.GetFilesAsync(x, _patterns));
            return matches.Select(x => x.Path).Distinct().Select(match => fileProvider.GetDocument(match));
        }
    }
}
