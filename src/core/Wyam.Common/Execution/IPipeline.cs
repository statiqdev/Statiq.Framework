using System.Collections.Generic;
using Wyam.Common.Documents;
using Wyam.Common.Modules;

namespace Wyam.Common.Execution
{
    /// <summary>
    /// Represents a named collection of modules that should be executed by the engine.
    /// </summary>
    public interface IPipeline
    {
        IModuleList Read { get; }

        IModuleList Process { get; }

        IModuleList Render { get; }

        IModuleList Write { get; }

        HashSet<IPipeline> Dependencies { get; }

        bool Isolated { get; set; }

        bool AlwaysProcess { get; set; }
    }
}
