using System.Collections.Generic;
using Wyam.Common.Documents;
using Wyam.Common.Modules;

namespace Wyam.Common.Execution
{
    /// <summary>
    /// Represents a named collection of modules that should be executed by the engine.
    /// </summary>
    public interface IPipeline : IModuleList
    {
        /// <summary>
        /// The name of the pipeline.
        /// </summary>
        string Name { get; }
    }
}
