using System.Collections.Immutable;

namespace Statiq.Common
{
    /// <summary>
    /// Tracks analyzer results and passes information about the current execution state to analyzers.
    /// </summary>
    public interface IAnalyzerContext : IExecutionState
    {
        /// <summary>
        /// Adds an analyzer result.
        /// </summary>
        /// <param name="result">The analyzer result to add.</param>
        void Add(AnalyzerResult result);

        /// <summary>
        /// The documents to analyze.
        /// </summary>
        ImmutableArray<IDocument> Documents { get; }

        /// <summary>
        /// Gets the current execution state.
        /// </summary>
        IExecutionState ExecutionState { get; }

        /// <summary>
        /// Gets the name of the currently executing pipeline.
        /// </summary>
        string PipelineName { get; }

        /// <summary>
        /// Gets the currently executing pipeline.
        /// </summary>
        IReadOnlyPipeline Pipeline { get; }

        /// <summary>
        /// Gets the currently executing pipeline phase.
        /// </summary>
        Phase Phase { get; }
    }
}
