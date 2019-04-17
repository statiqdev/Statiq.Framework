using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Core.Meta;

namespace Wyam.Core.Util
{
    /// <summary>
    /// Adapts a typed equality comparer to untyped metadata by attempting to convert the
    /// metadata values to the comparer type before running the comparison. If neither type
    /// can be converted to <typeparamref name="TValue"/>, the comparison fails.
    /// </summary>
    /// <typeparam name="TValue">The value type to convert to for comparisons.</typeparam>
    internal class ConvertingEqualityComparer<TValue> : IEqualityComparer<object>
    {
        private readonly IEqualityComparer<TValue> _comparer;

        public ConvertingEqualityComparer(IEqualityComparer<TValue> comparer)
        {
            _comparer = comparer ?? throw new ArgumentNullException(nameof(comparer));
        }

        bool IEqualityComparer<object>.Equals(object x, object y) =>
            TypeHelper.Instance.TryConvert(x, out TValue xValue)
                && TypeHelper.Instance.TryConvert(y, out TValue yValue)
                && _comparer.Equals(xValue, yValue);

        public int GetHashCode(object obj) =>
            TypeHelper.Instance.TryConvert(obj, out TValue value)
                ? _comparer.GetHashCode(value)
                : 0;
    }
}
