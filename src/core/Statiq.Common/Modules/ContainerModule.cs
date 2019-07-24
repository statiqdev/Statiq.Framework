using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Statiq.Common
{
    /// <summary>
    /// A base class for modules that contain a collection of child modules.
    /// </summary>
    public abstract class ContainerModule : IContainerModule
    {
        /// <summary>
        /// Creates a new container module with the specified child modules.
        /// Any <c>null</c> items in the sequence of modules will be discarded.
        /// </summary>
        /// <param name="children">The child modules.</param>
        protected ContainerModule(params IModule[] children)
        {
            Children = new ModuleList(children);
        }

        /// <inheritdoc />
        public IModuleList Children { get; }

        /// <inheritdoc />
        public void Add(params IModule[] modules) => Children.Add(modules);

        /// <inheritdoc />
        public IEnumerator<IModule> GetEnumerator() => Children.GetEnumerator();

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <inheritdoc />
        public abstract Task<IEnumerable<IDocument>> ExecuteAsync(IExecutionContext context);
    }
}
