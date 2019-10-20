using System.Threading.Tasks;
using NUnit.Framework;
using Shouldly;
using Statiq.Common;
using Statiq.Testing;

namespace Statiq.Core.Tests.Modules.Contents
{
    [TestFixture]
    public class ReplaceContentFixture : BaseFixture
    {
        public class ExecuteTests : ReplaceContentFixture
        {
            [Test]
            public async Task ReplacesContent()
            {
                // Given
                TestDocument document = new TestDocument("Original");
                ReplaceContent replace = new ReplaceContent("Replaced");

                // When
                TestDocument result = await ExecuteAsync(document, replace).SingleAsync();

                // Then
                result.Content.ShouldBe("Replaced");
            }

            [Test]
            public async Task ReplacesContentWithEmptyString()
            {
                // Given
                TestDocument document = new TestDocument("Original");
                ReplaceContent replace = new ReplaceContent(string.Empty);

                // When
                TestDocument result = await ExecuteAsync(document, replace).SingleAsync();

                // Then
                result.Content.ShouldBeEmpty();
            }

            [Test]
            public async Task OutputsOriginalDocumentForNullContent()
            {
                // Given
                TestDocument document = new TestDocument("Original");
                ReplaceContent replace = new ReplaceContent((string)null);

                // When
                TestDocument result = await ExecuteAsync(document, replace).SingleAsync();

                // Then
                result.ShouldBe(document);
            }
        }
    }
}
