using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Shouldly;
using Wyam.Common.Configuration;
using Wyam.Common.Documents;
using Wyam.Common.Execution;
using Wyam.Testing;
using Wyam.Testing.Execution;

namespace Wyam.Common.Tests.Configuration
{
    [TestFixture]
    public class DocumentConfigExtensionsFixture : BaseFixture
    {
        public class GetValueAsyncTests : DocumentConfigExtensionsFixture
        {
            [Test]
            public async Task ReturnsDefaultTaskForNullIntConfig()
            {
                // Given, When
                Task<int> task = ((DocumentConfig<int>)null).GetValueAsync(null, null);

                // Then
                (await task).ShouldBe(default);
            }

            [Test]
            public async Task ReturnsDefaultTaskForNullObjectConfig()
            {
                // Given, When
                Task<object> task = ((DocumentConfig<object>)null).GetValueAsync(null, null);

                // Then
                (await task).ShouldBe(default);
            }

            [Test]
            public async Task ConvertsValue()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                context.AddTypeConversion<string, int>(x => int.Parse(x));
                DocumentConfig<object> config = "10";

                // When
                int result = await config.GetValueAsync<int>(null, context);

                // Then
                result.ShouldBe(10);
            }

            [Test]
            public async Task ThrowsIfNoConversion()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                DocumentConfig<object> config = "10";

                // When, Then
                await Should.ThrowAsync<InvalidOperationException>(async () => await config.GetValueAsync<int>(null, context));
            }
        }

        public class TryGetValueAsyncTests : DocumentConfigExtensionsFixture
        {
            [Test]
            public async Task ConvertsValue()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                context.AddTypeConversion<string, int>(x => int.Parse(x));
                DocumentConfig<object> config = "10";

                // When
                int result = await config.TryGetValueAsync<int>(null, context);

                // Then
                result.ShouldBe(10);
            }

            [Test]
            public async Task ReturnsDefaultValueIfNoConversion()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                DocumentConfig<object> config = "10";

                // When
                int result = await config.TryGetValueAsync<int>(null, context);

                // Then
                result.ShouldBe(default);
            }
        }
    }
}
