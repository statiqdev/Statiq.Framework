using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Wyam.Common.Documents;
using Wyam.Common.Execution;

namespace Wyam.Common.Modules
{
    /// <summary>
    /// A base class for modules that contain a collection of child modules.
    /// </summary>
    public abstract class ContainerModule : IModule, IModuleList
    {
        private readonly IModuleList _modules;

        /// <summary>
        /// Creates a new container module with the specified child modules.
        /// Any <c>null</c> items in the sequence of modules will be discarded.
        /// </summary>
        /// <param name="modules">The child modules.</param>
        protected ContainerModule(IEnumerable<IModule> modules)
        {
            _modules = (modules as IModuleList) ?? new ModuleList(modules);
        }

        /// <inheritdoc />
        public abstract Task<IEnumerable<IDocument>> ExecuteAsync(IReadOnlyList<IDocument> inputs, IExecutionContext context);

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <inheritdoc />
        public IEnumerator<IModule> GetEnumerator() => _modules.GetEnumerator();

        /// <inheritdoc />
        public void Add(IModule item) => _modules.Add(item);

        /// <inheritdoc />
        public void Clear() => _modules.Clear();

        /// <inheritdoc />
        public bool Contains(IModule item) => _modules.Contains(item);

        /// <inheritdoc />
        public void CopyTo(IModule[] array, int arrayIndex) => _modules.CopyTo(array, arrayIndex);

        /// <inheritdoc />
        public bool Remove(IModule item) => _modules.Remove(item);

        /// <inheritdoc />
        public int Count => _modules.Count;

        /// <inheritdoc />
        public void Add(params IModule[] modules) => _modules.Add(modules);

        /// <inheritdoc />
        public void Add(IEnumerable<IModule> modules) => _modules.Add(modules);

        /// <inheritdoc />
        public void Insert(int index, params IModule[] modules) => _modules.Insert(index, modules);

        /// <inheritdoc />
        public void Insert(int index, IEnumerable<IModule> modules) => _modules.Insert(index, modules);

        /// <inheritdoc />
        public bool IsReadOnly => _modules.IsReadOnly;

        /// <inheritdoc />
        public int IndexOf(IModule item) => _modules.IndexOf(item);

        /// <inheritdoc />
        public void Insert(int index, IModule item) => _modules.Insert(index, item);

        /// <inheritdoc />
        public void RemoveAt(int index) => _modules.RemoveAt(index);

        /// <inheritdoc />
        public IModule this[int index]
        {
            get { return _modules[index]; }
            set { _modules[index] = value; }
        }
    }
}
