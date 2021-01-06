using System;
using NUnit.Framework;
using Shouldly;
using Statiq.Testing;

namespace Statiq.Common.Tests.Util
{
    [TestFixture]
    public class StringExtensionsFixture : BaseFixture
    {
        public class RemoveStartTests : StringExtensionsFixture
        {
            [TestCase("FooBar", null, "FooBar")]
            [TestCase("FooBar", "", "FooBar")]
            [TestCase(null, null, null)]
            [TestCase(null, "", null)]
            [TestCase(null, "Foo", null)]
            [TestCase("", null, "")]
            [TestCase("", "Foo", "")]
            [TestCase("FooBar", "Bar", "FooBar")]
            [TestCase("FooBar", "Foo", "Bar")]
            [TestCase("FooBar", "foo", "FooBar")]
            [TestCase("FooBar", "f", "FooBar")]
            [TestCase("FooBar", "F", "ooBar")]
            public void WithoutStringComparison(string input, string value, string expected)
            {
                // Given, When
                string result = input.RemoveStart(value);

                // Then
                result.ShouldBe(expected);
            }

            [TestCase("FooBar", null, "FooBar")]
            [TestCase("FooBar", "", "FooBar")]
            [TestCase(null, null, null)]
            [TestCase(null, "", null)]
            [TestCase(null, "Foo", null)]
            [TestCase("", null, "")]
            [TestCase("", "Foo", "")]
            [TestCase("FooBar", "Bar", "FooBar")]
            [TestCase("FooBar", "Foo", "Bar")]
            [TestCase("FooBar", "foo", "Bar")]
            [TestCase("FooBar", "f", "ooBar")]
            [TestCase("FooBar", "F", "ooBar")]
            public void OrdinalIgnoreCase(string input, string value, string expected)
            {
                // Given, When
                string result = input.RemoveStart(value, StringComparison.OrdinalIgnoreCase);

                // Then
                result.ShouldBe(expected);
            }
        }

        public class RemoveEndTests : StringExtensionsFixture
        {
            [TestCase("FooBar", null, "FooBar")]
            [TestCase("FooBar", "", "FooBar")]
            [TestCase(null, null, null)]
            [TestCase(null, "", null)]
            [TestCase(null, "Foo", null)]
            [TestCase("", null, "")]
            [TestCase("", "Foo", "")]
            [TestCase("FooBar", "Bar", "Foo")]
            [TestCase("FooBar", "Foo", "FooBar")]
            [TestCase("FooBar", "bar", "FooBar")]
            [TestCase("FooBar", "R", "FooBar")]
            [TestCase("FooBar", "r", "FooBa")]
            public void WithoutStringComparison(string input, string value, string expected)
            {
                // Given, When
                string result = input.RemoveEnd(value);

                // Then
                result.ShouldBe(expected);
            }

            [TestCase("FooBar", null, "FooBar")]
            [TestCase("FooBar", "", "FooBar")]
            [TestCase(null, null, null)]
            [TestCase(null, "", null)]
            [TestCase(null, "Foo", null)]
            [TestCase("", null, "")]
            [TestCase("", "Foo", "")]
            [TestCase("FooBar", "Bar", "Foo")]
            [TestCase("FooBar", "Foo", "FooBar")]
            [TestCase("FooBar", "bar", "Foo")]
            [TestCase("FooBar", "R", "FooBa")]
            [TestCase("FooBar", "r", "FooBa")]
            public void OrdinalIgnoreCase(string input, string value, string expected)
            {
                // Given, When
                string result = input.RemoveEnd(value, StringComparison.OrdinalIgnoreCase);

                // Then
                result.ShouldBe(expected);
            }
        }
    }
}
