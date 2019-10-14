using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace Statiq.Common
{
    public static class IConfigurationExtensions
    {
        public static IMetadata AsMetadata(this IConfiguration configuration) =>
            new ConfigurationMetadata(configuration);
    }
}
