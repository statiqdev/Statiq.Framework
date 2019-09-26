namespace Statiq.Common
{
    /// <summary>
    /// Indicates when the pipeline is executed.
    /// </summary>
    public enum ExecutionPolicy
    {
        /// <summary>
        /// The pipeline is normally executed unless explicit pipelines
        /// are specified and then it's only executed if specified.
        /// </summary>
        Default,

        /// <summary>
        /// The pipeline is not normally executed unless explicitly specified
        /// or as a dependency of an executing pipeline.
        /// </summary>
        Manual,

        /// <summary>
        /// The pipeline is always executed, regardless of explicitly specified pipelines.
        /// </summary>
        Always
    }
}
