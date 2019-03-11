using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Cli;

namespace Wyam.App.Commands
{

    internal class ServiceTypeResolver : ITypeResolver
    {
        private readonly IServiceProvider _serviceProvider;

        public ServiceTypeResolver(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        public object Resolve(Type type) => _serviceProvider.GetRequiredService(type);
    }
}
