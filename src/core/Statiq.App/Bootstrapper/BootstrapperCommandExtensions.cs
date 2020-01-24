using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Cli;
using Statiq.Common;

namespace Statiq.App
{
    public static class BootstrapperCommandExtensions
    {
        public static Bootstrapper AddCommand<TCommand>(this Bootstrapper bootstrapper)
            where TCommand : class, ICommand =>
            bootstrapper.ConfigureCommands(x => x.AddCommand<TCommand>());

        public static Bootstrapper AddCommand<TCommand>(this Bootstrapper bootstrapper, string name)
            where TCommand : class, ICommand =>
            bootstrapper.ConfigureCommands(x => x.AddCommand<TCommand>(name));

        public static Bootstrapper AddCommand(this Bootstrapper bootstrapper, Type commandType) =>
            bootstrapper.ConfigureCommands(x => x.AddCommand(commandType));

        public static Bootstrapper AddCommand(this Bootstrapper bootstrapper, Type commandType, string name) =>
            bootstrapper.ConfigureCommands(x => x.AddCommand(commandType, name));

        public static Bootstrapper AddCommand(this Bootstrapper bootstrapper, Type commandType, string name, string description) =>
            bootstrapper.ConfigureCommands(x => x.AddCommand(commandType, name).WithDescription(description));

        public static Bootstrapper AddPipelineCommand(
            this Bootstrapper bootstrapper,
            string name,
            params string[] pipelines) =>
            bootstrapper.ConfigureCommands(x => x.AddPipelineCommand(name, pipelines));

        public static Bootstrapper AddPipelineCommand(
            this Bootstrapper bootstrapper,
            string name,
            string description,
            params string[] pipelines) =>
            bootstrapper.ConfigureCommands(x => x.AddPipelineCommand(name, pipelines).WithDescription(description));

        public static Bootstrapper AddPipelineCommand(
            this Bootstrapper bootstrapper,
            string name,
            bool defaultPipelines,
            params string[] pipelines) =>
            bootstrapper.ConfigureCommands(x => x.AddPipelineCommand(name, defaultPipelines, pipelines));

        public static Bootstrapper AddPipelineCommand(
            this Bootstrapper bootstrapper,
            string name,
            string description,
            bool defaultPipelines,
            params string[] pipelines) =>
            bootstrapper.ConfigureCommands(x => x.AddPipelineCommand(name, defaultPipelines, pipelines).WithDescription(description));

        public static Bootstrapper AddPipelineCommand(
            this Bootstrapper bootstrapper,
            string name,
            EngineCommandSettings commandSettings) =>
            bootstrapper.ConfigureCommands(x => x.AddPipelineCommand(name, commandSettings));

        public static Bootstrapper AddPipelineCommand(
            this Bootstrapper bootstrapper,
            string name,
            string description,
            EngineCommandSettings commandSettings) =>
            bootstrapper.ConfigureCommands(x => x.AddPipelineCommand(name, commandSettings).WithDescription(description));

        public static Bootstrapper AddDelegateCommand(
            this Bootstrapper bootstrapper,
            string name,
            Func<CommandContext, int> func) =>
            bootstrapper.ConfigureCommands(x => x.AddDelegateCommand(name, func));

        public static Bootstrapper AddDelegateCommand(
            this Bootstrapper bootstrapper,
            string name,
            string description,
            Func<CommandContext, int> func) =>
            bootstrapper.ConfigureCommands(x => x.AddDelegateCommand(name, func).WithDescription(description));

        public static Bootstrapper AddDelegateCommand<TSettings>(
            this Bootstrapper bootstrapper,
            string name,
            Func<CommandContext, TSettings, int> func)
            where TSettings : CommandSettings =>
            bootstrapper.ConfigureCommands(x => x.AddDelegate(name, func));

        public static Bootstrapper AddDelegateCommand<TSettings>(
            this Bootstrapper bootstrapper,
            string name,
            string description,
            Func<CommandContext, TSettings, int> func)
            where TSettings : CommandSettings =>
            bootstrapper.ConfigureCommands(x => x.AddDelegate(name, func).WithDescription(description));

        /// <summary>
        /// Adds all commands that implement <see cref="ICommand"/> from the specified assembly.
        /// </summary>
        /// <param name="bootstrapper">The bootstrapper.</param>
        /// <param name="assembly">The assembly to add commands from.</param>
        /// <returns>The current bootstrapper.</returns>
        public static Bootstrapper AddCommands(this Bootstrapper bootstrapper, Assembly assembly)
        {
            _ = bootstrapper ?? throw new ArgumentNullException(nameof(bootstrapper));
            _ = assembly ?? throw new ArgumentNullException(nameof(assembly));
            foreach (Type commandType in bootstrapper.ClassCatalog.GetTypesAssignableTo<ICommand>().Where(x => x.Assembly.Equals(assembly)))
            {
                bootstrapper.AddCommand(commandType);
            }
            return bootstrapper;
        }

        /// <summary>
        /// Adds all commands that implement <see cref="ICommand"/> from the entry assembly.
        /// </summary>
        /// <param name="bootstrapper">The bootstrapper.</param>
        /// <returns>The current bootstrapper.</returns>
        public static Bootstrapper AddCommands(this Bootstrapper bootstrapper) => bootstrapper.AddCommands(Assembly.GetEntryAssembly());

        public static Bootstrapper AddCommands<TParent>(this Bootstrapper bootstrapper)
        {
            _ = bootstrapper ?? throw new ArgumentNullException(nameof(bootstrapper));
            foreach (Type commandType in typeof(TParent).GetNestedTypes().Where(x => typeof(ICommand).IsAssignableFrom(x)))
            {
                bootstrapper.AddCommand(commandType);
            }
            return bootstrapper;
        }
    }
}
