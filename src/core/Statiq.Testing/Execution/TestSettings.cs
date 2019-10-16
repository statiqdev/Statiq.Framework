using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Statiq.Common;

namespace Statiq.Testing
{
    public class TestSettings : ConfigurationMetadata, ISettings
    {
        public TestSettings(IConfiguration configuration)
            : base(configuration ?? new ConfigurationRoot(Array.Empty<IConfigurationProvider>()))
        {
        }

        public void SetConfiguration(IConfiguration configuration) =>
            Configuration = configuration ?? new ConfigurationRoot(Array.Empty<IConfigurationProvider>());

        public void SetConfigurationData(IEnumerable<KeyValuePair<string, string>> data) =>
            SetConfiguration(data == null ? null : new ConfigurationBuilder().AddInMemoryCollection(data).Build());
    }
}
