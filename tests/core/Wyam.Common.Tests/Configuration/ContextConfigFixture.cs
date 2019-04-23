using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Shouldly;
using Wyam.Common.Configuration;
using Wyam.Testing;

namespace Wyam.Common.Tests.Configuration
{
    [TestFixture]
    public class ContextConfigFixture : BaseFixture
    {
        public class CastOperatorTests : ContextConfigFixture
        {
            [Test]
            public async Task CastsToMatchingType()
            {
                // Given, When
                ContextConfig<int> config = 10;

                // Then
                (await config.GetAndCacheValueAsync(null, null)).ShouldBe(10);
            }

            [Test]
            public async Task CastsToObject()
            {
                // Given, When
                ContextConfig<object> config = 10;

                // Then
                (await config.GetAndCacheValueAsync(null, null)).ShouldBe(10);
            }

            [Test]
            public async Task CastsFromContextConfigToObject()
            {
                // Given, When
                ContextConfig<object> config = new ContextConfig<int>(_ => Task.FromResult(10));

                // Then
                (await config.GetAndCacheValueAsync(null, null)).ShouldBe(10);
            }

            [Test]
            public async Task CastsFromContextConfigToDocumentConfigOfObject()
            {
                // Given, When
                DocumentConfig<object> config = new ContextConfig<int>(_ => Task.FromResult(10));

                // Then
                (await config.GetAndCacheValueAsync(null, null)).ShouldBe(10);
            }

            [Test]
            public async Task CastsFromContextConfigToObjectEnumerable()
            {
                // Given, When
                ContextConfig<IEnumerable<object>> config = new ContextConfig<int>(_ => Task.FromResult(10));

                // Then
                (await config.GetAndCacheValueAsync(null, null)).ShouldBe(new object[] { 10 });
            }

            [Test]
            public async Task CastsFromContextConfigToDocumentConfigOfObjectEnumerable()
            {
                // Given, When
                DocumentConfig<IEnumerable<object>> config = new ContextConfig<int>(_ => Task.FromResult(10));

                // Then
                (await config.GetAndCacheValueAsync(null, null)).ShouldBe(new object[] { 10 });
            }

            [Test]
            public async Task CastsFromContextConfigOfEnumerableToObjectEnumerable()
            {
                // Given, When
                ContextConfig<IEnumerable<object>> config =
                    new ContextConfig<IEnumerable<int>>(_ => Task.FromResult((IEnumerable<int>)new[] { 8, 9, 10 }));

                // Then
                (await config.GetAndCacheValueAsync(null, null)).ShouldBe(new object[] { 8, 9, 10 });
            }

            [Test]
            public async Task CastsFromContextConfigOfEnumerableToDocumentConfigOfObjectEnumerable()
            {
                // Given, When
                DocumentConfig<IEnumerable<object>> config =
                    new ContextConfig<IEnumerable<int>>(_ => Task.FromResult((IEnumerable<int>)new[] { 8, 9, 10 }));

                // Then
                (await config.GetAndCacheValueAsync(null, null)).ShouldBe(new object[] { 8, 9, 10 });
            }

            [Test]
            public async Task CastingFromArgToArgAndObjectDocumentConfig()
            {
                // Given, When
                DocumentConfig<int, object> config = Config.FromContext<int, int>((ctx, arg) => 10);

                // Then
                (await config.GetAndCacheValueAsync(null, null, 100)).ShouldBe(10);
            }

            [Test]
            public async Task CastingFromArgToArgAndObjectContextConfig()
            {
                // Given, When
                ContextConfig<int, object> config = Config.FromContext<int, int>((ctx, arg) => 10);

                // Then
                (await config.GetAndCacheValueAsync(null, null, 100)).ShouldBe(10);
            }

            [Test]
            public void CastingFromArgToObjectDocumentConfigShouldThrow()
            {
                // Given, When
                DocumentConfig<object> config;

                // Then
                Should.Throw<InvalidCastException>(() => config = Config.FromContext<int, int>((ctx, arg) => 10));
            }

            [Test]
            public void CastingFromArgToObjectContextConfigShouldThrow()
            {
                // Given, When
                ContextConfig<object> config;

                // Then
                Should.Throw<InvalidCastException>(() => config = Config.FromContext<int, int>((ctx, arg) => 10));
            }

            [Test]
            public async Task CastingFromArgToDocumentConfig()
            {
                // Given, When
                DocumentConfig<int, int> config = Config.FromContext<int, int>((ctx, arg) => 10);

                // Then
                (await config.GetAndCacheValueAsync(null, null, 100)).ShouldBe(10);
            }

            [Test]
            public async Task CastingFromArgToContextConfig()
            {
                // Given, When
                ContextConfig<int, int> config = Config.FromContext<int, int>((ctx, arg) => 10);

                // Then
                (await config.GetAndCacheValueAsync(null, null, 100)).ShouldBe(10);
            }
        }
    }
}
