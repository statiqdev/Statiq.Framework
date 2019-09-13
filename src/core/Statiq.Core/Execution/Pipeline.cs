using System;
using System.Collections.Generic;
using Statiq.Common;

namespace Statiq.Core
{
    /// <summary>
    /// A base class for custom pipelines.
    /// </summary>
    public class Pipeline : IPipeline
    {
        private ModuleList _inputModules = new ModuleList();
        private ModuleList _processModules = new ModuleList();
        private ModuleList _transformModules = new ModuleList();
        private ModuleList _outputModules = new ModuleList();
        private HashSet<string> _dependencies = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        /// <inheritdoc/>
        public ModuleList InputModules
        {
            get => _inputModules;
            set => _inputModules = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <inheritdoc/>
        public ModuleList ProcessModules
        {
            get => _processModules;
            set => _processModules = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <inheritdoc/>
        public ModuleList TransformModules
        {
            get => _transformModules;
            set => _transformModules = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <inheritdoc/>
        public ModuleList OutputModules
        {
            get => _outputModules;
            set => _outputModules = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <inheritdoc/>
        public HashSet<string> Dependencies
        {
            get => _dependencies;
            set => _dependencies = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <inheritdoc/>
        public bool Isolated { get; set; }

        /// <inheritdoc/>
        public bool AlwaysProcess { get; set; }
    }
}
