using System.Collections.Generic;

namespace Wyam.Common.Modules
{
    /// <summary>
    /// A collection of optionally named modules.
    /// </summary>
    public interface IReadOnlyModuleList : IReadOnlyList<IModule>
    {
    }
}