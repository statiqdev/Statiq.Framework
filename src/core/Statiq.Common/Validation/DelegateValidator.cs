using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Statiq.Common
{
    public class DelegateValidator : IValidator
    {
        private readonly Func<IValidationContext, Task> _validateFunc;

        public DelegateValidator(IEnumerable<string> pipelines, IEnumerable<Phase> phases, Func<IValidationContext, Task> validateFunc)
        {
            Pipelines = pipelines?.ToArray();
            Phases = phases?.ToArray();
            _validateFunc = validateFunc.ThrowIfNull(nameof(validateFunc));
        }

        /// <inheritdoc/>
        public string[] Pipelines { get; }

        /// <inheritdoc/>
        public Phase[] Phases { get; }

        public Task ValidateAsync(IValidationContext context) => _validateFunc(context);
    }
}
