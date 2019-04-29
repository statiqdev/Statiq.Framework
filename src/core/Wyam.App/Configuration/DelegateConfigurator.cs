using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Wyam.App.Tracing;
using Wyam.Common.Configuration;
using Wyam.Common.Execution;

namespace Wyam.App.Configuration
{
    public class DelegateConfigurator<TConfigurable> : IConfigurator<TConfigurable>
        where TConfigurable : IConfigurable
    {
        private readonly Action<TConfigurable> _action;

        public DelegateConfigurator(Action<TConfigurable> action)
        {
            _action = action;
        }

        public void Configure(TConfigurable configurable) => _action?.Invoke(configurable);
    }
}
