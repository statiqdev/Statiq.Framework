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
        IModuleList ReadModules { get; }

        IModuleList ProcessModules { get; }

        IModuleList RenderModules { get; }

        IModuleList WriteModules { get; }

        HashSet<string> Dependencies { get; }

        bool Isolated { get; set; }

        bool AlwaysProcess { get; set; }
    }
}
