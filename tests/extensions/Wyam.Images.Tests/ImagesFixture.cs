using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using NUnit.Framework;
using Shouldly;
using Wyam.Common;
using Wyam.Common.Documents;
using Wyam.Common.IO;
using Wyam.Common.Meta;
using Wyam.Testing;
using Wyam.Testing.Documents;
using Wyam.Testing.Execution;

namespace Wyam.Images.Tests
{
    [TestFixture]
    public class ImagesFixture : BaseFixture
    {
        public class ExecuteTests : ImagesFixture
        {
            [Test]
            public async Task OutputsTheSameAsInput()
            {
                // Given
                MemoryStream fileStream = GetTestFileStream("logo-square.png");
                TestDocument document = new TestDocument(
                    new FilePath("/input/a/b/test.png"),
                    null,
                    fileStream);
                Image module = new Image();

                // When
                TestDocument result = await ExecuteAsync(document, module).SingleAsync();

                // Then
                result.Destination.FullPath.ShouldBe("a/b/test.png");
            }

            [Test]
            public async Task ChangesPathWhenOutputFormatSpecified()
            {
                // Given
                MemoryStream fileStream = GetTestFileStream("logo-square.png");
                TestDocument document = new TestDocument(
                    new FilePath("/input/a/b/test.png"),
                    null,
                    fileStream);
                Image module = new Image().OutputAsGif();

                // When
                TestDocument result = await ExecuteAsync(document, module).SingleAsync();

                // Then
                result.Destination.FullPath.ShouldBe("a/b/test.gif");
            }

            [Test]
            public async Task ChangesPathWhenBrightnessSpecified()
            {
                // Given
                MemoryStream fileStream = GetTestFileStream("logo-square.png");
                TestDocument document = new TestDocument(
                    new FilePath("/input/a/b/test.png"),
                    null,
                    fileStream);
                Image module = new Image().Brightness(123);

                // When
                TestDocument result = await ExecuteAsync(document, module).SingleAsync();

                // Then
                result.Destination.FullPath.ShouldBe("a/b/test-b123.png");
            }
        }
    }
}
