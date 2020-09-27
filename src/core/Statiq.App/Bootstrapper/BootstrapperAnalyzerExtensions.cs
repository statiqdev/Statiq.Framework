using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Statiq.Common;

namespace Statiq.App
{
    public static class BootstrapperAnalyzerExtensions
    {
        public static Bootstrapper AddAnalyzer<TAnalyzer>(this Bootstrapper bootstrapper)
            where TAnalyzer : IAnalyzer =>
            bootstrapper.ConfigureEngine(x => x.Analyzers.Add<TAnalyzer>());

        public static TBootstrapper AddAnalyzer<TBootstrapper>(this TBootstrapper bootstrapper, Type analyzerType)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.ConfigureEngine(x => x.Analyzers.Add(analyzerType));

        public static TBootstrapper AddAnalyzer<TBootstrapper>(this TBootstrapper bootstrapper, IAnalyzer analyzer)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.ConfigureEngine(x => x.Analyzers.Add(analyzer));

        public static TBootstrapper Analyze<TBootstrapper>(
            this TBootstrapper bootstrapper,
            IEnumerable<string> pipelines,
            IEnumerable<Phase> phases,
            Func<IAnalyzerContext, Task> analyzeFunc)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.ConfigureEngine(x => x.Analyzers.Add(pipelines, phases, analyzeFunc));

        public static TBootstrapper Analyze<TBootstrapper>(
            this TBootstrapper bootstrapper,
            IEnumerable<string> pipelines,
            IEnumerable<Phase> phases,
            Action<IAnalyzerContext> analyzeAction)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.ConfigureEngine(x => x.Analyzers.Add(pipelines, phases, analyzeAction));

        public static TBootstrapper Analyze<TBootstrapper>(
            this TBootstrapper bootstrapper,
            IEnumerable<string> pipelines,
            IEnumerable<Phase> phases,
            Func<IDocument, IAnalyzerContext, Task> analyzeFunc)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.ConfigureEngine(x => x.Analyzers.Add(pipelines, phases, analyzeFunc));

        public static TBootstrapper Analyze<TBootstrapper>(
            this TBootstrapper bootstrapper,
            IEnumerable<string> pipelines,
            IEnumerable<Phase> phases,
            Action<IDocument, IAnalyzerContext> analyzeAction)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.ConfigureEngine(x => x.Analyzers.Add(pipelines, phases, analyzeAction));

        public static TBootstrapper Analyze<TBootstrapper>(
            this TBootstrapper bootstrapper,
            string pipeline,
            Phase phase,
            Func<IAnalyzerContext, Task> analyzeFunc)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.ConfigureEngine(x => x.Analyzers.Add(pipeline, phase, analyzeFunc));

        public static TBootstrapper Analyze<TBootstrapper>(
            this TBootstrapper bootstrapper,
            string pipeline,
            Phase phase,
            Action<IAnalyzerContext> analyzeAction)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.ConfigureEngine(x => x.Analyzers.Add(pipeline, phase, analyzeAction));

        public static TBootstrapper Analyze<TBootstrapper>(
            this TBootstrapper bootstrapper,
            string pipeline,
            Phase phase,
            Func<IDocument, IAnalyzerContext, Task> analyzeFunc)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.ConfigureEngine(x => x.Analyzers.Add(pipeline, phase, analyzeFunc));

        public static TBootstrapper Analyze<TBootstrapper>(
            this TBootstrapper bootstrapper,
            string pipeline,
            Phase phase,
            Action<IDocument, IAnalyzerContext> analyzeAction)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.ConfigureEngine(x => x.Analyzers.Add(pipeline, phase, analyzeAction));

        /// <summary>
        /// Adds all analyzers that implement <see cref="IAnalyzer"/> from the specified assembly.
        /// </summary>
        /// <param name="bootstrapper">The bootstrapper.</param>
        /// <param name="assembly">The assembly to add analyzers from.</param>
        /// <returns>The current bootstrapper.</returns>
        public static TBootstrapper AddAnalyzers<TBootstrapper>(this TBootstrapper bootstrapper, Assembly assembly)
            where TBootstrapper : IBootstrapper
        {
            bootstrapper.ThrowIfNull(nameof(bootstrapper));
            assembly.ThrowIfNull(nameof(assembly));
            foreach (Type analyzerType in bootstrapper.ClassCatalog.GetTypesAssignableTo<IAnalyzer>().Where(x => x.Assembly.Equals(assembly)))
            {
                bootstrapper.AddAnalyzer(analyzerType);
            }
            return bootstrapper;
        }

        /// <summary>
        /// Adds all analyzers that implement <see cref="IAnalyzer"/> from the entry assembly.
        /// </summary>
        /// <param name="bootstrapper">The bootstrapper.</param>
        /// <returns>The current bootstrapper.</returns>
        public static TBootstrapper AddAnalyzers<TBootstrapper>(this TBootstrapper bootstrapper)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.AddAnalyzers(Assembly.GetEntryAssembly());

        public static Bootstrapper AddAnalyzers<TParent>(this Bootstrapper bootstrapper) => bootstrapper.AddAnalyzers(typeof(TParent));

        public static TBootstrapper AddAnalyzers<TBootstrapper>(this TBootstrapper bootstrapper, Type parentType)
            where TBootstrapper : IBootstrapper
        {
            bootstrapper.ThrowIfNull(nameof(bootstrapper));
            parentType.ThrowIfNull(nameof(parentType));
            foreach (Type analyzerType in parentType.GetNestedTypes().Where(x => typeof(IAnalyzer).IsAssignableFrom(x)))
            {
                bootstrapper.AddAnalyzer(analyzerType);
            }
            return bootstrapper;
        }
    }
}
