using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Shouldly;
using Statiq.Testing;

namespace Statiq.Common.Tests.Meta
{
    [TestFixture]
    public class IMetadataGetExtensionsFixture : BaseFixture
    {
        public class TryGetValueTests : IMetadataGetExtensionsFixture
        {
            [Test]
            public void ReturnsFalseForNullMetadata()
            {
                // Given, When
                bool result = ((IMetadata)null).TryGetValue<object>("Foo", out object value);

                // Then
                result.ShouldBeFalse();
                value.ShouldBeNull();
            }

            [Test]
            public void ReturnsFalseForNullKey()
            {
                // Given
                TestMetadata metadata = new TestMetadata();

                // When
                bool result = metadata.TryGetValue<object>(null, out object value);

                // Then
                result.ShouldBeFalse();
                value.ShouldBeNull();
            }

            [Test]
            public void ReturnsFalseForMissingKey()
            {
                // Given
                TestMetadata metadata = new TestMetadata();

                // When
                bool result = metadata.TryGetValue<object>("Foo", out object value);

                // Then
                result.ShouldBeFalse();
                value.ShouldBeNull();
            }

            [Test]
            public void ReturnsObjectValue()
            {
                // Given
                TestMetadata metadata = new TestMetadata()
                {
                    { "Foo", 2 }
                };

                // When
                bool result = metadata.TryGetValue<object>("Foo", out object value);

                // Then
                result.ShouldBeTrue();
                value.ShouldBeOfType<int>().ShouldBe(2);
            }

            [Test]
            public void ReturnsTypedValue()
            {
                // Given
                TestMetadata metadata = new TestMetadata()
                {
                    { "Foo", 2 }
                };

                // When
                bool result = metadata.TryGetValue("Foo", out int value);

                // Then
                result.ShouldBeTrue();
                value.ShouldBe(2);
            }

            [Test]
            public void ReturnsConvertedValue()
            {
                // Given
                TestMetadata metadata = new TestMetadata()
                {
                    { "Foo", "2" }
                };

                // When
                bool result = metadata.TryGetValue("Foo", out int value);

                // Then
                result.ShouldBeTrue();
                value.ShouldBe(2);
            }

            [Test]
            public void ReturnsFalseIfNoConversion()
            {
                // Given
                TestMetadata metadata = new TestMetadata()
                {
                    { "Foo", "abc" }
                };

                // When
                bool result = metadata.TryGetValue("Foo", out TryGetValueTests value);

                // Then
                result.ShouldBeFalse();
                value.ShouldBeNull();
            }
        }
    }
}
