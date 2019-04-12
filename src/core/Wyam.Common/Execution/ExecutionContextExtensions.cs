using System;
using System.Collections.Generic;
using System.Text;

namespace Wyam.Common.Execution
{
    public static class ExecutionContextExtensions
    {
        /// <summary>
        /// Provides access to the same enhanced type conversion used to convert metadata types.
        /// This method never throws an exception. It will return default(T) if the value cannot be converted to T.
        /// </summary>
        /// <typeparam name="T">The desired return type.</typeparam>
        /// <param name="context">The execution context.</param>
        /// <param name="value">The value to convert.</param>
        /// <returns>The value converted to type T or default(T) if the value cannot be converted to type T.</returns>
        public static T Convert<T>(this IExecutionContext context, object value) => Convert<T>(context, value, null);

        /// <summary>
        /// Provides access to the same enhanced type conversion used to convert metadata types.
        /// This method never throws an exception. It will return the specified default value if the value cannot be converted to T.
        /// </summary>
        /// <typeparam name="T">The desired return type.</typeparam>
        /// <param name="context">The execution context.</param>
        /// <param name="value">The value to convert.</param>
        /// <param name="defaultValueFactory">A factory to get a default value if the value cannot be converted to type T.</param>
        /// <returns>The value converted to type T or the specified default value if the value cannot be converted to type T.</returns>
        public static T Convert<T>(this IExecutionContext context, object value, Func<T> defaultValueFactory) =>
            context.TryConvert<T>(value, out T result) ? result : (defaultValueFactory == null ? default : defaultValueFactory());
    }
}
