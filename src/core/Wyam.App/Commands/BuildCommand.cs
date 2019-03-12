using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;
using Spectre.Cli;
using Wyam.App.Configuration;
using Wyam.Common.Configuration;
using Wyam.Common.Execution;
using Wyam.Core.Execution;

namespace Wyam.App.Commands
{
    public class BuildCommand : Command<BuildCommand.Settings>
    {
        public class Settings : CommandSettings
        {
            [CommandOption("-i|--input")]
            [Description("The path(s) of input files, can be absolute or relative to the current folder.")]
            public string InputPaths { get; set; }
        }

        private readonly IConfigurableBootstrapper _bootstrapper;
        private readonly IServiceProvider _serviceProvider;

        public BuildCommand(IConfigurableBootstrapper bootstrapper, IServiceProvider serviceProvider)
        {
            _bootstrapper = bootstrapper;
            _serviceProvider = serviceProvider;
        }

        public override int Execute(CommandContext context, Settings settings)
        {
            using (Engine engine = new Engine())
            {
                // Run configurators after command line has been applied
                _bootstrapper.Configurators.Configure<IEngine>(engine);

                // Make sure we clear out anything in the JavaScriptEngineSwitcher instance
                Engine.ResetJsEngines();

                // Execute the engine
                try
                {
                    engine.Execute(_serviceProvider);
                }
                catch (Exception)
                {
                    return (int)ExitCode.ExecutionError;
                }

                return (int)ExitCode.Normal;
            }
        }
    }
}
