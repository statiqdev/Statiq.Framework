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
        public override IAsyncEnumerable<IDocument> ExecuteAsync(IExecutionContext context) =>
            Parallel
                ? context.Inputs.ToAsyncEnumerable().ParallelSelectManyAsync(input => ExecuteInput(input, context, ExecuteAsync))
                : base.ExecuteAsync(context);
    }
}
