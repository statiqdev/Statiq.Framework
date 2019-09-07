using System;
using System.Collections.Generic;
using Statiq.Common;

namespace Statiq.Core
{
    public class Pipeline : IPipeline
    {
        /// <inheritdoc/>
        public ModuleList InputModules { get; } = new ModuleList();

        /// <inheritdoc/>
        public ModuleList ProcessModules { get; } = new ModuleList();

        /// <inheritdoc/>
        public ModuleList TransformModules { get; } = new ModuleList();

        /// <inheritdoc/>
        public ModuleList OutputModules { get; } = new ModuleList();

        /// <inheritdoc/>
        public HashSet<string> Dependencies { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        /// <inheritdoc/>
        public bool Isolated { get; set; }

        /// <inheritdoc/>
        public bool AlwaysProcess { get; set; }
    }
}
