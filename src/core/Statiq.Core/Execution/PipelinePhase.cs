using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Statiq.Common;

namespace Statiq.Core
{
    /// <summary>
    /// This contains the pipeline execution context and other data
    /// needed to execute a pipeline and cache it's results.
    /// </summary>
    internal class PipelinePhase : IDisposable
    {
        private readonly IList<IModule> _modules;
        private readonly ILogger _logger;
        private bool _disposed;

        public PipelinePhase(IPipeline pipeline, string pipelineName, Phase phase, IList<IModule> modules, ILogger logger, params PipelinePhase[] dependencies)
        {
            Pipeline = pipeline;
            PipelineName = pipelineName;
            Phase = phase;
            _modules = modules ?? new List<IModule>();
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            Dependencies = dependencies ?? Array.Empty<PipelinePhase>();
        }

        public IPipeline Pipeline { get; }

        public string PipelineName { get; }

        public Phase Phase { get; }

        /// <summary>
        /// Contains direct dependencies for this pipeline and phase.
        /// The first dependency should contain the input documents for this phase.
        /// </summary>
        public PipelinePhase[] Dependencies { get; set; }

        /// <summary>
        /// Holds the output documents from the previous execution of this phase.
        /// </summary>
        public ImmutableArray<IDocument> Outputs { get; private set; } = ImmutableArray<IDocument>.Empty;

        /// <summary>
        /// The first dependency always holds the input documents for this phase.
        /// </summary>
        /// <returns>The input documents for this phase.</returns>
        private ImmutableArray<IDocument> GetInputs() => Dependencies.Length == 0 ? ImmutableArray<IDocument>.Empty : Dependencies[0].Outputs;

        // This is the main execute method called by the engine
        public async Task ExecuteAsync(
            Engine engine,
            Guid executionId,
            ConcurrentDictionary<string, PhaseResult[]> phaseResults,
            CancellationTokenSource cancellationTokenSource)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(PipelinePhase));
            }

            // Raise the before event
            await engine.Events.RaiseAsync(new BeforePipelinePhaseExecution(executionId, PipelineName, Phase));

            // Skip the phase if there are no modules
            if (_modules.Count == 0)
            {
                _logger.LogDebug($"{PipelineName}/{Phase} » Pipeline contains no modules, skipping");
                Outputs = GetInputs();
                return;
            }

            // Execute the phase
            ImmutableArray<IDocument> inputs = GetInputs();
            System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();
            _logger.LogInformation($"-> {PipelineName}/{Phase} » Starting {PipelineName} {Phase} phase execution... ({inputs.Length} input document(s), {_modules.Count} module(s))");
            try
            {
                // Execute all modules in the pipeline with a new DI scope per phase
                IServiceScopeFactory serviceScopeFactory = engine.Services.GetRequiredService<IServiceScopeFactory>();
                using (IServiceScope serviceScope = serviceScopeFactory.CreateScope())
                {
                    ExecutionContextData contextData = new ExecutionContextData(
                        this,
                        engine,
                        executionId,
                        phaseResults,
                        serviceScope.ServiceProvider,
                        cancellationTokenSource.Token);
                    Outputs = await Engine.ExecuteModulesAsync(contextData, null, _modules, inputs, _logger);
                    stopwatch.Stop();
                    _logger.LogInformation($"-- {PipelineName}/{Phase} » Finished {PipelineName} {Phase} phase execution ({Outputs.Length} output document(s), {stopwatch.ElapsedMilliseconds} ms)");
                }
            }
            catch (Exception ex)
            {
                if (!(ex is OperationCanceledException))
                {
                    _logger.LogCritical($"Exception while executing pipeline {PipelineName}/{Phase}: {ex}");
                }
                Outputs = ImmutableArray<IDocument>.Empty;
                throw;
            }
            finally
            {
                stopwatch.Stop();
            }

            // Raise the after event
            await engine.Events.RaiseAsync(new AfterPipelinePhaseExecution(executionId, PipelineName, Phase, Outputs, stopwatch.ElapsedMilliseconds));

            // Record the results
            PhaseResult phaseResult = new PhaseResult(PipelineName, Phase, Outputs, stopwatch.ElapsedMilliseconds);
            phaseResults.AddOrUpdate(
                phaseResult.PipelineName,
                _ =>
                {
                    PhaseResult[] results = new PhaseResult[4];
                    results[(int)phaseResult.Phase] = phaseResult;
                    return results;
                },
                (_, results) =>
                {
                    if (results[(int)phaseResult.Phase] != null)
                    {
                        // Sanity check, we should never hit this
                        throw new InvalidOperationException($"Results for phase {phaseResult.Phase} have already been added");
                    }
                    results[(int)phaseResult.Phase] = phaseResult;
                    return results;
                });
        }

        public void Dispose()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(Pipeline));
            }
            _disposed = true;

            DisposeModules(_modules);
        }

        private static void DisposeModules(IEnumerable<IModule> modules)
        {
            foreach (IModule module in modules)
            {
                (module as IDisposable)?.Dispose();
                if (module is IEnumerable<IModule> childModules)
                {
                    DisposeModules(childModules);
                }
            }
        }
    }
}
