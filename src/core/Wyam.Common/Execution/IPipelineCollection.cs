using System.Collections.Generic;
using Wyam.Common.Modules;

namespace Wyam.Common.Execution
{
    /// <summary>
    /// A collection of pipelines.
    /// </summary>
    public interface IPipelineCollection : IDictionary<string, IPipeline>
    {
        // Adds a new pipeline and returns it for editing
        IPipeline Add(string name);

        // Adds a new pipeline with the specified modules in the process phase
        // and creates a dependency to the previously added pipeline
        IPipeline AddSequential(string name, IEnumerable<IModule> processModules);
    }
}
