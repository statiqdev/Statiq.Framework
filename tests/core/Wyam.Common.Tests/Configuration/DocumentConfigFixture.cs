using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Shouldly;
using Wyam.Common.Configuration;
using Wyam.Common.Documents;
using Wyam.Testing;
using Wyam.Testing.Documents;
using Wyam.Testing.Execution;

namespace Wyam.Common.Tests.Configuration
{
    [TestFixture]
    public class DocumentConfigFixture : BaseFixture
    {
        public class CastOperatorTests : DocumentConfigFixture
        {
            [Test]
            public async Task CastsToMatchingType()
            {
                // Given, When
                DocumentConfig<int> config = 10;

                // Then
                (await config.GetAndCacheValueAsync(null, null)).ShouldBe(10);
            }

            [Test]
            public async Task CastsToObject()
            {
                // Given, When
                DocumentConfig<object> config = 10;

                // Then
                (await config.GetAndCacheValueAsync(null, null)).ShouldBe(10);
            }

            [Test]
            public async Task CastsFromDocumentConfigToObject()
            {
                // Given, When
                DocumentConfig<object> config = new DocumentConfig<int>((_, __) => Task.FromResult(10));

                // Then
                (await config.GetAndCacheValueAsync(null, null)).ShouldBe(10);
            }

            [Test]
            public async Task CastsFromDocumentConfigToObjectEnumerable()
            {
                // Given, When
                DocumentConfig<IEnumerable<object>> config = new DocumentConfig<int>((_, __) => Task.FromResult(10));

                // Then
                (await config.GetAndCacheValueAsync(null, null)).ShouldBe(new object[] { 10 });
            }

            [Test]
            public async Task CastsFromDocumentConfigOfEnumerableToObjectEnumerable()
            {
                // Given, When
                DocumentConfig<IEnumerable<object>> config =
                    new DocumentConfig<IEnumerable<int>>((_, __) => Task.FromResult((IEnumerable<int>)new[] { 8, 9, 10 }));

                // Then
                (await config.GetAndCacheValueAsync(null, null)).ShouldBe(new object[] { 8, 9, 10 });
            }

            [Test]
            public async Task CastingFromArgToArgAndObjectDocumentConfig()
            {
                // Given, When
                DocumentConfig<int, object> config = Config.FromDocument<int, int>((doc, ctx, arg) => 10);

                // Then
                (await config.GetAndCacheValueAsync(null, null, 100)).ShouldBe(10);
            }

            [Test]
            public async Task CastingFromArgToArgAndObjectContextConfig()
            {
                // Given, When
                ContextConfig<int, object> config = Config.FromDocument<int, int>((doc, ctx, arg) => 10);

                // Then
                (await config.GetAndCacheValueAsync(null, null, 100)).ShouldBe(10);
            }

            [Test]
            public void CastingFromArgToObjectDocumentConfig()
            {
                // Given, When
                DocumentConfig<object> config;

                // Then
                Should.Throw<InvalidCastException>(() => config = Config.FromDocument<int, int>((doc, ctx, arg) => 10));
            }

            [Test]
            public void CastingFromArgToObjectContextConfig()
            {
                // Given, When
                ContextConfig<object> config;

                // Then
                Should.Throw<InvalidCastException>(() => config = Config.FromDocument<int, int>((doc, ctx, arg) => 10));
            }
        }

        public class GetCacheAndValueAsyncTests : DocumentConfigFixture
        {
            [Test]
            public async Task CachesForSameContext()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                int count = 1;
                DocumentConfig<int> config = new DocumentConfig<int>(async (_, __) =>
                {
                    await Task.CompletedTask;
                    return count++;
                });

                // When
                int result1 = await config.GetAndCacheValueAsync(null, context);
                int result2 = await config.GetAndCacheValueAsync(null, context);

                // Then
                result1.ShouldBe(1);
                result2.ShouldBe(1);
            }

            [Test]
            public async Task DoesNotCacheForDifferentContexts()
            {
                // Given
                TestExecutionContext context1 = new TestExecutionContext();
                TestExecutionContext context2 = new TestExecutionContext();
                int count = 1;
                DocumentConfig<int> config = new DocumentConfig<int>(async (_, __) =>
                {
                    await Task.CompletedTask;
                    return count++;
                });

                // When
                int result1 = await config.GetAndCacheValueAsync(null, context1);
                int result2 = await config.GetAndCacheValueAsync(null, context2);

                // Then
                result1.ShouldBe(1);
                result2.ShouldBe(2);
            }

            [Test]
            public async Task DoesNotCacheIfDocument()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                TestDocument document = new TestDocument();
                int count = 1;
                DocumentConfig<int> config = new DocumentConfig<int>(async (_, __) =>
                {
                    await Task.CompletedTask;
                    return count++;
                });

                // When
                int result1 = await config.GetAndCacheValueAsync(document, context);
                int result2 = await config.GetAndCacheValueAsync(document, context);

                // Then
                result1.ShouldBe(1);
                result2.ShouldBe(2);
            }

            [Test]
            public async Task CachesForContextConfigIfDocument()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                TestDocument document = new TestDocument();
                int count = 1;
                DocumentConfig<int> config = new ContextConfig<int>(async _ =>
                {
                    await Task.CompletedTask;
                    return count++;
                });

                // When
                int result1 = await config.GetAndCacheValueAsync(document, context);
                int result2 = await config.GetAndCacheValueAsync(document, context);

                // Then
                result1.ShouldBe(1);
                result2.ShouldBe(1);
            }

            [Test]
            public async Task DoesNotCacheForSameContextWithArgument()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                int count = 1;
                DocumentConfig<int, int> config = new DocumentConfig<int, int>(async (_, __, arg) =>
                {
                    await Task.CompletedTask;
                    return count++;
                });

                // When
                int result1 = await config.GetAndCacheValueAsync(null, context, 100);
                int result2 = await config.GetAndCacheValueAsync(null, context, 100);

                // Then
                result1.ShouldBe(1);
                result2.ShouldBe(2);
            }

            [Test]
            public async Task DoesNotCacheForContextConfigIfDocumentWithArgument()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                TestDocument document = new TestDocument();
                int count = 1;
                DocumentConfig<int, int> config = new ContextConfig<int, int>(async (_, __) =>
                {
                    await Task.CompletedTask;
                    return count++;
                });

                // When
                int result1 = await config.GetAndCacheValueAsync(document, context, 100);
                int result2 = await config.GetAndCacheValueAsync(document, context, 100);

                // Then
                result1.ShouldBe(1);
                result2.ShouldBe(2);
            }
        }
    }
}
