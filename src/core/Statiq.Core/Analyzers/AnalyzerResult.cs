using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Statiq.Common;

namespace Statiq.Core
{
    internal class AnalyzerResult
    {
        public AnalyzerResult(string analyzerName, LogLevel logLevel, IDocument document, string message)
        {
            AnalyzerName = analyzerName;
            LogLevel = logLevel;
            Document = document;
            Message = message;
        }

        public string AnalyzerName { get; }

        public LogLevel LogLevel { get; }

        public IDocument Document { get; }

        public string Message { get; }

        public static void LogResults(IEnumerable<AnalyzerResult> results, IExecutionState executionState)
        {
            // Group by document and prefix with analyzer name
            foreach (IGrouping<IDocument, AnalyzerResult> documentGroup in results.GroupBy(x => x.Document).OrderBy(x => x.Key.ToSafeDisplayString()))
            {
                if (documentGroup.Key is object)
                {
                    executionState.Logger.LogInformation(documentGroup.Key.ToDisplayString());
                }
                foreach (AnalyzerResult result in documentGroup.OrderBy(x => x.AnalyzerName))
                {
                    executionState.Logger.Log(result.LogLevel, $"{result.Message} ({result.AnalyzerName})");
                    if (result.LogLevel == LogLevel.Warning)
                    {
                        executionState.LogBuildServerWarning(documentGroup.Key, $"{result.Message} ({result.AnalyzerName})");
                    }
                    else if (result.LogLevel != LogLevel.None && result.LogLevel >= LogLevel.Error)
                    {
                        executionState.LogBuildServerError(documentGroup.Key, $"{result.Message} ({result.AnalyzerName})");
                    }
                }
            }
        }
    }
}
