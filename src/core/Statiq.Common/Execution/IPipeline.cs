using System;
using System.Collections.Generic;

namespace Statiq.Common
{
    /// <summary>
    /// Represents a named collection of modules that should be executed by the engine.
    /// </summary>
    /// <remarks>
    /// If the pipeline implements <see cref="IDisposable"/>, <see cref="IDisposable.Dispose"/>
    /// will be called when the engine is disposed (I.e., on application exit).
    /// </remarks>
    public interface IPipeline
    {
        /// <summary>
        /// Modules that will execute during the input phase.
        /// </summary>
        ModuleList InputModules { get; }

        /// <summary>
        /// Modules that will execute during the process phase.
        /// </summary>
        ModuleList ProcessModules { get; }

        /// <summary>
        /// Modules that will execute during the transform phase.
        /// </summary>
        ModuleList TransformModules { get; }

        /// <summary>
        /// Modules that will execute during the output phase.
        /// </summary>
        ModuleList OutputModules { get; }

        /// <summary>
        /// The names of pipelines this pipeline depends on.
        /// </summary>
        HashSet<string> Dependencies { get; }

        /// <summary>
        /// An isolated pipeline runs immediately without any dependencies and
        /// has restrictions on accessing documents from other pipelines.
        /// </summary>
        bool Isolated { get; set; }

        /// <summary>
        /// A deployment pipeline works just like other pipelines except
        /// it's <see cref="Phase.Output"/> phase will only execute when
        /// all other non-deployment pipelines have completed their
        /// <see cref="Phase.Output"/> phase (including isolated pipelines).
        /// </summary>
        bool Deployment { get; set; }

        /// <summary>
        /// Indicates when the pipeline is executed.
        /// </summary>
        ExecutionPolicy ExecutionPolicy { get; set; }
    }
}
