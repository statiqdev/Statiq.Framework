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
        public override async Task<IEnumerable<IDocument>> ExecuteAsync(IExecutionContext context)
        {
            if (Parallel)
            {
                IEnumerable<IDocument>[] taskResults = await Task.WhenAll(
                    context.Inputs.Select(input => Task.Run(() => ExecuteInput(input, context, ExecuteAsync), context.CancellationToken)));
                return taskResults.Where(x => x != null).SelectMany(x => x);
            }

            return await base.ExecuteAsync(context);
        }
    }
}
