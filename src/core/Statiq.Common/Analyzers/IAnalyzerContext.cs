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
        /// <param name="document">The document this result applies to.</param>
        /// <param name="message">The analyzer result message to add.</param>
        void Add(IDocument document, string message);

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
