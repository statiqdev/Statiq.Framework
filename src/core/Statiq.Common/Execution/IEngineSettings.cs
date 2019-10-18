using System.Collections;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Configuration;

namespace Statiq.Common
{
    public interface IEngineSettings : IMetadata
    {
        IConfiguration Configuration { get; }
    }
}
