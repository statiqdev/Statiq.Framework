using System;
using System.Collections.Generic;
using System.Text;

namespace Wyam.Common.Meta
{
    /// <summary>
    /// Capable of converting metadata types.
    /// </summary>
    public interface IMetadataTypeConverter
    {
        /// <summary>
        /// Provides access to the same enhanced type conversion used to convert metadata types.
        /// </summary>
        /// <typeparam name="T">The destination type.</typeparam>
        /// <param name="value">The value to convert.</param>
        /// <param name="result">The result of the conversion.</param>
        /// <returns><c>true</c> if the conversion could be completed, <c>false</c> otherwise.</returns>
        bool TryConvert<T>(object value, out T result);
    }
}
