using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Wyam.Common.Modules
{
    /// <summary>
    /// A collection of optionally named modules.
    /// </summary>
    public class ModuleList : IModuleList
    {
        private readonly List<IModule> _modules = new List<IModule>();

        /// <summary>
        /// Creates a new empty module list.
        /// </summary>
        public ModuleList()
        {
        }

        /// <summary>
        /// Creates a new module list with an initial set of modules.
        /// Any <c>null</c> items in the sequence of modules will be discarded.
        /// </summary>
        /// <param name="modules">The initial modules in the list.</param>
        public ModuleList(params IModule[] modules)
            : this((IEnumerable<IModule>)modules)
        {
        }

        /// <summary>
        /// Creates a new module list with an initial set of modules.
        /// Any <c>null</c> items in the sequence of modules will be discarded.
        /// </summary>
        /// <param name="modules">The initial modules in the list.</param>
        public ModuleList(IEnumerable<IModule> modules)
        {
            if (modules != null)
            {
                foreach (IModule module in modules.Where(x => x != null))
                {
                    Add(module);
                }
            }
        }

        /// <inheritdoc />
        public void Add(params IModule[] modules)
        {
            foreach (IModule module in modules.Where(x => x != null))
            {
                Add(module);
            }
        }

        /// <inheritdoc />
        public void Add(IModule item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }
            _modules.Add(item);
        }

        /// <inheritdoc />
        public void Insert(int index, params IModule[] modules)
        {
            modules = modules.Where(x => x != null).ToArray();
            for (int i = index; i < index + modules.Length; i++)
            {
                Insert(i, modules[i - index]);
            }
        }

        /// <inheritdoc />
        public void Insert(int index, IModule item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }
            _modules.Insert(index, item);
        }

        /// <inheritdoc />
        public bool Remove(IModule item)
        {
            int index = IndexOf(item);
            if (index >= 0)
            {
                _modules.RemoveAt(index);
                return true;
            }
            return false;
        }

        /// <inheritdoc />
        public void RemoveAt(int index) => _modules.RemoveAt(index);

        /// <inheritdoc />
        public IModule this[int index]
        {
            get => _modules[index];
            set => _modules[index] = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <inheritdoc />
        public int Count => _modules.Count;

        /// <inheritdoc />
        public void Clear() => _modules.Clear();

        /// <inheritdoc />
        public bool Contains(IModule item) => _modules.Any(x => x.Equals(item));

        /// <inheritdoc />
        public void CopyTo(IModule[] array, int arrayIndex) => _modules.CopyTo(array);

        /// <inheritdoc />
        public bool IsReadOnly => false;

        /// <inheritdoc />
        public int IndexOf(IModule item) => _modules.FindIndex(x => x.Equals(item));

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <inheritdoc />
        public IEnumerator<IModule> GetEnumerator() => _modules.GetEnumerator();
    }
}
