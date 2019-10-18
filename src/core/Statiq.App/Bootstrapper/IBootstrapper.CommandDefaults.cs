using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Cli;
using Statiq.Common;

namespace Statiq.App
{
    public partial interface IBootstrapper
    {
        public IBootstrapper AddCommand<TCommand>()
            where TCommand : class, ICommand =>
            ConfigureCommands(x => x.AddCommand<TCommand>());

        public IBootstrapper AddCommand<TCommand>(string name)
            where TCommand : class, ICommand =>
            ConfigureCommands(x => x.AddCommand<TCommand>(name));

        public IBootstrapper AddCommand(Type commandType) =>
            ConfigureCommands(x => x.AddCommand(commandType));

        public IBootstrapper AddCommand(Type commandType, string name) =>
            ConfigureCommands(x => x.AddCommand(commandType, name));

        public IBootstrapper AddCommand(Type commandType, string name, string description) =>
            ConfigureCommands(x => x.AddCommand(commandType, name).WithDescription(description));

        public IBootstrapper AddBuildCommand(
            string name,
            params string[] pipelines) =>
            ConfigureCommands(x => x.AddBuild(name, pipelines));

        public IBootstrapper AddBuildCommand(
            string name,
            string description,
            params string[] pipelines) =>
            ConfigureCommands(x => x.AddBuild(name, pipelines).WithDescription(description));

        public IBootstrapper AddBuildCommand(
            string name,
            bool defaultPipelines,
            params string[] pipelines) =>
            ConfigureCommands(x => x.AddBuild(name, defaultPipelines, pipelines));

        public IBootstrapper AddBuildCommand(
            string name,
            string description,
            bool defaultPipelines,
            params string[] pipelines) =>
            ConfigureCommands(x => x.AddBuild(name, defaultPipelines, pipelines).WithDescription(description));

        public IBootstrapper AddBuildCommand(string name, EngineCommandSettings commandSettings) =>
            ConfigureCommands(x => x.AddBuild(name, commandSettings));

        public IBootstrapper AddBuildCommand(string name, string description, EngineCommandSettings commandSettings) =>
            ConfigureCommands(x => x.AddBuild(name, commandSettings).WithDescription(description));

        public IBootstrapper AddDelegateCommand(string name, Func<CommandContext, int> func) =>
            ConfigureCommands(x => x.AddDelegate(name, func));

        public IBootstrapper AddDelegateCommand(string name, string description, Func<CommandContext, int> func) =>
            ConfigureCommands(x => x.AddDelegate(name, func).WithDescription(description));

        public IBootstrapper AddDelegateCommand<TSettings>(string name, Func<CommandContext, TSettings, int> func)
            where TSettings : CommandSettings =>
            ConfigureCommands(x => x.AddDelegate(name, func));

        public IBootstrapper AddDelegateCommand<TSettings>(string name, string description, Func<CommandContext, TSettings, int> func)
            where TSettings : CommandSettings =>
            ConfigureCommands(x => x.AddDelegate(name, func).WithDescription(description));

        public IBootstrapper AddCommands(Assembly assembly)
        {
            _ = assembly ?? throw new ArgumentNullException(nameof(assembly));
            foreach (Type commandType in ClassCatalog.GetTypesAssignableTo<ICommand>().Where(x => x.Assembly.Equals(assembly)))
            {
                AddCommand(commandType);
            }
            return this;
        }

        public IBootstrapper AddCommands() => AddCommands(Assembly.GetEntryAssembly());
    }
}
