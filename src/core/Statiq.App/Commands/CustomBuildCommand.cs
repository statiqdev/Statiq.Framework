using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Cli;
using Statiq.Common;

namespace Statiq.App
{
    public abstract class CustomBuildCommand : BaseCommand<BaseSettings>
    {
        private readonly IServiceCollection _serviceCollection;
        private readonly IBootstrapper _bootstrapper;
        private readonly BuildCommand.Settings _settings;

        public CustomBuildCommand(IServiceCollection serviceCollection, IBootstrapper bootstrapper)
            : this((BuildCommand.Settings)null, serviceCollection, bootstrapper)
        {
        }

        public CustomBuildCommand(string[] pipelines, IServiceCollection serviceCollection, IBootstrapper bootstrapper)
            : this(new BuildCommand.Settings { Pipelines = pipelines }, serviceCollection, bootstrapper)
        {
        }

        public CustomBuildCommand(string[] pipelines, bool defaultPipelines, IServiceCollection serviceCollection, IBootstrapper bootstrapper)
            : this(new BuildCommand.Settings { Pipelines = pipelines, DefaultPipelines = defaultPipelines }, serviceCollection, bootstrapper)
        {
        }

        public CustomBuildCommand(BuildCommand.Settings settings, IServiceCollection serviceCollection, IBootstrapper bootstrapper)
            : base(serviceCollection)
        {
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
                    return await engineManager.ExecuteAsync(cancellationTokenSource)
                        ? (int)ExitCode.Normal
                        : (int)ExitCode.ExecutionError;
                }
            }
        }
    }
}
