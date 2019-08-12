using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Statiq.Common
{
    /// <summary>
    /// A module that processes documents in parallel (with the option to process sequentially).
    /// </summary>
    public abstract class ParallelModule : Module
    {
        /// <summary>
        /// Indicates whether documents will be
        /// processed by this module in parallel.
        /// </summary>
        public bool Parallel { get; internal set; } = true;

        /// <inheritdoc />
        public override Task<IEnumerable<IDocument>> ExecuteAsync(IExecutionContext context) =>
            Parallel
                ? context.Inputs.ParallelSelectManyAsync(input => ExecuteInput(input, context, ExecuteAsync), context.CancellationToken)
                : base.ExecuteAsync(context);
    }
}
