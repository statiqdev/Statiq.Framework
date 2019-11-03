using System;

namespace Statiq.Common
{
    /// <summary>
    /// Extensions for working with pipeline collections.
    /// </summary>
    public partial interface IPipelineCollection
    {
        public void Add(IPipeline pipeline) =>
            Add((pipeline as INamedPipeline)?.PipelineName ?? pipeline?.GetType().Name, pipeline);

        public void AddIfNonExisting(IPipeline pipeline)
        {
            string name = (pipeline as INamedPipeline)?.PipelineName ?? pipeline?.GetType().Name;
            if (!ContainsKey(name))
            {
                Add(name, pipeline);
            }
        }
    }
}