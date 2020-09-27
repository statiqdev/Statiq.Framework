using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Statiq.Common
{
    public static class IValidatorCollectionExtensions
    {
        /// <summary>
        /// Adds a validator by type.
        /// </summary>
        /// <typeparam name="TValidator">The type of the validator to add.</typeparam>
        /// <param name="validators">The validators.</param>
        public static void Add<TValidator>(this IValidatorCollection validators)
            where TValidator : IValidator
        {
            validators.ThrowIfNull(nameof(validators));
            validators.Add(Activator.CreateInstance<TValidator>());
        }

        /// <summary>
        /// Adds a validator by type.
        /// </summary>
        /// <param name="validators">The validators.</param>
        /// <param name="validatorType">The type of the validator to add (must implement <see cref="IValidator"/>).</param>
        public static void Add(this IValidatorCollection validators, Type validatorType)
        {
            validators.ThrowIfNull(nameof(validators));
            validatorType.ThrowIfNull(nameof(validatorType));
            if (!typeof(IValidator).IsAssignableFrom(validatorType))
            {
                throw new ArgumentException("The type must implement " + nameof(IValidator), nameof(validatorType));
            }
            validators.Add((IValidator)Activator.CreateInstance(validatorType));
        }

        public static void Add(
            this IValidatorCollection validators,
            IEnumerable<string> pipelines,
            IEnumerable<Phase> phases,
            Func<IValidationContext, Task> validateFunc) =>
            validators
                .ThrowIfNull(nameof(validators))
                .Add(new DelegateValidator(pipelines, phases, validateFunc));

        public static void Add(
            this IValidatorCollection validators,
            IEnumerable<string> pipelines,
            IEnumerable<Phase> phases,
            Action<IValidationContext> validateAction)
        {
            validateAction.ThrowIfNull(nameof(validateAction));
            validators.Add(
                pipelines,
                phases,
                context =>
                {
                    validateAction(context);
                    return Task.CompletedTask;
                });
        }

        public static void Add(
            this IValidatorCollection validators,
            IEnumerable<string> pipelines,
            IEnumerable<Phase> phases,
            Func<IDocument, IValidationContext, Task> validateFunc)
        {
            validateFunc.ThrowIfNull(nameof(validateFunc));
            validators.Add(
                pipelines,
                phases,
                async context => await context.Documents.ParallelForEachAsync(async doc => await validateFunc(doc, context), context.CancellationToken));
        }

        public static void Add(
            this IValidatorCollection validators,
            IEnumerable<string> pipelines,
            IEnumerable<Phase> phases,
            Action<IDocument, IValidationContext> validateAction)
        {
            validateAction.ThrowIfNull(nameof(validateAction));
            validators.Add(
                pipelines,
                phases,
                (document, context) =>
                {
                    validateAction(document, context);
                    return Task.CompletedTask;
                });
        }

        public static void Add(
            this IValidatorCollection validators,
            string pipeline,
            Phase phase,
            Func<IValidationContext, Task> validateFunc) =>
            validators.Add(new[] { pipeline }, new[] { phase }, validateFunc);

        public static void Add(
            this IValidatorCollection validators,
            string pipeline,
            Phase phase,
            Action<IValidationContext> validateAction)
        {
            validateAction.ThrowIfNull(nameof(validateAction));
            validators.Add(new[] { pipeline }, new[] { phase }, validateAction);
        }

        public static void Add(
            this IValidatorCollection validators,
            string pipeline,
            Phase phase,
            Func<IDocument, IValidationContext, Task> validateFunc) =>
            validators.Add(new[] { pipeline }, new[] { phase }, validateFunc);

        public static void Add(
            this IValidatorCollection validators,
            string pipeline,
            Phase phase,
            Action<IDocument, IValidationContext> validateAction)
        {
            validateAction.ThrowIfNull(nameof(validateAction));
            validators.Add(new[] { pipeline }, new[] { phase }, validateAction);
        }
    }
}
