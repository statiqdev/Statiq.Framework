namespace Statiq.Lunr
{
    /// <summary>
    /// Metadata keys for use by the <see cref="GenerateLunrIndexOld"/> module.
    /// </summary>
    public static class LunrKeys
    {
        /// <summary>
        /// Contains a <see cref="LunrIndexItem"/> that can be used to provide
        /// specific search index information for a given document.
        /// </summary>
        /// <type><see cref="LunrIndexItem"/></type>
        public const string LunrIndexItem = nameof(LunrIndexItem);

        /// <summary>
        /// Set to <c>true</c> in document metadata to omit a document from the search.
        /// </summary>
        public const string OmitFromSearch = nameof(OmitFromSearch);
    }
}
