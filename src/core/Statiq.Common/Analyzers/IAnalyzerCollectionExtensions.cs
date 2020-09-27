using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Statiq.Common
{
    public static class IAnalyzerCollectionExtensions
    {
        /// <summary>
        /// Adds an analyzer by type.
        /// </summary>
        /// <typeparam name="TAnalyzer">The type of the analyzer to add.</typeparam>
        /// <param name="analyzers">The analyzers.</param>
        public static void Add<TAnalyzer>(this IAnalyzerCollection analyzers)
            where TAnalyzer : IAnalyzer
        {
            analyzers.ThrowIfNull(nameof(analyzers));
            analyzers.Add(Activator.CreateInstance<TAnalyzer>());
        }

        /// <summary>
        /// Adds an analyzer by type.
        /// </summary>
        /// <param name="analyzers">The analyzers.</param>
        /// <param name="analyzerType">The type of the analyzer to add (must implement <see cref="IAnalyzer"/>).</param>
        public static void Add(this IAnalyzerCollection analyzers, Type analyzerType)
        {
            analyzers.ThrowIfNull(nameof(analyzers));
            analyzerType.ThrowIfNull(nameof(analyzerType));
            if (!typeof(IAnalyzer).IsAssignableFrom(analyzerType))
            {
                throw new ArgumentException("The type must implement " + nameof(IAnalyzer), nameof(analyzerType));
            }
            analyzers.Add((IAnalyzer)Activator.CreateInstance(analyzerType));
        }

        public static void Add(
            this IAnalyzerCollection analyzers,
            IEnumerable<string> pipelines,
            IEnumerable<Phase> phases,
            Func<IAnalyzerContext, Task> analyzeFunc) =>
            analyzers
                .ThrowIfNull(nameof(analyzers))
                .Add(new DelegateAnalyzer(pipelines, phases, analyzeFunc));

        public static void Add(
            this IAnalyzerCollection analyzers,
            IEnumerable<string> pipelines,
            IEnumerable<Phase> phases,
            Action<IAnalyzerContext> analyzeAction)
        {
            analyzeAction.ThrowIfNull(nameof(analyzeAction));
            analyzers.Add(
                pipelines,
                phases,
                context =>
                {
                    analyzeAction(context);
                    return Task.CompletedTask;
                });
        }

        public static void Add(
            this IAnalyzerCollection analyzers,
            IEnumerable<string> pipelines,
            IEnumerable<Phase> phases,
            Func<IDocument, IAnalyzerContext, Task> analyzeFunc)
        {
            analyzeFunc.ThrowIfNull(nameof(analyzeFunc));
            analyzers.Add(
                pipelines,
                phases,
                async context => await context.Documents.ParallelForEachAsync(async doc => await analyzeFunc(doc, context), context.CancellationToken));
        }

        public static void Add(
            this IAnalyzerCollection analyzers,
            IEnumerable<string> pipelines,
            IEnumerable<Phase> phases,
            Action<IDocument, IAnalyzerContext> analyzeAction)
        {
            analyzeAction.ThrowIfNull(nameof(analyzeAction));
            analyzers.Add(
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
            string pipeline,
            Phase phase,
            Func<IAnalyzerContext, Task> analyzeFunc) =>
            analyzers.Add(new[] { pipeline }, new[] { phase }, analyzeFunc);

        public static void Add(
            this IAnalyzerCollection analyzers,
            string pipeline,
            Phase phase,
            Action<IAnalyzerContext> analyzeAction)
        {
            analyzeAction.ThrowIfNull(nameof(analyzeAction));
            analyzers.Add(new[] { pipeline }, new[] { phase }, analyzeAction);
        }

        public static void Add(
            this IAnalyzerCollection analyzers,
            string pipeline,
            Phase phase,
            Func<IDocument, IAnalyzerContext, Task> analyzeFunc) =>
            analyzers.Add(new[] { pipeline }, new[] { phase }, analyzeFunc);

        public static void Add(
            this IAnalyzerCollection analyzers,
            string pipeline,
            Phase phase,
            Action<IDocument, IAnalyzerContext> analyzeAction)
        {
            analyzeAction.ThrowIfNull(nameof(analyzeAction));
            analyzers.Add(new[] { pipeline }, new[] { phase }, analyzeAction);
        }
    }
}
