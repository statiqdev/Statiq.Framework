using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Spectre.Console.Cli;

namespace Statiq.App
{
    // Wraps a standalone service collection just for command use
    internal class CommandServiceTypeRegistrar : ITypeRegistrar
    {
        private readonly IServiceCollection _serviceCollection = new ServiceCollection();

        public void Register(Type service, Type implementation) =>
            _serviceCollection.AddScoped(service, implementation);

        public void RegisterInstance(Type service, object implementation) =>
            _serviceCollection.AddScoped(service, _ => implementation);

        public ITypeResolver Build() => new CommandServiceTypeResolver(_serviceCollection.BuildServiceProvider());
    }
}
