using System.Collections.Generic;

namespace Statiq.Common
{
    /// <summary>
    /// Represents a named collection of modules that should be executed by the engine.
    /// </summary>
    public interface IReadOnlyPipeline
    {
        /// <summary>
        /// The names of pipelines this pipeline depends on.
        /// </summary>
        IReadOnlyCollection<string> Dependencies { get; }

        /// <summary>
        /// The names of pipelines that depend on this pipeline.
        /// </summary>
        IReadOnlyCollection<string> DependencyOf { get; }

        /// <summary>
        /// An isolated pipeline runs immediately without any dependencies and
        /// has restrictions on accessing documents from other pipelines.
        /// </summary>
        bool Isolated { get; }

        /// <summary>
        /// A deployment pipeline works just like other pipelines except
        /// it's <see cref="Phase.Output"/> phase will only execute when
        /// all other non-deployment pipelines have completed their
        /// <see cref="Phase.Output"/> phase (including isolated pipelines).
        /// </summary>
        bool Deployment { get; }

        /// <summary>
        /// Indicates when the pipeline is executed.
        /// </summary>
        ExecutionPolicy ExecutionPolicy { get; }
    }
}
