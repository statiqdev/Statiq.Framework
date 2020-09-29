using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            List<(IDocument Document, AnalyzerResult Result)> buildServerResults = new List<(IDocument, AnalyzerResult)>();
            foreach (IGrouping<IDocument, AnalyzerResult> documentGroup in results.GroupBy(x => x.Document).OrderBy(x => x.Key.ToSafeDisplayString()))
            {
                if (documentGroup.Key is object)
                {
                    executionState.Logger.LogInformation(documentGroup.Key.ToDisplayString());
                }
                foreach (AnalyzerResult result in documentGroup.OrderBy(x => x.AnalyzerName))
                {
                    executionState.Logger.Log(result.LogLevel, $"{result.Message} ({result.AnalyzerName})");
                    if (result.LogLevel != LogLevel.None && result.LogLevel >= LogLevel.Warning)
                    {
                        buildServerResults.Add((documentGroup.Key, result));
                    }
                }
            }

            // Special build server analyzer logging
            if (buildServerResults.Count > 0)
            {
                // Azure Pipelines
                if (!string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("TF_BUILD")))
                {
                    LogBuildServer("Azure Pipelines", "##vso[task.logissue type=", ";sourcepath=", "]");
                }

                // GitHub Actions
                if (!string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("GITHUB_ACTIONS")))
                {
                    LogBuildServer("GitHub Actions", "::", " file=", "::");
                }
            }

            void LogBuildServer(string buildServer, string prefix, string filePrefix, string suffix)
            {
                // Get the repository directory
                IDirectory repoDirectory = executionState.FileSystem.GetRootDirectory();
                while (repoDirectory is object && repoDirectory.GetDirectory(".git") is null)
                {
                    repoDirectory = repoDirectory.Parent;
                }

                // Generate the logging string
                StringBuilder builder = new StringBuilder($"-- {buildServer} Log Output --");
                foreach ((IDocument Document, AnalyzerResult Result) buildServerResult in buildServerResults)
                {
                    builder.AppendLine();
                    builder.Append(prefix);
                    builder.Append(buildServerResult.Result.LogLevel == LogLevel.Warning ? "warning" : "error");
                    if (buildServerResult.Document is object && !buildServerResult.Document.Source.IsNullOrEmpty && repoDirectory is object)
                    {
                        NormalizedPath repoRelativePath = repoDirectory.Path.GetRelativePath(buildServerResult.Document.Source);
                        if (!repoRelativePath.IsNullOrEmpty)
                        {
                            builder.Append(filePrefix);
                            builder.Append(repoRelativePath.FullPath);
                        }
                    }
                    builder.Append(suffix);
                    builder.Append($"{buildServerResult.Result.Message} ({buildServerResult.Result.AnalyzerName})");
                }

                // Log the build server output
                executionState.Logger.LogInformation(builder.ToString());
            }
        }
    }
}
