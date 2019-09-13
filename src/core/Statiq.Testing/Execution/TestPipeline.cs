using System;
using System.Collections.Generic;
using Statiq.Common;

namespace Statiq.Testing
{
    /// <summary>
    /// A pipeline with utility methods for easier testing.
    /// </summary>
    public class TestPipeline : IPipeline
    {
        public ModuleList InputModules { get; } = new ModuleList();

        public ModuleList ProcessModules { get; } = new ModuleList();

        public ModuleList TransformModules { get; } = new ModuleList();

        public ModuleList OutputModules { get; } = new ModuleList();

        public HashSet<string> Dependencies { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        public bool Isolated { get; set; }

        public bool AlwaysProcess { get; set; }

        public TestPipeline(params IModule[] processModules)
        {
            ICollectionExtensions.AddRange(ProcessModules, processModules);
        }

        public TestPipeline(IEnumerable<IModule> processModules)
        {
            ICollectionExtensions.AddRange(ProcessModules, processModules);
        }
    }
}
