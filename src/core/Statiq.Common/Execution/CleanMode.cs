namespace Statiq.Common
{
    public enum CleanMode
    {
        /// <summary>
        /// Cleans all files in the output folder on initial execution and then
        /// only those files written or copied between executions.
        /// </summary>
        Self,

        /// <summary>
        /// Does not clean the output folder between executions.
        /// </summary>
        None,

        /// <summary>
        /// Cleans the entire output folder between executions.
        /// </summary>
        Full
    }
}
