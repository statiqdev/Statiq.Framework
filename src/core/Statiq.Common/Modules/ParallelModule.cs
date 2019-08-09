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
                ? ParallelExecuteAsync(context, (i, c) => ExecuteInput(i, c, ExecuteAsync))
                : base.ExecuteAsync(context);

        internal static async IAsyncEnumerable<IDocument> ParallelExecuteAsync(
            IExecutionContext context,
            Func<IDocument, IExecutionContext, IAsyncEnumerable<IDocument>> executeFunc)
        {
            // Have to convert it to a plain IEnumerable since that's what Task.WhenAll() takes
            IEnumerable<Task<IAsyncEnumerable<IDocument>>> tasks =
                context.Inputs.Select(input => Task.Run(() => executeFunc(input, context), context.CancellationToken)).ToEnumerable();
            IAsyncEnumerable<IDocument>[] results = await Task.WhenAll(tasks);
            foreach (IAsyncEnumerable<IDocument> result in results)
            {
                if (result != null)
                {
                    await foreach (IDocument document in result)
                    {
                        yield return document;
                    }
                }
            }
        }
    }
}
