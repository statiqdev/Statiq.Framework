using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Statiq.Common;

namespace Statiq.Core
{
    /// <summary>
    /// Filters the current sequence of documents by destination.
    /// </summary>
    /// <remarks>
    /// This module filters documents using "or" logic. If you want to also apply
    /// "and" conditions, place additional <see cref="FilterDestinations"/> modules
    /// after this one.
    /// </remarks>
    /// <category>Control</category>
    public class FilterDestinations : SyncModule
    {
        private IEnumerable<string> _patterns;

        /// <summary>
        /// Filters input document by using globbing pattern(s) on the document source.
        /// </summary>
        /// <param name="patterns">The globbing patterns to apply.</param>
        public FilterDestinations(params string[] patterns)
            : this((IEnumerable<string>)patterns)
        {
        }

        /// <summary>
        /// Filters input document by using globbing pattern(s) on the document source.
        /// </summary>
        /// <param name="patterns">The globbing patterns to apply.</param>
        public FilterDestinations(IEnumerable<string> patterns)
        {
            _patterns = patterns ?? throw new ArgumentNullException(nameof(patterns));
        }

        /// <inheritdoc />
        protected override IEnumerable<IDocument> ExecuteContext(IExecutionContext context) =>
            context.Inputs.FilterDestinations(_patterns);
    }
}
