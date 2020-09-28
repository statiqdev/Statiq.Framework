using System.Collections.Generic;
using System.Linq;
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

        public static void LogResults(IEnumerable<AnalyzerResult> results, ILogger logger)
        {
            // Group by document and prefix with analyzer name
            foreach (IGrouping<IDocument, AnalyzerResult> documentGroup in results.GroupBy(x => x.Document).OrderBy(x => x.Key.ToSafeDisplayString()))
            {
                if (documentGroup.Key is object)
                {
                    logger.LogInformation(documentGroup.Key.ToDisplayString());
                }
                foreach (AnalyzerResult result in documentGroup.OrderBy(x => x.AnalyzerName))
                {
                    logger.Log(result.LogLevel, $"{result.AnalyzerName} » {result.Message}");
                }
            }
        }
    }
}
