using System.Collections;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Configuration;

namespace Statiq.Common
{
    public interface IReadOnlyConfigurationSettings : IMetadata
    {
        IConfiguration Configuration { get; }
    }
}
