using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Cli;
using Statiq.Common;

namespace Statiq.App
{
    public abstract class CustomBuildCommand : BaseCommand<BaseSettings>
    {
        private readonly IConfiguration _configuration;
        private readonly IServiceCollection _serviceCollection;
        private readonly IBootstrapper _bootstrapper;
        private readonly BuildCommand.Settings _settings;

        public CustomBuildCommand(IConfiguration configuration, IServiceCollection serviceCollection, IBootstrapper bootstrapper)
            : this((BuildCommand.Settings)null, configuration, serviceCollection, bootstrapper)
        {
        }

        public CustomBuildCommand(string[] pipelines, IConfiguration configuration, IServiceCollection serviceCollection, IBootstrapper bootstrapper)
            : this(new BuildCommand.Settings { Pipelines = pipelines }, configuration, serviceCollection, bootstrapper)
        {
        }

        public CustomBuildCommand(string[] pipelines, bool defaultPipelines, IConfiguration configuration, IServiceCollection serviceCollection, IBootstrapper bootstrapper)
            : this(new BuildCommand.Settings { Pipelines = pipelines, DefaultPipelines = defaultPipelines }, configuration, serviceCollection, bootstrapper)
        {
        }

        public CustomBuildCommand(BuildCommand.Settings settings, IConfiguration configuration, IServiceCollection serviceCollection, IBootstrapper bootstrapper)
            : base(serviceCollection)
        {
            _settings = settings;
            _configuration = configuration;
            _serviceCollection = serviceCollection;
            _bootstrapper = bootstrapper;
        }

        public override async Task<int> ExecuteCommandAsync(CommandContext context, BaseSettings settings)
        {
            using (CancellationTokenSource cancellationTokenSource = new CancellationTokenSource())
            {
                using (EngineManager engineManager = new EngineManager(_configuration, _serviceCollection, _bootstrapper, this, _settings))
                {
                    return await engineManager.ExecuteAsync(cancellationTokenSource)
                        ? (int)ExitCode.Normal
                        : (int)ExitCode.ExecutionError;
                }
            }
        }
    }
}
