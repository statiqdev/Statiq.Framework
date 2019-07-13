namespace Statiq.Common
{
    /// <summary>
    /// Stores global settings that control behavior and execution.
    /// </summary>
    /// <metadata cref="Keys.Host" usage="Setting" />
    /// <metadata cref="Keys.LinksUseHttps" usage="Setting" />
    /// <metadata cref="Keys.LinkRoot" usage="Setting" />
    /// <metadata cref="Keys.LinkHideIndexPages" usage="Setting" />
    /// <metadata cref="Keys.LinkHideExtensions" usage="Setting" />
    /// <metadata cref="Keys.UseCache" usage="Setting" />
    /// <metadata cref="Keys.CleanOutputPath" usage="Setting" />
    /// <metadata cref="Keys.DateTimeInputCulture" usage="Setting" />
    /// <metadata cref="Keys.DateTimeDisplayCulture" usage="Setting" />
    public interface ISettings : IMetadataDictionary, IReadOnlySettings, IConfigurable
    {
    }
}
