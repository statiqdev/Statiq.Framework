using System;
using System.Collections.Generic;
using System.Text;
using Statiq.Common;

namespace Statiq.Core
{
    public static class IModuleExtensions
    {
        public static ForEachDocument ForEachDocument(this IModule module) => new ForEachDocument(module);
    }
}
