using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using Shouldly;
using Statiq.Common;
using Statiq.Core;
using Statiq.Testing;

namespace Statiq.App.Tests
{
    [TestFixture]
    public class BootstrapperFixture : BaseFixture
    {
        public class FooTests : BootstrapperFixture
        {
            [Test]
            public async Task LogLevel()
            {
                // Given
                string[] args = new[] { "build", "-l", "Debug" };
                TestLoggerProvider provider = new TestLoggerProvider();
                Bootstrapper bootstrapper = new Bootstrapper(args);
                bootstrapper.AddCommand<BuildCommand>("build");
                bootstrapper.AddServices(services => services.AddSingleton<ILoggerProvider>(provider));
                bootstrapper.AddPipeline("Pipeline", new LogMessage("A"));

                int exitCode = await bootstrapper.RunAsync();

                exitCode.ShouldBe((int)ExitCode.Normal);
            }
        }
    }
}
