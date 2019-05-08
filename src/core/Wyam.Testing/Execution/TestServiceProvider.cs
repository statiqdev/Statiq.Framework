using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace Wyam.Testing.Execution
{
    public class TestServiceProvider : IServiceProvider
    {
        public Dictionary<Type, object> Services { get; } = new Dictionary<Type, object>();

        public TestServiceProvider()
        {
            Services[typeof(IServiceScopeFactory)] = new TestServiceScopeFactory(this);
        }

        public object GetService(Type serviceType) =>
            Services.TryGetValue(serviceType, out object value) ? value : null;
    }
}
