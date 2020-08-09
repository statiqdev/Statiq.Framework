namespace Statiq.Itunes
{
    /// <summary>
    /// Keys for use with the <see cref="ReadItunes"/> module.
    /// </summary>
    public static class ItunesKeys
    {
        /// <summary>
        /// The default metadata key for getting the Url of iTunes podcasts.
        /// </summary>
        /// <type><see cref="string"/></type>
        public const string ItunesLink = nameof(ItunesLink);

        /// <summary>
        /// The default metadata key for setting the iTunes podcast data in serialized format.
        /// </summary>
        /// <type><see cref="string"/></type>
        public const string SerializedPodcastData = nameof(SerializedPodcastData);

        /// <summary>
        /// The default metadata key for setting the collection of iTunes podcast episodes.
        /// </summary>
        /// <type><see cref="string"/></type>
        public const string Episodes = nameof(Episodes);
    }
}
