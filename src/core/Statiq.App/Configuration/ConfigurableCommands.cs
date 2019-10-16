using System;
using System.Collections.Generic;
using Statiq.Common;

namespace Statiq.App
{
    public class ConfigurableCommands : IConfigurable
    {
        internal ConfigurableCommands(Spectre.Cli.IConfigurator configurator)
        {
            Configurator = configurator;
        }

        public Spectre.Cli.IConfigurator Configurator { get; }
    }
}
