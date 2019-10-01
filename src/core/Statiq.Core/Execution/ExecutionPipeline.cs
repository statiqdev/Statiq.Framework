using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Statiq.Common;

namespace Statiq.Core
{
    public abstract class ExecutionPipeline : Common.Module, IPipeline
    {
        protected ExecutionPipeline()
        {
            Dependencies = new HashSet<string>();
            if (GetType()
                .GetMethod(nameof(ExecuteInputAsync), BindingFlags.Instance | BindingFlags.NonPublic)
                .DeclaringType != typeof(ExecutionPipeline))
            {
                InputModules.Add(this);
            }
            if (GetType()
                .GetMethod(nameof(ExecuteProcessPhaseAsync), BindingFlags.Instance | BindingFlags.NonPublic)
                .DeclaringType != typeof(ExecutionPipeline))
            {
                ProcessModules.Add(this);
            }
            if (GetType()
                .GetMethod(nameof(ExecuteTransformPhaseAsync), BindingFlags.Instance | BindingFlags.NonPublic)
                .DeclaringType != typeof(ExecutionPipeline))
            {
                TransformModules.Add(this);
            }
            if (GetType()
                .GetMethod(nameof(ExecuteOutputPhaseAsync), BindingFlags.Instance | BindingFlags.NonPublic)
                .DeclaringType != typeof(ExecutionPipeline))
            {
                OutputModules.Add(this);
            }
        }

        /// <inheritdoc/>
        public ModuleList InputModules { get; } = new ModuleList();

        /// <inheritdoc/>
        public ModuleList ProcessModules { get; } = new ModuleList();

        /// <inheritdoc/>
        public ModuleList TransformModules { get; } = new ModuleList();

        /// <inheritdoc/>
        public ModuleList OutputModules { get; } = new ModuleList();

        /// <inheritdoc/>
        public virtual HashSet<string> Dependencies { get; } = new HashSet<string>();

        /// <inheritdoc/>
        public virtual bool Isolated { get; set; }

        /// <inheritdoc/>
        public virtual ExecutionPolicy ExecutionPolicy { get; set; }

        protected sealed override Task<IEnumerable<IDocument>> ExecuteContextAsync(IExecutionContext context) =>
            context.Phase switch
            {
                Phase.Input => ExecuteInputPhaseAsync(context),
                Phase.Process => ExecuteProcessPhaseAsync(context),
                Phase.Transform => ExecuteTransformPhaseAsync(context),
                Phase.Output => ExecuteOutputPhaseAsync(context),
                _ => base.ExecuteContextAsync(context),
            };

        /// <inheritdoc />
        // Unused, prevent overriding in derived classes
        protected sealed override Task<IEnumerable<IDocument>> ExecuteInputAsync(IDocument input, IExecutionContext context) =>
            throw new NotSupportedException();

        protected virtual Task<IEnumerable<IDocument>> ExecuteInputPhaseAsync(IExecutionContext context) =>
            throw new NotImplementedException();

        protected virtual Task<IEnumerable<IDocument>> ExecuteProcessPhaseAsync(IExecutionContext context) =>
            throw new NotImplementedException();

        protected virtual Task<IEnumerable<IDocument>> ExecuteTransformPhaseAsync(IExecutionContext context) =>
            throw new NotImplementedException();

        protected virtual Task<IEnumerable<IDocument>> ExecuteOutputPhaseAsync(IExecutionContext context) =>
            throw new NotImplementedException();
    }
}
