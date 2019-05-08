using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using Shouldly;
using Wyam.Common.Configuration;
using Wyam.Testing;

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
                (await config.GetAndTransformValueAsync(null, null)).ShouldBe(10);
            }

            [Test]
            public async Task CastsToObject()
            {
                // Given, When
                DocumentConfig<object> config = 10;

                // Then
                (await config.GetAndTransformValueAsync(null, null)).ShouldBe(10);
            }

            [Test]
            public async Task CastsFromDocumentConfigToObject()
            {
                // Given, When
                DocumentConfig<object> config = new DocumentConfig<int>((_, __) => Task.FromResult(10));

                // Then
                (await config.GetAndTransformValueAsync(null, null)).ShouldBe(10);
            }

            [Test]
            public async Task CastsFromDocumentConfigToObjectEnumerable()
            {
                // Given, When
                DocumentConfig<IEnumerable<object>> config = new DocumentConfig<int>((_, __) => Task.FromResult(10));

                // Then
                (await config.GetAndTransformValueAsync(null, null)).ShouldBe(new object[] { 10 });
            }

            [Test]
            public async Task CastsFromDocumentConfigOfEnumerableToObjectEnumerable()
            {
                // Given, When
                DocumentConfig<IEnumerable<object>> config =
                    new DocumentConfig<IEnumerable<int>>((_, __) => Task.FromResult((IEnumerable<int>)new[] { 8, 9, 10 }));

                // Then
                (await config.GetAndTransformValueAsync(null, null)).ShouldBe(new object[] { 8, 9, 10 });
            }
        }
    }
}
