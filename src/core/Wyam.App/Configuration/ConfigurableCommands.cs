using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Wyam.Common.Configuration;

namespace Wyam.App.Configuration
{
    public class ConfigurableCommands : IConfigurable
    {
        internal ConfigurableCommands(Spectre.Cli.IConfigurator commandConfigurator) => CommandConfigurator = commandConfigurator;

        public Spectre.Cli.IConfigurator CommandConfigurator { get; }
    }
}
