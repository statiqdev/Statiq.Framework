using System;
using System.Collections.Generic;
using Statiq.Common;

namespace Statiq.App
{
    public class ConfigurableCommands : IConfigurable
    {
        private readonly Spectre.Cli.IConfigurator _commandConfigurator;
        private readonly Dictionary<Type, string> _commandNames;

        internal ConfigurableCommands(
            Spectre.Cli.IConfigurator commandConfigurator,
            Dictionary<Type, string> commandNames)
        {
            _commandConfigurator = commandConfigurator;
            _commandNames = commandNames;
        }

        public void AddCommand<TCommand>(string name)
            where TCommand : class, Spectre.Cli.ICommand
        {
            _commandConfigurator.AddCommand<TCommand>(name);
            _commandNames.Add(typeof(TCommand), name);
        }
    }
}
