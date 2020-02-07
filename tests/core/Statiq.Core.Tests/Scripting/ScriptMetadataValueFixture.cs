using System.Text;
using NUnit.Framework;
using Shouldly;
using Statiq.Common;
using Statiq.Testing;

namespace Statiq.Core.Tests.Scripting
{
    [TestFixture]
    public class ScriptMetadataValueFixture : BaseFixture
    {
        public class TryGetMetadataValueTests : ScriptMetadataValueFixture
        {
            [Test]
            public void ReturnsFalseIfNull()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();

                // When
                bool result = ScriptMetadataValue.TryGetMetadataValue(null, context, out ScriptMetadataValue metadataValue);

                // Then
                result.ShouldBeFalse();
                metadataValue.ShouldBeNull();
            }

            [Test]
            public void ReturnsFalseForEmptyString()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();

                // When
                bool result = ScriptMetadataValue.TryGetMetadataValue(string.Empty, context, out ScriptMetadataValue metadataValue);

                // Then
                result.ShouldBeFalse();
                metadataValue.ShouldBeNull();
            }

            [Test]
            public void ReturnsFalseForNonString()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();

                // When
                bool result = ScriptMetadataValue.TryGetMetadataValue(1234, context, out ScriptMetadataValue metadataValue);

                // Then
                result.ShouldBeFalse();
                metadataValue.ShouldBeNull();
            }

            // To test values that can convert to a string
            [Test]
            public void ReturnsFalseForStringBuilder()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();

                // When
                bool result = ScriptMetadataValue.TryGetMetadataValue(new StringBuilder("=> $\"ABC {1+2} XYZ\""), context, out ScriptMetadataValue metadataValue);

                // Then
                result.ShouldBeFalse();
                metadataValue.ShouldBeNull();
            }

            [Test]
            public void ReturnsFalseForNormalString()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();

                // When
                bool result = ScriptMetadataValue.TryGetMetadataValue("1234", context, out ScriptMetadataValue metadataValue);

                // Then
                result.ShouldBeFalse();
                metadataValue.ShouldBeNull();
            }

            [TestCase("=> $\"ABC {1+2} XYZ\"")]
            [TestCase("=> return $\"ABC {1+2} XYZ\";")]
            [TestCase("=> { int x = 1 + 2; return $\"ABC {x} XYZ\"; }")]
            [TestCase("=> int x = 1 + 2; return $\"ABC {x} XYZ\";")]
            [TestCase("  => $\"ABC {1+2} XYZ\"")]
            public void ReturnsTrueForValidScript(string value)
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();

                // When
                bool result = ScriptMetadataValue.TryGetMetadataValue(value, context, out ScriptMetadataValue metadataValue);

                // Then
                result.ShouldBeTrue();
                metadataValue.ShouldNotBeNull();
            }
        }
    }
}
