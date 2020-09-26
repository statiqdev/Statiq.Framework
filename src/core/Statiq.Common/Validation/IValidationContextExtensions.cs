using Microsoft.Extensions.Logging;

namespace Statiq.Common
{
    public static class IValidationContextExtensions
    {
        public static void Add(this IValidationContext context, LogLevel logLevel, string message) => context.Add(logLevel, null, message);

        public static void Add(this IValidationContext context, LogLevel logLevel, IDocument document, string message) =>
            context.Add(new ValidationResult
            {
                LogLevel = logLevel,
                Document = document,
                Message = message
            });

        public static void AddError(this IValidationContext context, string message) => context.Add(LogLevel.Error, null, message);

        public static void AddError(this IValidationContext context, IDocument document, string message) => context.Add(LogLevel.Error, document, message);

        public static void AddWarning(this IValidationContext context, string message) => context.Add(LogLevel.Warning, null, message);

        public static void AddWarning(this IValidationContext context, IDocument document, string message) => context.Add(LogLevel.Warning, document, message);

        public static void AddInformation(this IValidationContext context, string message) => context.Add(LogLevel.Information, null, message);

        public static void AddInformation(this IValidationContext context, IDocument document, string message) => context.Add(LogLevel.Information, document, message);

        public static void AddDebug(this IValidationContext context, string message) => context.Add(LogLevel.Debug, null, message);

        public static void AddDebug(this IValidationContext context, IDocument document, string message) => context.Add(LogLevel.Debug, document, message);
    }
}
