using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Cli;

namespace Wyam.App.Commands
{
    internal class ServiceTypeRegistrar : ITypeRegistrar
    {
        private readonly ServiceCollection _serviceCollection;

        public ServiceTypeRegistrar(ServiceCollection serviceCollection)
        {
            _serviceCollection = serviceCollection ?? throw new ArgumentNullException(nameof(serviceCollection));
        }

        public void Register(Type service, Type implementation) =>
            _serviceCollection.AddScoped(service, implementation);

        public void RegisterInstance(Type service, object implementation) =>
            _serviceCollection.AddScoped(service, _ => implementation);

        public ITypeResolver Build() => new ServiceTypeResolver(_serviceCollection.BuildServiceProvider());
    }
}
