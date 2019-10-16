using System;
using System.Threading.Tasks;
using NUnit.Framework;
using Shouldly;
using Statiq.Testing;

namespace Statiq.Common.Tests.Config
{
    [TestFixture]
    public class ConfigExtensionsFixture : BaseFixture
    {
        public class GetValueAsyncTests : ConfigExtensionsFixture
        {
            [Test]
            public async Task ReturnsDefaultTaskForNullIntConfig()
            {
                // Given, When
                Task<int> task = ((Config<int>)null).GetValueAsync(null, null);

                // Then
                (await task).ShouldBe(default);
            }

            [Test]
            public async Task ReturnsDefaultTaskForNullObjectConfig()
            {
                // Given, When
                Task<object> task = ((Config<object>)null).GetValueAsync(null, null);

                // Then
                (await task).ShouldBe(default);
            }

            [Test]
            public async Task ConvertsValue()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                Config<object> config = "10";

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
                Config<object> config = "abc";

                // When, Then
                await Should.ThrowAsync<InvalidOperationException>(async () => await config.GetValueAsync<int>(null, context));
            }
        }

        public class TryGetValueAsyncTests : ConfigExtensionsFixture
        {
            [Test]
            public async Task ConvertsValue()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                Config<object> config = "10";

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
                Config<object> config = "abc";

                // When
                int result = await config.TryGetValueAsync<int>(null, context);

                // Then
                result.ShouldBe(default);
            }
        }
    }
}
