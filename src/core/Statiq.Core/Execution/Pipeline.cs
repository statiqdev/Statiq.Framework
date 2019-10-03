using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Statiq.Common;

namespace Statiq.Core
{
    /// <summary>
    /// A base class for custom pipelines.
    /// </summary>
    /// <remarks>
    /// Use the derived constructor to initialize the phases and other properties.
    /// </remarks>
    public class Pipeline : IPipeline
    {
        private ModuleList _inputModules;
        private ModuleList _processModules;
        private ModuleList _transformModules;
        private ModuleList _outputModules;
        private HashSet<string> _dependencies;

        /// <inheritdoc/>
        public ModuleList InputModules
        {
            get => _inputModules ?? (_inputModules = new ModuleList());
            set => _inputModules = value;
        }

        /// <inheritdoc/>
        public ModuleList ProcessModules
        {
            get => _processModules ?? (_processModules = new ModuleList());
            set => _processModules = value;
        }

        /// <inheritdoc/>
        public ModuleList TransformModules
        {
            get => _transformModules ?? (_transformModules = new ModuleList());
            set => _transformModules = value;
        }

        /// <inheritdoc/>
        public ModuleList OutputModules
        {
            get => _outputModules ?? (_outputModules = new ModuleList());
            set => _outputModules = value;
        }

        /// <inheritdoc/>
        public HashSet<string> Dependencies
        {
            get => _dependencies ?? (_dependencies = new HashSet<string>(StringComparer.OrdinalIgnoreCase));
            set => _dependencies = value;
        }

        /// <inheritdoc/>
        public bool Isolated { get; set; }

        /// <inheritdoc/>
        public bool Deployment { get; set; }

        /// <inheritdoc/>
        public ExecutionPolicy ExecutionPolicy { get; set; }
    }
}
