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
    }
}