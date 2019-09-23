using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using Shouldly;
using Statiq.Common;
using Statiq.Core;
using Statiq.Testing;

namespace Statiq.App.Tests.Bootstrapper
{
    [TestFixture]
    public class BootstrapperFixture : BaseFixture
    {
        public class RunTests : BootstrapperFixture
        {
            [Test]
            public async Task LogsVersion()
            {
                // Given
                string[] args = new[] { "build" };
                TestLoggerProvider provider = new TestLoggerProvider();
                IBootstrapper bootstrapper = App.Bootstrapper.Create(args);
                bootstrapper.AddCommand<BuildCommand>("build");
                bootstrapper.AddServices(services => services.AddSingleton<ILoggerProvider>(provider));
                bootstrapper.AddPipeline("Foo");

                // When
                int exitCode = await bootstrapper.RunAsync();

                // Then
                exitCode.ShouldBe((int)ExitCode.Normal);
                provider.Messages.ShouldContain(x => x.FormattedMessage.StartsWith("Statiq version"));
            }

            [Test]
            public async Task NoPipelinesWarning()
            {
                // Given
                string[] args = new[] { "build" };
                TestLoggerProvider provider = new TestLoggerProvider
                {
                    ThrowLogLevel = LogLevel.None
                };
                IBootstrapper bootstrapper = App.Bootstrapper.Create(args);
                bootstrapper.AddCommand<BuildCommand>("build");
                bootstrapper.AddServices(services => services.AddSingleton<ILoggerProvider>(provider));

                // When
                int exitCode = await bootstrapper.RunAsync();

                // Then
                exitCode.ShouldBe((int)ExitCode.Normal);
                provider.Messages.ShouldContain(x =>
                    x.LogLevel == LogLevel.Warning
                    && x.FormattedMessage == "No pipelines are configured or specified.");
            }

            [TestCase("Trace", 19)] // Includes module start/finish
            [TestCase("Debug", 18)] // Include modules start/finish
            [TestCase("Information", 5)] // Includes pipeline finish
            [TestCase("Warning", 3)]
            [TestCase("Error", 2)]
            [TestCase("Critical", 1)]
            public async Task SetsLogLevel(string logLevel, int expected)
            {
                // Given
                string[] args = new[] { "build", "-l", logLevel };
                TestLoggerProvider provider = new TestLoggerProvider
                {
                    ThrowLogLevel = LogLevel.None
                };
                IBootstrapper bootstrapper = App.Bootstrapper.Create(args);
                bootstrapper.AddCommand<BuildCommand>("build");
                bootstrapper.AddServices(services => services.AddSingleton<ILoggerProvider>(provider));
                bootstrapper.AddPipeline(
                    "Foo",
                    new LogMessage(LogLevel.Trace, "A"),
                    new LogMessage(LogLevel.Debug, "B"),
                    new LogMessage(LogLevel.Information, "C"),
                    new LogMessage(LogLevel.Warning, "D"),
                    new LogMessage(LogLevel.Error, "E"),
                    new LogMessage(LogLevel.Critical, "F"));

                // When
                int exitCode = await bootstrapper.RunAsync();

                // Then
                exitCode.ShouldBe((int)ExitCode.Normal);
                provider.Messages.Count(x => x.FormattedMessage.StartsWith("Foo/Process")).ShouldBe(expected);
            }

            [Test]
            public async Task CatalogsType()
            {
                // Given
                string[] args = new[] { "build", "-l", "Debug" };
                TestLoggerProvider provider = new TestLoggerProvider();
                IBootstrapper bootstrapper = App.Bootstrapper.Create(args);
                bootstrapper.AddCommand<BuildCommand>("build");
                bootstrapper.AddServices(services => services.AddSingleton<ILoggerProvider>(provider));
                bootstrapper.AddPipeline("Foo");

                // When
                int exitCode = await bootstrapper.RunAsync();

                // Then
                exitCode.ShouldBe((int)ExitCode.Normal);
                bootstrapper.ClassCatalog.GetAssignableFrom<BootstrapperFixture>().Count().ShouldBe(1);
                provider.Messages.ShouldContain(x => x.FormattedMessage.StartsWith("Cataloging types in assembly"));
            }
        }
    }
}
