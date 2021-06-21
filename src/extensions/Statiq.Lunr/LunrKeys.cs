namespace Statiq.Lunr
{
    /// <summary>
    /// Metadata keys for use by the <see cref="GenerateLunrIndex"/> module.
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
        /// Set to <c>true</c> in document metadata to hide a document from the search index.
        /// </summary>
        public const string HideFromSearchIndex = nameof(HideFromSearchIndex);
    }
}
