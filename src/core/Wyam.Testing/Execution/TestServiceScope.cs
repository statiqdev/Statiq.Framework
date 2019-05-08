using System;
using Microsoft.Extensions.DependencyInjection;

namespace Wyam.Testing.Execution
{
    public class TestServiceScope : IServiceScope
    {
        public TestServiceScope(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
        }

        public IServiceProvider ServiceProvider { get; }

        public void Dispose()
        {
        }
    }
}
