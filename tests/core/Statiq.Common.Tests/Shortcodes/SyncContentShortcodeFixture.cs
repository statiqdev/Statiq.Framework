using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using Shouldly;
using Statiq.Testing;

namespace Statiq.Common.Tests.Shortcodes
{
    [TestFixture]
    public class SyncContentShortcodeFixture : BaseFixture
    {
        public class ExecuteTests : ContentShortcodeFixture
        {
            [Test]
            public async Task ReturnsNullForNullContent()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                TestDocument document = new TestDocument();
                IShortcode shortcode = new TestShortcode(null);

                // When
                IEnumerable<IDocument> result = await shortcode.ExecuteAsync(null, null, document, context);

                // Then
                result.ShouldBeNull();
            }

            [Test]
            public async Task ReturnsNullForEmptyContent()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                TestDocument document = new TestDocument();
                IShortcode shortcode = new TestShortcode(string.Empty);

                // When
                IEnumerable<IDocument> result = await shortcode.ExecuteAsync(null, null, document, context);

                // Then
                result.ShouldBeNull();
            }

            [Test]
            public async Task ReturnsContent()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                TestDocument document = new TestDocument();
                IShortcode shortcode = new TestShortcode("Foo");

                // When
                IEnumerable<IDocument> result = await shortcode.ExecuteAsync(null, null, document, context);

                // Then
                result.ShouldHaveSingleItem().ShouldBeOfType<TestDocument>().Content.ShouldBe("Foo");
            }
        }

        public class TestShortcode : SyncContentShortcode
        {
            public string Content { get; set; }

            public TestShortcode(string content)
            {
                Content = content;
            }

            public override string Execute(KeyValuePair<string, string>[] args, string content, IDocument document, IExecutionContext context) => Content;
        }
    }
}
