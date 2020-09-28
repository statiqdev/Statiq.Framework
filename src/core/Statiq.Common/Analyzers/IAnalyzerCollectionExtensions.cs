using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Statiq.Common
{
    public static class IAnalyzerCollectionExtensions
    {
        /// <summary>
        /// Adds an analyzer, inferring the name from the type name and removing a trailing "Analyzer" from the type name.
        /// </summary>
        /// <param name="analyzers">The analyzers.</param>
        /// <param name="analyzer">The analyzer to add.</param>
        public static void Add(this IAnalyzerCollection analyzers, IAnalyzer analyzer) =>
            analyzers.Add(analyzer?.GetType().Name.RemoveEnd("Analyzer", StringComparison.OrdinalIgnoreCase), analyzer);

        /// <summary>
        /// Adds an analyzer by type, inferring the name from the type name and removing a trailing "Analyzer" from the type name.
        /// </summary>
        /// <param name="analyzers">The analyzers.</param>
        /// <param name="analyzerType">The type of the analyzer to add (must implement <see cref="IAnalyzer"/>).</param>
        public static void Add(this IAnalyzerCollection analyzers, Type analyzerType) =>
            analyzers.Add(analyzerType?.Name.RemoveEnd("Analyzer", StringComparison.OrdinalIgnoreCase), analyzerType);

        /// <summary>
        /// Adds an analyzer by type, inferring the name from the type name.
        /// </summary>
        /// <typeparam name="TAnalyzer">The type of the analyzer to add.</typeparam>
        /// <param name="analyzers">The analyzers.</param>
        public static void Add<TAnalyzer>(this IAnalyzerCollection analyzers)
            where TAnalyzer : IAnalyzer =>
            analyzers.Add<TAnalyzer>(typeof(TAnalyzer).Name.RemoveEnd("Analyzer", StringComparison.OrdinalIgnoreCase));

        /// <summary>
        /// Adds an analyzer by type.
        /// </summary>
        /// <typeparam name="TAnalyzer">The type of the analyzer to add.</typeparam>
        /// <param name="analyzers">The analyzers.</param>
        /// <param name="name">The name of the analyzer.</param>
        public static void Add<TAnalyzer>(this IAnalyzerCollection analyzers, string name)
            where TAnalyzer : IAnalyzer
        {
            analyzers.ThrowIfNull(nameof(analyzers));
            analyzers.Add(name, Activator.CreateInstance<TAnalyzer>());
        }

        /// <summary>
        /// Adds an analyzer by type, inferring the name from the type name and removing a trailing "Analyzer" from the type name.
        /// </summary>
        /// <param name="analyzers">The analyzers.</param>
        /// <param name="name">The name of the analyzer.</param>
        /// <param name="analyzerType">The type of the analyzer to add (must implement <see cref="IAnalyzer"/>).</param>
        public static void Add(this IAnalyzerCollection analyzers, string name, Type analyzerType)
        {
            analyzers.ThrowIfNull(nameof(analyzers));
            analyzerType.ThrowIfNull(nameof(analyzerType));
            if (!typeof(IAnalyzer).IsAssignableFrom(analyzerType))
            {
                throw new ArgumentException("The type must implement " + nameof(IAnalyzer), nameof(analyzerType));
            }
            analyzers.Add(name, (IAnalyzer)Activator.CreateInstance(analyzerType));
        }

        /// <summary>
        /// Adds an analyzer, inferring the name from the type name and removing a trailing "Analyzer" from the type name.
        /// </summary>
        /// <param name="analyzers">The analyzers.</param>
        /// <param name="name">The name of the analyzer.</param>
        /// <param name="logLevel">Sets the log level of the analyzer being added.</param>
        /// <param name="analyzer">The analyzer to add.</param>
        public static void Add(this IAnalyzerCollection analyzers, string name, LogLevel logLevel, IAnalyzer analyzer)
        {
            analyzer.ThrowIfNull(nameof(analyzer)).LogLevel = logLevel;
            analyzers.Add(name, analyzer);
        }

        /// <summary>
        /// Adds an analyzer, inferring the name from the type name and removing a trailing "Analyzer" from the type name.
        /// </summary>
        /// <param name="analyzers">The analyzers.</param>
        /// <param name="logLevel">Sets the log level of the analyzer being added.</param>
        /// <param name="analyzer">The analyzer to add.</param>
        public static void Add(this IAnalyzerCollection analyzers, LogLevel logLevel, IAnalyzer analyzer)
        {
            analyzer.ThrowIfNull(nameof(analyzer)).LogLevel = logLevel;
            analyzers.Add(analyzer?.GetType().Name.RemoveEnd("Analyzer", StringComparison.OrdinalIgnoreCase), analyzer);
        }

        /// <summary>
        /// Adds an analyzer by type, inferring the name from the type name and removing a trailing "Analyzer" from the type name.
        /// </summary>
        /// <param name="analyzers">The analyzers.</param>
        /// <param name="logLevel">Sets the log level of the analyzer being added.</param>
        /// <param name="analyzerType">The type of the analyzer to add (must implement <see cref="IAnalyzer"/>).</param>
        public static void Add(this IAnalyzerCollection analyzers, LogLevel logLevel, Type analyzerType) =>
            analyzers.Add(analyzerType?.Name.RemoveEnd("Analyzer", StringComparison.OrdinalIgnoreCase), logLevel, analyzerType);

        /// <summary>
        /// Adds an analyzer by type, inferring the name from the type name.
        /// </summary>
        /// <typeparam name="TAnalyzer">The type of the analyzer to add.</typeparam>
        /// <param name="analyzers">The analyzers.</param>
        /// <param name="logLevel">Sets the log level of the analyzer being added.</param>
        public static void Add<TAnalyzer>(this IAnalyzerCollection analyzers, LogLevel logLevel)
            where TAnalyzer : IAnalyzer =>
            analyzers.Add<TAnalyzer>(typeof(TAnalyzer).Name.RemoveEnd("Analyzer", StringComparison.OrdinalIgnoreCase), logLevel);

        /// <summary>
        /// Adds an analyzer by type.
        /// </summary>
        /// <typeparam name="TAnalyzer">The type of the analyzer to add.</typeparam>
        /// <param name="analyzers">The analyzers.</param>
        /// <param name="name">The name of the analyzer.</param>
        /// <param name="logLevel">Sets the log level of the analyzer being added.</param>
        public static void Add<TAnalyzer>(this IAnalyzerCollection analyzers, string name, LogLevel logLevel)
            where TAnalyzer : IAnalyzer
        {
            analyzers.ThrowIfNull(nameof(analyzers));
            TAnalyzer analyzer = Activator.CreateInstance<TAnalyzer>();
            analyzer.LogLevel = logLevel;
            analyzers.Add(name, analyzer);
        }

        /// <summary>
        /// Adds an analyzer by type, inferring the name from the type name and removing a trailing "Analyzer" from the type name.
        /// </summary>
        /// <param name="analyzers">The analyzers.</param>
        /// <param name="name">The name of the analyzer.</param>
        /// <param name="logLevel">Sets the log level of the analyzer being added.</param>
        /// <param name="analyzerType">The type of the analyzer to add (must implement <see cref="IAnalyzer"/>).</param>
        public static void Add(this IAnalyzerCollection analyzers, string name, LogLevel logLevel, Type analyzerType)
        {
            analyzers.ThrowIfNull(nameof(analyzers));
            analyzerType.ThrowIfNull(nameof(analyzerType));
            if (!typeof(IAnalyzer).IsAssignableFrom(analyzerType))
            {
                throw new ArgumentException("The type must implement " + nameof(IAnalyzer), nameof(analyzerType));
            }
            IAnalyzer analyzer = (IAnalyzer)Activator.CreateInstance(analyzerType);
            analyzer.LogLevel = logLevel;
            analyzers.Add(name, analyzer);
        }

        // DelegateAnalyzer

        public static void Add(
            this IAnalyzerCollection analyzers,
            string name,
            LogLevel logLevel,
            IEnumerable<string> pipelines,
            IEnumerable<Phase> phases,
            Func<ImmutableArray<IDocument>, IAnalyzerContext, Task> analyzeFunc) =>
            analyzers
                .ThrowIfNull(nameof(analyzers))
                .Add(name, new DelegateAnalyzer(logLevel, pipelines, phases, analyzeFunc));

        public static void Add(
            this IAnalyzerCollection analyzers,
            string name,
            LogLevel logLevel,
            IEnumerable<string> pipelines,
            IEnumerable<Phase> phases,
            Action<ImmutableArray<IDocument>, IAnalyzerContext> analyzeAction)
        {
            analyzeAction.ThrowIfNull(nameof(analyzeAction));
            analyzers.Add(
                name,
                logLevel,
                pipelines,
                phases,
                (documents, context) =>
                {
                    analyzeAction(documents, context);
                    return Task.CompletedTask;
                });
        }

        public static void AddDocument(
            this IAnalyzerCollection analyzers,
            string name,
            LogLevel logLevel,
            IEnumerable<string> pipelines,
            IEnumerable<Phase> phases,
            Func<IDocument, IAnalyzerContext, Task> analyzeFunc)
        {
            analyzeFunc.ThrowIfNull(nameof(analyzeFunc));
            analyzers.Add(
                name,
                logLevel,
                pipelines,
                phases,
                async (documents, context) => await documents.ParallelForEachAsync(async doc => await analyzeFunc(doc, new DocumentAnalyzerContext(context, doc)), context.CancellationToken));
        }

        public static void AddDocument(
            this IAnalyzerCollection analyzers,
            string name,
            LogLevel logLevel,
            IEnumerable<string> pipelines,
            IEnumerable<Phase> phases,
            Action<IDocument, IAnalyzerContext> analyzeAction)
        {
            analyzeAction.ThrowIfNull(nameof(analyzeAction));
            analyzers.AddDocument(
                name,
                logLevel,
                pipelines,
                phases,
                (document, context) =>
                {
                    analyzeAction(document, context);
                    return Task.CompletedTask;
                });
        }

        public static void Add(
            this IAnalyzerCollection analyzers,
            string name,
            LogLevel logLevel,
            string pipeline,
            Phase phase,
            Func<ImmutableArray<IDocument>, IAnalyzerContext, Task> analyzeFunc) =>
            analyzers.Add(name, logLevel, new[] { pipeline }, new[] { phase }, analyzeFunc);

        public static void Add(
            this IAnalyzerCollection analyzers,
            string name,
            LogLevel logLevel,
            string pipeline,
            Phase phase,
            Action<ImmutableArray<IDocument>, IAnalyzerContext> analyzeAction)
        {
            analyzeAction.ThrowIfNull(nameof(analyzeAction));
            analyzers.Add(name, logLevel, new[] { pipeline }, new[] { phase }, analyzeAction);
        }

        public static void AddDocument(
            this IAnalyzerCollection analyzers,
            string name,
            LogLevel logLevel,
            string pipeline,
            Phase phase,
            Func<IDocument, IAnalyzerContext, Task> analyzeFunc) =>
            analyzers.AddDocument(name, logLevel, new[] { pipeline }, new[] { phase }, analyzeFunc);

        public static void AddDocument(
            this IAnalyzerCollection analyzers,
            string name,
            LogLevel logLevel,
            string pipeline,
            Phase phase,
            Action<IDocument, IAnalyzerContext> analyzeAction)
        {
            analyzeAction.ThrowIfNull(nameof(analyzeAction));
            analyzers.AddDocument(name, logLevel, new[] { pipeline }, new[] { phase }, analyzeAction);
        }
    }
}
