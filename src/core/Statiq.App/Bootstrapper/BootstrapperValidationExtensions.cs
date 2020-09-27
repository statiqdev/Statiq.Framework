using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Statiq.Common;

namespace Statiq.App
{
    public static class BootstrapperValidationExtensions
    {
        public static Bootstrapper AddValidator<TValidator>(this Bootstrapper bootstrapper)
            where TValidator : IValidator =>
            bootstrapper.ConfigureEngine(x => x.Validators.Add<TValidator>());

        public static TBootstrapper AddValidator<TBootstrapper>(this TBootstrapper bootstrapper, Type validatorType)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.ConfigureEngine(x => x.Validators.Add(validatorType));

        public static TBootstrapper AddValidator<TBootstrapper>(this TBootstrapper bootstrapper, IValidator validator)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.ConfigureEngine(x => x.Validators.Add(validator));

        public static void AddValidator<TBootstrapper>(
            this Bootstrapper bootstrapper,
            IEnumerable<string> pipelines,
            IEnumerable<Phase> phases,
            Func<IValidationContext, Task> validateFunc)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.ConfigureEngine(x => x.Validators.Add(pipelines, phases, validateFunc));

        public static void AddValidator<TBootstrapper>(
            this Bootstrapper bootstrapper,
            IEnumerable<string> pipelines,
            IEnumerable<Phase> phases,
            Action<IValidationContext> validateAction)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.ConfigureEngine(x => x.Validators.Add(pipelines, phases, validateAction));

        public static void AddValidator<TBootstrapper>(
            this Bootstrapper bootstrapper,
            IEnumerable<string> pipelines,
            IEnumerable<Phase> phases,
            Func<IDocument, IValidationContext, Task> validateFunc)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.ConfigureEngine(x => x.Validators.Add(pipelines, phases, validateFunc));

        public static void AddValidator<TBootstrapper>(
            this Bootstrapper bootstrapper,
            IEnumerable<string> pipelines,
            IEnumerable<Phase> phases,
            Action<IDocument, IValidationContext> validateAction)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.ConfigureEngine(x => x.Validators.Add(pipelines, phases, validateAction));

        public static void AddValidator<TBootstrapper>(
            this Bootstrapper bootstrapper,
            string pipeline,
            Phase phase,
            Func<IValidationContext, Task> validateFunc)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.ConfigureEngine(x => x.Validators.Add(pipeline, phase, validateFunc));

        public static void AddValidator<TBootstrapper>(
            this Bootstrapper bootstrapper,
            string pipeline,
            Phase phase,
            Action<IValidationContext> validateAction)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.ConfigureEngine(x => x.Validators.Add(pipeline, phase, validateAction));

        public static void AddValidator<TBootstrapper>(
            this Bootstrapper bootstrapper,
            string pipeline,
            Phase phase,
            Func<IDocument, IValidationContext, Task> validateFunc)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.ConfigureEngine(x => x.Validators.Add(pipeline, phase, validateFunc));

        public static void AddValidator<TBootstrapper>(
            this Bootstrapper bootstrapper,
            string pipeline,
            Phase phase,
            Action<IDocument, IValidationContext> validateAction)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.ConfigureEngine(x => x.Validators.Add(pipeline, phase, validateAction));

        /// <summary>
        /// Adds all validators that implement <see cref="IValidator"/> from the specified assembly.
        /// </summary>
        /// <param name="bootstrapper">The bootstrapper.</param>
        /// <param name="assembly">The assembly to add validators from.</param>
        /// <returns>The current bootstrapper.</returns>
        public static TBootstrapper AddValidators<TBootstrapper>(this TBootstrapper bootstrapper, Assembly assembly)
            where TBootstrapper : IBootstrapper
        {
            bootstrapper.ThrowIfNull(nameof(bootstrapper));
            assembly.ThrowIfNull(nameof(assembly));
            foreach (Type validatorType in bootstrapper.ClassCatalog.GetTypesAssignableTo<IValidator>().Where(x => x.Assembly.Equals(assembly)))
            {
                bootstrapper.AddValidator(validatorType);
            }
            return bootstrapper;
        }

        /// <summary>
        /// Adds all validators that implement <see cref="IValidator"/> from the entry assembly.
        /// </summary>
        /// <param name="bootstrapper">The bootstrapper.</param>
        /// <returns>The current bootstrapper.</returns>
        public static TBootstrapper AddValidators<TBootstrapper>(this TBootstrapper bootstrapper)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.AddValidators(Assembly.GetEntryAssembly());

        public static Bootstrapper AddValidators<TParent>(this Bootstrapper bootstrapper) => bootstrapper.AddValidators(typeof(TParent));

        public static TBootstrapper AddValidators<TBootstrapper>(this TBootstrapper bootstrapper, Type parentType)
            where TBootstrapper : IBootstrapper
        {
            bootstrapper.ThrowIfNull(nameof(bootstrapper));
            parentType.ThrowIfNull(nameof(parentType));
            foreach (Type validatorType in parentType.GetNestedTypes().Where(x => typeof(IValidator).IsAssignableFrom(x)))
            {
                bootstrapper.AddValidator(validatorType);
            }
            return bootstrapper;
        }
    }
}
