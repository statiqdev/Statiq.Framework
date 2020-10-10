using Microsoft.Extensions.Logging;

namespace Statiq.Common
{
    public static class IAnalyzerContextExtensions
    {
        public static void Add(this IAnalyzerContext context, string message) => context.AddAnalyzerResult(null, message);
    }
}
