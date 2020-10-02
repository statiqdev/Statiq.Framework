using Microsoft.Extensions.Logging;

namespace Statiq.Common
{
    public static class IExecutionStateLogBuildServerExtensions
    {
        private static readonly object _buildServerLoggerLock = new object();
        private static BuildServerLogger _buildServerLogger;

        public static void LogBuildServerWarning(this IExecutionState executionState, string message) =>
            executionState.LogBuildServerWarning(null, message);

        public static void LogBuildServerWarning(this IExecutionState executionState, IDocument document, string message) =>
            LogBuildServer(executionState, LogLevel.Warning, document, message);

        public static void LogBuildServerError(this IExecutionState executionState, string message) =>
            executionState.LogBuildServerError(null, message);

        public static void LogBuildServerError(this IExecutionState executionState, IDocument document, string message) =>
            LogBuildServer(executionState, LogLevel.Error, document, message);

        private static void LogBuildServer(IExecutionState executionState, LogLevel logLevel, IDocument document, string message)
        {
            lock (_buildServerLoggerLock)
            {
                if (_buildServerLogger is null)
                {
                    _buildServerLogger = new BuildServerLogger(executionState);
                }
            }
            _buildServerLogger.Log(logLevel, document, message);
        }
    }
}
