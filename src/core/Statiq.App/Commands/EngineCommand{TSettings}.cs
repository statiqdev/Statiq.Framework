using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NetEscapades.Extensions.Logging.RollingFile;
using Spectre.Cli;
using Statiq.Common;
using Statiq.Core;

namespace Statiq.App
{
    internal abstract class EngineCommand<TSettings> : BaseCommand<TSettings>, IEngineCommand
        where TSettings : BaseCommandSettings
    {
        protected EngineCommand(SettingsConfigurationProvider settingsProvider, IConfigurationRoot configurationRoot, IServiceCollection serviceCollection, IBootstrapper bootstrapper)
            : base(serviceCollection)
        {
            SettingsProvider = settingsProvider;
            ConfigurationRoot = configurationRoot;
            ServiceCollection = serviceCollection;
            Bootstrapper = bootstrapper;
        }

        public SettingsConfigurationProvider SettingsProvider { get; }

        public IConfigurationRoot ConfigurationRoot { get; }

        public IServiceCollection ServiceCollection { get; }

        public IBootstrapper Bootstrapper { get; }
    }
}
