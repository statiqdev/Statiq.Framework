using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace Statiq.Testing
{
    public class TestServiceProvider : IServiceProvider
    {
        private IServiceProvider _services;

        public TestServiceProvider(Action<IServiceCollection> configure = null)
        {
            IServiceCollection serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton<IServiceScopeFactory>(new TestServiceScopeFactory(this));
            configure?.Invoke(serviceCollection);
            _services = serviceCollection.BuildServiceProvider();
        }

        public object GetService(Type serviceType) => _services.GetService(serviceType);
    }
}
