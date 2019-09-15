using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Spectre.Cli;
using Statiq.Common;

namespace Statiq.App
{
    public partial interface IBootstrapper : IConfigurableBootstrapper
    {
        /// <summary>
        /// The command line arguments.
        /// </summary>
        string[] Arguments { get; }

        /// <summary>
        /// Sets the default command used by the application.
        /// </summary>
        /// <typeparam name="TCommand">The type of default command.</typeparam>
        /// <returns>The current bootstrapper.</returns>
        IBootstrapper SetDefaultCommand<TCommand>()
            where TCommand : class, ICommand;

        /// <summary>
        /// All of the registered commands by name.
        /// </summary>
        IReadOnlyDictionary<Type, string> CommandNames { get; }

        /// <summary>
        /// Runs the command specified by the command line arguments.
        /// </summary>
        /// <returns>The resulting exit code (see <see cref="ExitCode"/>).</returns>
        Task<int> RunAsync();
    }
}
