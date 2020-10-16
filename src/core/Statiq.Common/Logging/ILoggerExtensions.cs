using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Logging;

namespace Statiq.Common
{
    public static class ILoggerExtensions
    {
        /// <summary>
        /// Logs an appropriate error message for the exception, unwrapping <see cref="AggregateException"/>
        /// and <see cref="TargetInvocationException"/> exceptions and ignoring <see cref="LoggedException"/>.
        /// </summary>
        /// <remarks>
        /// When using in a catch block, you should throw the result of this method instead of the original
        /// exception to ensure the exception is wrapped in a <see cref="LoggedException"/> as it bubbles up
        /// and won't get logged again.
        /// </remarks>
        /// <param name="logger">The logger to log to.</param>
        /// <param name="exception">The exception to log.</param>
        /// <returns>The original exception or a <see cref="LoggedException"/> wrapping the original exception.</returns>
        public static Exception LogAndWrapException(this ILogger logger, Exception exception)
        {
            logger.ThrowIfNull(nameof(logger));

            if (exception is object && !(exception is OperationCanceledException) && !(exception is LoggedException))
            {
                // Unwrap aggregate and invocation exceptions
                switch (exception)
                {
                    case AggregateException aggregateException:
                        foreach (Exception innerException in aggregateException.InnerExceptions)
                        {
                            logger.LogAndWrapException(innerException);
                        }
                        break;
                    case TargetInvocationException invocationException:
                        if (invocationException.InnerException is object)
                        {
                            logger.LogAndWrapException(invocationException.InnerException);
                        }
                        break;
                    default:
                        logger.LogError(exception.Message);
                        break;
                }
                return new LoggedException(exception);
            }

            return exception;
        }
    }
}
