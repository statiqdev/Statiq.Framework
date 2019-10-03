using System;

namespace Statiq.Common
{
    /// <summary>
    /// Extensions for working with pipeline collections.
    /// </summary>
    public partial interface IPipelineCollection
    {
        public void Add(IPipeline pipeline) => Add(pipeline?.GetType().Name, pipeline);

        public IPipeline Add(Type pipelineType) =>
            Add(pipelineType ?? throw new ArgumentNullException(nameof(pipelineType)), pipelineType.Name);

        public IPipeline Add(Type pipelineType, string name)
        {
            _ = pipelineType ?? throw new ArgumentNullException(nameof(pipelineType));
            if (!typeof(IPipeline).IsAssignableFrom(pipelineType))
            {
                throw new ArgumentException("Provided type is not a pipeline");
            }
            IPipeline pipeline = (IPipeline)Activator.CreateInstance(pipelineType);
            Add(name ?? throw new ArgumentNullException(nameof(name)), pipeline);
            return pipeline;
        }

        public IPipeline Add<TPipeline>()
            where TPipeline : IPipeline =>
            Add<TPipeline>(typeof(TPipeline).Name);

        public IPipeline Add<TPipeline>(string name)
            where TPipeline : IPipeline
        {
            IPipeline pipeline = Activator.CreateInstance<TPipeline>();
            Add(name ?? throw new ArgumentNullException(nameof(name)), pipeline);
            return pipeline;
        }
    }
}