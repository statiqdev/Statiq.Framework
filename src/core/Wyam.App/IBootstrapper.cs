using System.Collections.Generic;
using Spectre.Cli;
using Wyam.App.Configuration;
using Wyam.Common.Execution;

namespace Wyam.App
{
    public interface IBootstrapper
    {
        string[] Args { get; }

        ConfiguratorCollection<IEngine> EngineConfigurators { get; }

        ConfiguratorCollection<IConfigurator> CommandConfigurators { get; }

        void SetDefaultCommand<TCommand>()
            where TCommand : class, ICommand;

        int Run();
    }
}
