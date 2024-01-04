using System;
using System.Collections.Generic;
using Statiq.Common;

namespace Statiq.Core
{
    /// <summary>
    /// Throws an exception.
    /// </summary>
    /// <category name="Control" />
#pragma warning disable CA1711 // Identifiers should not have incorrect suffix
    public class ThrowException : SyncConfigModule<Exception>
#pragma warning restore CA1711 // Identifiers should not have incorrect suffix
    {
        /// <summary>
        /// Throws the specified exception.
        /// </summary>
        /// <param name="exception">The exception to throw or <c>null</c> not to throw an exception.</param>
        public ThrowException(Config<Exception> exception)
            : base(exception, false)
        {
        }

        /// <summary>
        /// Throws a <see cref="ExecutionException"/> with the specified message.
        /// </summary>
        /// <param name="message">The message of the exception to throw or <c>null</c> not to throw an exception.</param>
        public ThrowException(Config<string> message)
            : base(message.Transform(msg => msg is null ? null : (Exception)new ExecutionException(msg)), false)
        {
        }

        protected override IEnumerable<IDocument> ExecuteConfig(IDocument input, IExecutionContext context, Exception value)
        {
            if (value is object)
            {
                throw value;
            }

            ArgumentNullException.ThrowIfNull(context);

            return input is null ? context.Inputs : input.Yield();
        }
    }
}