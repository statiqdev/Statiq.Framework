using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Statiq.Common;

namespace Statiq.Core
{
    /// <summary>
    /// Orders the input documents based on the specified key function.
    /// </summary>
    /// <remarks>
    /// The ordered documents are output as the result of this module.
    /// </remarks>
    /// <category>Control</category>
    public class OrderDocuments : Module
    {
        private readonly Stack<Order> _orders = new Stack<Order>();

        /// <summary>
        /// Orders the input documents using the specified delegate to get the ordering key.
        /// </summary>
        /// <param name="key">A delegate that should return the key to use for ordering.</param>
        public OrderDocuments(Config<object> key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            _orders.Push(new Order(key));
        }

        /// <summary>
        /// Orders the input documents using the value of the specified metadata key.
        /// </summary>
        /// <param name="key">A metadata key to get the objects to use for comparison.</param>
        public OrderDocuments(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            _orders.Push(new Order(Config.FromDocument(key)));
        }

        /// <summary>
        /// Orders the input documents using the specified delegate to get a secondary ordering key.
        /// You can chain as many <c>ThenBy</c> calls together as needed.
        /// </summary>
        /// <param name="key">A delegate that should return the key to use for ordering.</param>
        /// <returns>The current module instance.</returns>
        public OrderDocuments ThenBy(Config<object> key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            _orders.Push(new Order(key));
            return this;
        }

        /// <summary>
        /// Orders the input documents using the value of the specified metadata key.
        /// You can chain as many <c>ThenBy</c> calls together as needed.
        /// </summary>
        /// <param name="key">A metadata key to get the objects to use for comparison.</param>
        /// <returns>The current module instance.</returns>
        public OrderDocuments ThenBy(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            _orders.Push(new Order(Config.FromDocument(key)));
            return this;
        }

        /// <summary>
        /// Specifies whether the documents should be output in descending order (the default is ascending order).
        /// If you use this method after called ThenBy, the descending ordering will apply to the secondary sort.
        /// </summary>
        /// <param name="descending">If set to <c>true</c>, the documents are output in descending order.</param>
        /// <returns>The current module instance.</returns>
        public OrderDocuments Descending(bool descending = true)
        {
            _orders.Peek().Descending = descending;
            return this;
        }

        /// <summary>
        /// Specifies a comparer to use for the ordering.
        /// </summary>
        /// <param name="comparer">The comparer to use.</param>
        /// <returns>The current module instance.</returns>
        public OrderDocuments WithComparer(IComparer<object> comparer)
        {
            _orders.Peek().Comparer = comparer;
            return this;
        }

        /// <summary>
        /// Specifies a typed comparer to use for the ordering. A conversion to the
        /// comparer type will be attempted for the object being compared. If the conversion fails
        /// for either object, the default object comparer will be used. Note that this will also have the effect
        /// of treating different convertible types as being of the same type. For example,
        /// if you have two keys, 1 and "1", and use a string-based comparison, the
        /// documents will compare as equal.
        /// </summary>
        /// <param name="comparer">The typed comparer to use.</param>
        /// <returns>The current module instance.</returns>
        public OrderDocuments WithComparer<TValue>(IComparer<TValue> comparer)
        {
            _orders.Peek().Comparer = comparer == null ? null : new ConvertingComparer<TValue>(comparer);
            return this;
        }

        /// <summary>
        /// Specifies a comparison delegate to use for the ordering.
        /// </summary>
        /// <param name="comparison">The comparison delegate to use.</param>
        /// <returns>The current module instance.</returns>
        public OrderDocuments WithComparison(Comparison<object> comparison)
        {
            _orders.Peek().Comparer = comparison == null ? null : new ComparisonComparer<object>(comparison);
            return this;
        }

        /// <summary>
        /// Specifies a typed comparison delegate to use for the ordering. A conversion to the
        /// comparison type will be attempted for the object being compared. If the conversion fails
        /// for either object, the default object comparer will be used. Note that this will also have the effect
        /// of treating different convertible types as being of the same type. For example,
        /// if you have two keys, 1 and "1", and use a string-based comparison, the
        /// documents will compare as equal.
        /// </summary>
        /// <param name="comparison">The typed comparison delegate to use.</param>
        /// <returns>The current module instance.</returns>
        public OrderDocuments WithComparison<TValue>(Comparison<TValue> comparison)
        {
            _orders.Peek().Comparer = comparison == null
                ? null
                : new ConvertingComparer<TValue>(new ComparisonComparer<TValue>(comparison));
            return this;
        }

        /// <inheritdoc />
        public override Task<IEnumerable<IDocument>> ExecuteAsync(IExecutionContext context)
        {
            IOrderedEnumerable<IDocument> orderdList = null;
            foreach (Order order in _orders.Reverse())
            {
                if (orderdList == null)
                {
                    orderdList = order.Descending
                        ? context.Inputs.OrderByDescending(x => order.Key.GetValueAsync(x, context).Result, order.Comparer)
                        : context.Inputs.OrderBy(x => order.Key.GetValueAsync(x, context).Result, order.Comparer);
                }
                else
                {
                    orderdList = order.Descending
                        ? orderdList.ThenByDescending(x => order.Key.GetValueAsync(x, context).Result, order.Comparer)
                        : orderdList.ThenBy(x => order.Key.GetValueAsync(x, context).Result, order.Comparer);
                }
            }

            return Task.FromResult<IEnumerable<IDocument>>(orderdList);
        }

        private class Order
        {
            public Config<object> Key { get; }
            public bool Descending { get; set; }
            public IComparer<object> Comparer { get; set; }

            public Order(Config<object> key)
            {
                Key = key;
            }
        }
    }
}
