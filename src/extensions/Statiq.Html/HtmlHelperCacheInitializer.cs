using Statiq.Common;

namespace Statiq.Html
{
    // Clears the cache after each execution
    public class HtmlHelperCacheInitializer : IEngineInitializer
    {
        public void Initialize(IEngine engine) =>
            engine.Events.Subscribe<AfterEngineExecution>(_ => HtmlHelper.ClearHtmlDocumentCache());
    }
}
