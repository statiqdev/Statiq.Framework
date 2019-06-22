using Statiq.Common.Configuration;
using Statiq.Common.Meta;

namespace Statiq.Core.Configuration
{
    internal class Settings : MetadataDictionary, ISettings
    {
        public Settings()
        {
            this[Common.Meta.Keys.LinkHideIndexPages] = true;
            this[Common.Meta.Keys.LinkHideExtensions] = true;
            this[Common.Meta.Keys.UseCache] = true;
            this[Common.Meta.Keys.CleanOutputPath] = true;
        }
    }
}
