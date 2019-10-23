using Microsoft.Extensions.Configuration;
using Statiq.Common;

namespace Statiq.Core
{
    internal class ReadOnlyConfigurationSettings : ConfigurationMetadata, IReadOnlyConfigurationSettings
    {
        public ReadOnlyConfigurationSettings(IConfiguration configuration)
            : base(configuration)
        {
        }
    }
}
