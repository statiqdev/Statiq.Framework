namespace Statiq.Core.Modules.Control
{
    /// <summary>
    /// Determines how to handle the output documents from the <see cref="ExecuteModules"/> module.
    /// </summary>
    public enum ExecuteModuleResults
    {
        /// <summary>
        /// Replaces the input documents with the output documents.
        /// </summary>
        Replace,

        /// <summary>
        /// Concatenates the output documents with the input documents.
        /// </summary>
        Concat,

        /// <summary>
        /// Replaces the content and merges the metadata of each input document with the output documents.
        /// </summary>
        Merge
    }
}
