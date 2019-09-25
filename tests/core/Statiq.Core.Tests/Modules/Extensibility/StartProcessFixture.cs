using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Shouldly;
using Statiq.Common;
using Statiq.Testing;

namespace Statiq.Core.Tests.Modules.Extensibility
{
    [TestFixture]
    public class StartProcessFixture : BaseFixture
    {
        public class ExecuteTests : StartProcessFixture
        {
            [Test]
            public async Task LogsOutput()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                ((TestLogger)context.Logger).ThrowLogLevel = Microsoft.Extensions.Logging.LogLevel.None;
                StartProcess startProcess = new StartProcess("dotnet", "--info").LogOutput();

                // When
                await ExecuteAsync(context, startProcess);

                // Then
                context.LogMessages.ShouldContain(x => x.FormattedMessage.Contains("Started process"));
                context.LogMessages.ShouldContain(x => x.FormattedMessage.Contains("exited with code 0"));
                context.LogMessages.ShouldContain(x => x.FormattedMessage.Contains(".NET Core runtimes installed"));
            }

            [Test]
            public async Task OutputsDocumentWithProcessOutput()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                ((TestLogger)context.Logger).ThrowLogLevel = Microsoft.Extensions.Logging.LogLevel.None;
                StartProcess startProcess = new StartProcess("dotnet", "--info");

                // When
                ImmutableArray<TestDocument> results = await ExecuteAsync(context, startProcess);

                // Then
                results.Single().Content.ShouldContain(".NET Core runtimes installed");
            }
        }
    }
}
