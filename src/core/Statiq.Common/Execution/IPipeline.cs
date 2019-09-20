using System;
using System.Collections.Generic;

namespace Statiq.Common
{
    /// <summary>
    /// Represents a named collection of modules that should be executed by the engine.
    /// </summary>
    /// <remarks>
    /// If the pipeline implements <see cref="IDisposable"/>, <see cref="IDisposable.Dispose"/>
    /// will be called when the engine is disposed (I.e., on application exit).
    /// </remarks>
    public interface IPipeline
    {
        ModuleList InputModules { get; }

        ModuleList ProcessModules { get; }

        ModuleList TransformModules { get; }

        ModuleList OutputModules { get; }

        HashSet<string> Dependencies { get; }

        bool Isolated { get; set; }

        bool AlwaysProcess { get; set; }
    }
}
