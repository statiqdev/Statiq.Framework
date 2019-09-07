using System;

namespace Statiq.Common
{
    /// <summary>
    /// Extensions for working with pipeline collections.
    /// </summary>
    public partial interface IPipelineCollection
    {
        public void Add(IPipeline pipeline) => Add(pipeline?.GetType().Name, pipeline);

        public IPipeline Add<TPipeline>(string name)
            where TPipeline : IPipeline
        {
            IPipeline pipeline = Activator.CreateInstance<TPipeline>();
            Add(name, pipeline);
            return pipeline;
        }

        public IPipeline Add<TPipeline>()
            where TPipeline : IPipeline =>
            Add<TPipeline>(typeof(TPipeline).Name);
    }
}