using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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

            [CommandArgument(0, "[root]")]
            [Description("The root folder to use.")]
            public string RootPath { get; set; }
        }

        private readonly IConfiguratorCollection _configurators;

        public BuildCommand(IServiceCollection serviceCollection, IConfiguratorCollection configurators)
            : base(serviceCollection)
        {
            _configurators = configurators;
        }

        public override async Task<int> ExecuteCommandAsync(IServiceProvider services, CommandContext context, Settings settings)
        {
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            using (EngineManager engineManager = new EngineManager(services, _configurators, settings))
            {
                return await engineManager.ExecuteAsync(cancellationTokenSource)
                    ? (int)ExitCode.Normal
                    : (int)ExitCode.ExecutionError;
            }
        }
    }
}
