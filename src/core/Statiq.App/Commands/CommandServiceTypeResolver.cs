using System;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Cli;

namespace Statiq.App
{
    internal class CommandServiceTypeResolver : ITypeResolver
    {
        private readonly IServiceProvider _serviceProvider;

        public CommandServiceTypeResolver(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        public object Resolve(Type type) => _serviceProvider.GetRequiredService(type);
    }
}
