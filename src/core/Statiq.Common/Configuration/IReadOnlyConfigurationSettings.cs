using Microsoft.Extensions.Configuration;

namespace Statiq.Common
{
    public interface IReadOnlyConfigurationSettings : IMetadata
    {
        IConfiguration Configuration { get; }
    }
}
