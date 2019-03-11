using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Wyam.Common.Configuration;

namespace Wyam.App.Configuration
{
    internal class ConfiguratorList<T> : IList<IConfigurator<T>>
        where T : class
    {
        private readonly List<object> _list;

        public ConfiguratorList(List<object> list)
        {
            _list = list ?? throw new ArgumentNullException(nameof(list));
        }

        public IConfigurator<T> this[int index]
        {
            get => (IConfigurator<T>)_list[index];
            set
            {
                if (value != null)
                {
                    _list[index] = value;
                }
            }
        }

        public int Count => _list.Count;

        public bool IsReadOnly => false;

        public void Add(IConfigurator<T> item)
        {
            if (item != null)
            {
                _list.Add(item);
            }
        }

        public void Clear() => _list.Clear();

        public bool Contains(IConfigurator<T> item) => _list.Contains(item);

        public void CopyTo(IConfigurator<T>[] array, int arrayIndex) =>
            _list.Cast<IConfigurator<T>>().ToList().CopyTo(array, arrayIndex);

        public IEnumerator<IConfigurator<T>> GetEnumerator() =>
            _list.Cast<IConfigurator<T>>().GetEnumerator();

        public int IndexOf(IConfigurator<T> item) => _list.IndexOf(item);

        public void Insert(int index, IConfigurator<T> item)
        {
            if (item != null)
            {
                _list.Insert(index, item);
            }
        }

        public bool Remove(IConfigurator<T> item) => _list.Remove(item);

        public void RemoveAt(int index) => _list.RemoveAt(index);

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
