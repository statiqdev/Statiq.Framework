using System.Collections.Generic;
using System.Threading.Tasks;

namespace Statiq.Common
{
    /// <summary>
    /// A base class for modules that contain a collection of child modules.
    /// </summary>
    public abstract class ContainerModule : IModule
    {
        /// <summary>
        /// Creates a new container module with the specified child modules.
        /// Any <c>null</c> items in the sequence of modules will be discarded.
        /// </summary>
        /// <param name="children">The child modules.</param>
        protected ContainerModule(IEnumerable<IModule> children)
        {
            Children = (children as IModuleList) ?? new ModuleList(children);
        }

        public IModuleList Children { get; }

        /// <inheritdoc />
        public abstract Task<IEnumerable<IDocument>> ExecuteAsync(IReadOnlyList<IDocument> inputs, IExecutionContext context);
    }
}
