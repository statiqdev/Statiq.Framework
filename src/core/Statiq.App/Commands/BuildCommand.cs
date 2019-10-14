using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Cli;
using Statiq.Common;

namespace Statiq.App
{
    [Description("Builds the site.")]
    public class BuildCommand : BaseCommand<BuildCommand.Settings>
    {
        public class Settings : BaseSettings
        {
            [CommandOption("-i|--input")]
            [Description("The path(s) of input files, can be absolute or relative to the current folder.")]
            public string[] InputPaths { get; set; }

            [CommandOption("-o|--output")]
            [Description("The path to output files, can be absolute or relative to the current folder.")]
            public string OutputPath { get; set; }

            [CommandOption("--noclean")]
            [Description("Prevents cleaning of the output path on each execution.")]
            public bool NoClean { get; set; }

            [CommandOption("--nocache")]
            [Description("Prevents caching information during execution (less memory usage but slower execution).")]
            public bool NoCache { get; set; }

            [CommandOption("--stdin")]
            [Description("Reads standard input at startup and sets ApplicationInput in the execution context.")]
            public bool StdIn { get; set; }

            [CommandOption("-s|--setting")]
            [Description("Specifies a setting as a key=value pair. Use the syntax [x,y] to specify an array value.")]
            public string[] MetadataSettings { get; set; }

            [CommandOption("-p|--pipeline")]
            [Description("Explicitly specifies one or more pipelines to execute.")]
            public string[] Pipelines { get; set; }

            [CommandOption("-d|--defaults")]
            [Description("Executes default pipelines in addition to the ones specified.")]
            public bool DefaultPipelines { get; set; }

            [CommandOption("--serial")]
            [Description("Executes pipeline phases and modules in serial.")]
            public bool SerialExecution { get; set; }

            [CommandArgument(0, "[root]")]
            [Description("The root folder to use.")]
            public string RootPath { get; set; }
        }

        private readonly IConfiguration _configuration;
        private readonly IServiceCollection _serviceCollection;
        private readonly IBootstrapper _bootstrapper;

        public BuildCommand(IConfiguration configuration, IServiceCollection serviceCollection, IBootstrapper bootstrapper)
            : base(serviceCollection)
        {
            _configuration = configuration;
            _serviceCollection = serviceCollection;
            _bootstrapper = bootstrapper;
        }

        public override async Task<int> ExecuteCommandAsync(CommandContext context, Settings settings)
        {
            using (CancellationTokenSource cancellationTokenSource = new CancellationTokenSource())
            {
                using (EngineManager engineManager = new EngineManager(
                    _configuration,
                    _serviceCollection,
                    _bootstrapper,
                    this,
                    settings))
                {
                    return await engineManager.ExecuteAsync(cancellationTokenSource)
                        ? (int)ExitCode.Normal
                        : (int)ExitCode.ExecutionError;
                }
            }
        }
    }
}
