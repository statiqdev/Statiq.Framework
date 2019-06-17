using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Statiq.Common.Configuration;
using Statiq.Common.Documents;
using Statiq.Common.Execution;
using Statiq.Common.Modules;

namespace Statiq.Core.Modules.Control
{
    /// <summary>
    /// Filters the current sequence of modules using a predicate.
    /// </summary>
    /// <category>Control</category>
    public class Where : IModule
    {
        private readonly DocumentConfig<bool> _predicate;

        /// <summary>
        /// Specifies the predicate to use for filtering documents.
        /// Only input documents for which the predicate returns <c>true</c> will be output.
        /// </summary>
        /// <param name="predicate">A predicate delegate that should return a <c>bool</c>.</param>
        public Where(DocumentConfig<bool> predicate)
        {
            _predicate = predicate ?? throw new ArgumentNullException(nameof(predicate));
        }

        /// <inheritdoc />
        public Task<IEnumerable<IDocument>> ExecuteAsync(IReadOnlyList<IDocument> inputs, IExecutionContext context) => inputs.FilterAsync(_predicate, context);
    }
}
