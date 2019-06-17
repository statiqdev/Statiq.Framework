using System.Collections.Generic;
using Statiq.Common.Modules;

namespace Statiq.Common.Execution
{
    /// <summary>
    /// Represents a named collection of modules that should be executed by the engine.
    /// </summary>
    public interface IPipeline
    {
        IModuleList InputModules { get; }

        IModuleList ProcessModules { get; }

        IModuleList TransformModules { get; }

        IModuleList OutputModules { get; }

        HashSet<string> Dependencies { get; }

        bool Isolated { get; set; }

        bool AlwaysProcess { get; set; }
    }
}
