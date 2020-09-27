using Microsoft.Extensions.Logging;

namespace Statiq.Common
{
    public static class IAnalyzerContextExtensions
    {
        public static void Add(this IAnalyzerContext context, LogLevel logLevel, string message) => context.Add(logLevel, null, message);

        public static void Add(this IAnalyzerContext context, LogLevel logLevel, IDocument document, string message) =>
            context.Add(new AnalyzerResult
            {
                LogLevel = logLevel,
                Document = document,
                Message = message
            });

        public static void AddError(this IAnalyzerContext context, string message) => context.Add(LogLevel.Error, null, message);

        public static void AddError(this IAnalyzerContext context, IDocument document, string message) => context.Add(LogLevel.Error, document, message);

        public static void AddWarning(this IAnalyzerContext context, string message) => context.Add(LogLevel.Warning, null, message);

        public static void AddWarning(this IAnalyzerContext context, IDocument document, string message) => context.Add(LogLevel.Warning, document, message);

        public static void AddInformation(this IAnalyzerContext context, string message) => context.Add(LogLevel.Information, null, message);

        public static void AddInformation(this IAnalyzerContext context, IDocument document, string message) => context.Add(LogLevel.Information, document, message);

        public static void AddDebug(this IAnalyzerContext context, string message) => context.Add(LogLevel.Debug, null, message);

        public static void AddDebug(this IAnalyzerContext context, IDocument document, string message) => context.Add(LogLevel.Debug, document, message);
    }
}
