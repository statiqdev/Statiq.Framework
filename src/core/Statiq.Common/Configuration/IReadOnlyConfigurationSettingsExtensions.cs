namespace Statiq.Common
{
    public static class IReadOnlyConfigurationSettingsExtensions
    {
        /// <summary>
        /// Gets a guaranteed index file name, returning a default of "index.html" if
        /// <see cref="Keys.IndexFileName"/> is not defined or is empty or whitespace.
        /// </summary>
        /// <param name="settings">The settings.</param>
        /// <returns>The index file name.</returns>
        public static string GetIndexFileName(this IReadOnlyConfigurationSettings settings)
        {
            settings.ThrowIfNull(nameof(settings));
            string indexFileName = settings.GetString(Keys.IndexFileName);
            return indexFileName.IsNullOrWhiteSpace() ? "index.html" : indexFileName;
        }
    }
}
