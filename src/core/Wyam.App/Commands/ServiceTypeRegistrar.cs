using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Cli;

namespace Wyam.App.Commands
{
    internal class ServiceTypeRegistrar : ITypeRegistrar
    {
        private readonly IServiceCollection _serviceCollection;
        private readonly Func<IServiceCollection, IServiceProvider> _buildServiceProvider;

        public ServiceTypeRegistrar(
            IServiceCollection serviceCollection,
            Func<IServiceCollection, IServiceProvider> buildServiceProvider)
        {
            _serviceCollection = serviceCollection ?? throw new ArgumentNullException(nameof(serviceCollection));
            _buildServiceProvider = buildServiceProvider ?? throw new ArgumentNullException(nameof(buildServiceProvider));
        }

        public void Register(Type service, Type implementation) =>
            _serviceCollection.AddScoped(service, implementation);

        public void RegisterInstance(Type service, object implementation) =>
            _serviceCollection.AddScoped(service, _ => implementation);

        public ITypeResolver Build() => new ServiceTypeResolver(_buildServiceProvider(_serviceCollection));
    }
}
