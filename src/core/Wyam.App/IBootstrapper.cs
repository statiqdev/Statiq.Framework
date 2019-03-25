using System.Collections.Generic;
using System.Threading.Tasks;
using Spectre.Cli;
using Wyam.App.Configuration;
using Wyam.Common.Configuration;
using Wyam.Common.Execution;

namespace Wyam.App
{
    public interface IBootstrapper : IConfigurableBootstrapper
    {
        string[] Args { get; }

        void SetDefaultCommand<TCommand>()
            where TCommand : class, ICommand;

        Task<int> RunAsync();
    }
}
