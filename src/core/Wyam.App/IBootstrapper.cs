using System.Threading.Tasks;
using Spectre.Cli;
using Wyam.Common.Configuration;

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
