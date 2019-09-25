namespace Statiq.Common
{
    /// <summary>
    /// Indicates when the pipeline is executed.
    /// </summary>
    public enum PipelineTrigger
    {
        /// <summary>
        /// The pipeline is normally executed unless explicit pipelines
        /// are specified and then it's only executed if specified.
        /// </summary>
        Default,

        /// <summary>
        /// The pipeline is not normally executed unless explicitly specified.
        /// </summary>
        /// <remarks>
        /// Manual pipelines can not be a dependency of another pipeline.
        /// </remarks>
        Manual,

        /// <summary>
        /// The pipeline is not normally executed unless explicitly specified
        /// or as a dependency of an executing pipeline.
        /// </summary>
        ManualOrDependency,

        /// <summary>
        /// The pipeline is always executed, regardless of explicitly specified pipelines.
        /// </summary>
        Always
    }
}
