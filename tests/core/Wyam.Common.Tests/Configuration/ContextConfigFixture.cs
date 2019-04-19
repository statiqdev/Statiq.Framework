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
        }
    }
}
