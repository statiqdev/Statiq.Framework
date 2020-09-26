using System.Collections.Immutable;

namespace Statiq.Common
{
    /// <summary>
    /// Tracks validation results and passes information about the current execution state to validators.
    /// </summary>
    public interface IValidationContext : IExecutionState
    {
        /// <summary>
        /// Adds a validation result.
        /// </summary>
        /// <param name="result">The validation result to add.</param>
        void Add(ValidationResult result);

        /// <summary>
        /// The documents to validate.
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
