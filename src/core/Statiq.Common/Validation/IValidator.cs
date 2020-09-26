using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Statiq.Common
{
    public interface IValidator
    {
        /// <summary>
        /// Performs validation.
        /// </summary>
        /// <param name="context">A validation context that contains the documents to validate as well as other state information.</param>
        Task ValidateAsync(IValidationContext context);

        /// <summary>
        /// The pipelines this validation applies to, or null to apply to all pipelines.
        /// </summary>
        string[] Pipelines { get; }

        /// <summary>
        /// The phases this validation applies to, or null to apply to all phases.
        /// </summary>
        Phase[] Phases { get; }
    }
}
