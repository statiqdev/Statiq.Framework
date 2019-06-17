using System;
using System.Collections.Generic;
using Statiq.Common.Modules;
using Statiq.Common.Execution;

namespace Statiq.Core.Execution
{
    public class Pipeline : IPipeline
    {
        /// <inheritdoc/>
        public IModuleList InputModules { get; } = new ModuleList();

        /// <inheritdoc/>
        public IModuleList ProcessModules { get; } = new ModuleList();

        /// <inheritdoc/>
        public IModuleList TransformModules { get; } = new ModuleList();

        /// <inheritdoc/>
        public IModuleList OutputModules { get; } = new ModuleList();

        /// <inheritdoc/>
        public HashSet<string> Dependencies { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        /// <inheritdoc/>
        public bool Isolated { get; set; }

        /// <inheritdoc/>
        public bool AlwaysProcess { get; set; }
    }
}
