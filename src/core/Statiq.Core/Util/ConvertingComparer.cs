using System;
using System.Collections.Generic;
using Statiq.Common.Meta;

namespace Statiq.Core.Util
{
    /// <summary>
    /// Adapts a typed equality comparer to untyped metadata by attempting to convert the
    /// metadata values to the comparer type before running the comparison. If neither type
    /// can be converted to <typeparamref name="TValue"/>, the comparison returns 0 (equivalent).
    /// </summary>
    /// <typeparam name="TValue">The value type to convert to for comparisons.</typeparam>
    internal class ConvertingComparer<TValue> : IComparer<object>
    {
        private readonly IComparer<TValue> _comparer;

        public ConvertingComparer(IComparer<TValue> comparer)
        {
            _comparer = comparer ?? throw new ArgumentNullException(nameof(comparer));
        }

        public int Compare(object x, object y) =>
            TypeHelper.TryConvert(x, out TValue xValue) && TypeHelper.TryConvert(y, out TValue yValue)
                ? _comparer.Compare(xValue, yValue)
                : 0;
    }
}