using System.Collections.Generic;

namespace Statiq.Common.Modules
{
    /// <summary>
    /// A collection of optionally named modules.
    /// </summary>
    public interface IModuleList : IList<IModule>
    {
        /// <summary>
        /// Adds modules to the list.
        /// Any <c>null</c> items in the sequence of modules will be discarded.
        /// </summary>
        /// <param name="modules">The modules to add.</param>
        void Add(params IModule[] modules);

        /// <summary>
        /// Adds modules to the list.
        /// Any <c>null</c> items in the sequence of modules will be discarded.
        /// </summary>
        /// <param name="modules">The modules to add.</param>
        void Add(IEnumerable<IModule> modules);

        /// <summary>
        /// Inserts modules into the list.
        /// Any <c>null</c> items in the sequence of modules will be discarded.
        /// </summary>
        /// <param name="index">The index at which to insert the modules.</param>
        /// <param name="modules">The modules to insert.</param>
        void Insert(int index, params IModule[] modules);

        /// <summary>
        /// Inserts modules into the list.
        /// Any <c>null</c> items in the sequence of modules will be discarded.
        /// </summary>
        /// <param name="index">The index at which to insert the modules.</param>
        /// <param name="modules">The modules to insert.</param>
        void Insert(int index, IEnumerable<IModule> modules);
    }
}
