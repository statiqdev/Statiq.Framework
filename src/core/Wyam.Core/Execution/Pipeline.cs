using System;
using System.Collections.Generic;
using Wyam.Common.Modules;
using Wyam.Common.Execution;

namespace Wyam.Core.Execution
{
    public class Pipeline : IPipeline
    {
        public IModuleList ReadModules { get; } = new ModuleList();

        public IModuleList ProcessModules { get; } = new ModuleList();

        public IModuleList RenderModules { get; } = new ModuleList();

        public IModuleList WriteModules { get; } = new ModuleList();

        public HashSet<string> Dependencies { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        public bool Isolated { get; set; }

        public bool AlwaysProcess { get; set; }
    }
}
