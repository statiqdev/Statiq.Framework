using System;
using System.Collections.Generic;
using System.Text;

namespace Statiq.Common.Execution
{
    /// <summary>
    /// Can perform enhanced type conversion.
    /// </summary>
    public interface ITypeConverter
    {
        /// <summary>
        /// Performs enhanced type conversion.
        /// </summary>
        /// <typeparam name="T">The destination type.</typeparam>
        /// <param name="value">The value to convert.</param>
        /// <param name="result">The result of the conversion.</param>
        /// <returns><c>true</c> if the conversion could be completed, <c>false</c> otherwise.</returns>
        bool TryConvert<T>(object value, out T result);
    }
}
