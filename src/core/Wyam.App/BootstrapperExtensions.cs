using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Cli;
using Wyam.App.Commands;
using Wyam.App.Configuration;
using Wyam.App.Tracing;
using Wyam.Common.Configuration;

namespace Wyam.App
{
    public static class BootstrapperExtensions
    {
        public static IBootstrapper AddCommand<TCommand>(this IBootstrapper bootstrapper, string name)
            where TCommand : class, ICommand
        {
            bootstrapper.Configurators.Add(new AddCommandConfigurator<TCommand>(name));
            return bootstrapper;
        }

        public static IBootstrapper AddDefaultConfigurators(this IBootstrapper bootstrapper)
        {
            // Scan and add boostrapper configurators using a temporary service collection
            ServiceCollection serviceCollection = new ServiceCollection();
            serviceCollection.Scan(x => x
                .FromApplicationDependencies(a =>
                {
                    // Exclude assemblies that throw type load exceptions
                    try
                    {
                        IEnumerable<TypeInfo> definedType = a.DefinedTypes;
                    }
                    catch (ReflectionTypeLoadException)
                    {
                        return false;
                    }
                    return true;
                })
                .AddClasses(c => c.AssignableTo<Common.Configuration.IConfigurator<IConfigurableBootstrapper>>())
                .As<Common.Configuration.IConfigurator<IConfigurableBootstrapper>>()
                .WithSingletonLifetime());
            IServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();
            foreach (Common.Configuration.IConfigurator<IConfigurableBootstrapper> bootstraperConfigurator in
                serviceProvider.GetServices<Common.Configuration.IConfigurator<IConfigurableBootstrapper>>())
            {
                bootstrapper.Configurators.Add(bootstraperConfigurator);
            }
            return bootstrapper;
        }

        public static IBootstrapper AddDefaultCommands(this IBootstrapper bootstrapper)
        {
            bootstrapper.SetDefaultCommand<BuildCommand>();
            bootstrapper.AddCommand<BuildCommand>("build");
            return bootstrapper;
        }

        public static IBootstrapper AddDefaultTracing(this IBootstrapper bootstrapper)
        {
            Common.Tracing.Trace.AddListener(new SimpleColorConsoleTraceListener
            {
                TraceOutputOptions = System.Diagnostics.TraceOptions.None
            });

            AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
            {
                if (e.ExceptionObject is Exception exception)
                {
                    Common.Tracing.Trace.Critical(exception.ToString());
                }
                Environment.Exit((int)ExitCode.UnhandledError);
            };

            return bootstrapper;
        }

        public static IBootstrapper AddConfigurator<T, TConfigurator>(this IBootstrapper bootstrapper)
            where T : class
            where TConfigurator : class, Common.Configuration.IConfigurator<T>
        {
            bootstrapper.Configurators.Add<T, TConfigurator>();
            return bootstrapper;
        }

        public static IBootstrapper AddConfigurator<T>(this IBootstrapper bootstrapper, Action<T> action)
            where T : class
        {
            bootstrapper.Configurators.Add(action);
            return bootstrapper;
        }

        public static IBootstrapper AddConfigurator<T>(
            this IBootstrapper bootstrapper,
            Common.Configuration.IConfigurator<T> configurator)
            where T : class
        {
            bootstrapper.Configurators.Add(configurator);
            return bootstrapper;
        }
    }
}
