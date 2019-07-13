using Statiq.Common;

namespace Statiq.Core
{
    internal class Settings : MetadataDictionary, ISettings
    {
        public Settings()
        {
            this[Common.Keys.LinkHideIndexPages] = true;
            this[Common.Keys.LinkHideExtensions] = true;
            this[Common.Keys.UseCache] = true;
            this[Common.Keys.CleanOutputPath] = true;
        }
    }
}
