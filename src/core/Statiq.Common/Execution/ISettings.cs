using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace Statiq.Common
{
    public interface ISettings : IMetadata
    {
        IConfiguration Configuration { get; }
    }
}
