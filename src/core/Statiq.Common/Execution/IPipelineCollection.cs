using System.Collections.Generic;

namespace Statiq.Common
{
    /// <summary>
    /// A collection of pipelines.
    /// </summary>
    public interface IPipelineCollection : IDictionary<string, IPipeline>
    {
        // Adds a new pipeline and returns it for editing
        IPipeline Add(string name);
    }
}
