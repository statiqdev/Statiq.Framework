using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Cli;
using Statiq.Common;

namespace Statiq.App
{
    public abstract class SimpleBuildCommand : BaseCommand<BaseSettings>
    {
        private readonly IServiceCollection _serviceCollection;
        private readonly IBootstrapper _bootstrapper;
        private readonly string[] _pipelines;
        private readonly BuildCommand.Settings _settings;

        public SimpleBuildCommand(IServiceCollection serviceCollection, IBootstrapper bootstrapper)
            : this(null, null, serviceCollection, bootstrapper)
        {
        }

        public SimpleBuildCommand(string[] pipelines, IServiceCollection serviceCollection, IBootstrapper bootstrapper)
            : this(pipelines, null, serviceCollection, bootstrapper)
        {
        }

        public SimpleBuildCommand(string[] pipelines, BuildCommand.Settings settings, IServiceCollection serviceCollection, IBootstrapper bootstrapper)
            : base(serviceCollection)
        {
            _pipelines = pipelines;
            _settings = settings;
            _serviceCollection = serviceCollection;
            _bootstrapper = bootstrapper;
        }

        public override async Task<int> ExecuteCommandAsync(CommandContext context, BaseSettings settings)
        {
            using (CancellationTokenSource cancellationTokenSource = new CancellationTokenSource())
            {
                using (EngineManager engineManager = new EngineManager(_serviceCollection, _bootstrapper, this, _settings))
                {
                    return await engineManager.ExecuteAsync(_pipelines, cancellationTokenSource)
                        ? (int)ExitCode.Normal
                        : (int)ExitCode.ExecutionError;
                }
            }
        }
    }
}
