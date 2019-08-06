namespace Statiq.SearchIndex
{
    /// <summary>
    /// Metadata keys for use by the <see cref="GenerateLunrIndex"/> module.
    /// </summary>
    public static class GenerateLunrIndexKeys
    {
        /// <summary>
        /// Contains a <see cref="LunrIndexItem"/> that can be used to provide
        /// specific search index information for a given document.
        /// </summary>
        /// <type><see cref="LunrIndexItem"/></type>
        public const string LunrIndexItem = nameof(LunrIndexItem);
    }
}
