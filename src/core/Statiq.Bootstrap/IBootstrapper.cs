using System.Threading.Tasks;
using Spectre.Cli;
using Statiq.Common.Configuration;

namespace Statiq.Bootstrap
{
    public interface IBootstrapper : IConfigurableBootstrapper
    {
        string[] Args { get; }

        void SetDefaultCommand<TCommand>()
            where TCommand : class, ICommand;

        Task<int> RunAsync();
    }
}
