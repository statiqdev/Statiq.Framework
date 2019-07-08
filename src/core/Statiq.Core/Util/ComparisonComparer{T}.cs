using System;
using System.Collections.Generic;
using Statiq.Common.Meta;

namespace Statiq.Core.Util
{
    /// <summary>
    /// Adapts a <see cref="Comparison{T}"/> delegate to <see cref="IComparer{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type to compare.</typeparam>
    public class ComparisonComparer<T> : IComparer<T>
    {
        private readonly Comparison<T> _comparison;

        public ComparisonComparer(Comparison<T> comparison)
        {
            _comparison = comparison ?? throw new ArgumentNullException(nameof(comparison));
        }

        public int Compare(T x, T y) => _comparison(x, y);
    }
}