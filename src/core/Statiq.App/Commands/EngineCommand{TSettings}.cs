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
    internal abstract class EngineCommand<TSettings> : BaseCommand<TSettings>
        where TSettings : EngineCommandSettings
    {
        protected EngineCommand(SettingsConfigurationProvider settingsProvider, IConfiguration configuration, IServiceCollection serviceCollection, IBootstrapper bootstrapper)
            : base(serviceCollection)
        {
            SettingsProvider = settingsProvider;
            Configuration = configuration;
            ServiceCollection = serviceCollection;
            Bootstrapper = bootstrapper;
        }

        public SettingsConfigurationProvider SettingsProvider { get; }

        public IConfiguration Configuration { get; }

        public IServiceCollection ServiceCollection { get; }

        public IBootstrapper Bootstrapper { get; }
    }
}
