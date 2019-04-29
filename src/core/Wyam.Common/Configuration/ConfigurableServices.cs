using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;

namespace Wyam.Common.Configuration
{
    public class ConfigurableServices : IConfigurable
    {
        public ConfigurableServices(IServiceCollection services) => Services = services;

        public IServiceCollection Services { get; }
    }
}
