using System.Collections.Generic;
using Spectre.Cli;
using Wyam.App.Configuration;
using Wyam.Common.Configuration;
using Wyam.Common.Execution;

namespace Wyam.App
{
    public interface ICommandBootstrapper : IBootstrapper
    {
        string[] Args { get; }

        void SetDefaultCommand<TCommand>()
            where TCommand : class, ICommand;

        int Run();
    }
}
