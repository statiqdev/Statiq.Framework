using System.Threading.Tasks;
using Spectre.Cli;
using Statiq.Common;

namespace Statiq.App
{
    public interface IBootstrapper : IConfigurableBootstrapper
    {
        string[] Args { get; }

        IBootstrapper SetDefaultCommand<TCommand>()
            where TCommand : class, ICommand;

        Task<int> RunAsync();
    }
}
