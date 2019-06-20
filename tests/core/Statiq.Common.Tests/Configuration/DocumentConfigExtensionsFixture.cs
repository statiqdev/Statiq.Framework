using System;
using System.Threading.Tasks;
using NUnit.Framework;
using Shouldly;
using Statiq.Common.Configuration;
using Statiq.Testing;
using Statiq.Testing.Execution;

namespace Statiq.Common.Tests.Configuration
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
                context.TypeConverter.AddTypeConversion<string, int>(x => int.Parse(x));
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
                context.TypeConverter.AddTypeConversion<string, int>(x => int.Parse(x));
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
