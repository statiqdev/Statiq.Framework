using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Wyam.App.Tracing;
using Wyam.Common.Configuration;
using Wyam.Common.Execution;

namespace Wyam.App.Configuration
{
    public class DelegateConfigurator<T> : IConfigurator<T>
        where T : class
    {
        private readonly Action<T> _action;

        public DelegateConfigurator(Action<T> action)
        {
            _action = action;
        }

        public void Configure(T configurable) => _action?.Invoke(configurable);
    }
}
