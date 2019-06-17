using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using NUnit.Framework;
using Shouldly;
using Statiq.Common.Meta;
using Statiq.Testing;
using Statiq.Testing.Documents;
using Statiq.Testing.Execution;

namespace Statiq.Core.Tests.Shortcodes.Metadata
{
    [TestFixture]
    public class MetaFixture : BaseFixture
    {
        public class ExecuteTests : MetaFixture
        {
            [Test]
            public async Task RendersMetadata()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                TestDocument document = new TestDocument(new MetadataItems
                {
                    { "Foo", "Bar" }
                });
                KeyValuePair<string, string>[] args = new KeyValuePair<string, string>[]
                {
                    new KeyValuePair<string, string>(null, "Foo")
                };
                Core.Shortcodes.Metadata.Meta shortcode = new Core.Shortcodes.Metadata.Meta();

                // When
                TestDocument result = (TestDocument)await shortcode.ExecuteAsync(args, string.Empty, document, context);

                // Then
                result.Content.ShouldBe("Bar");
            }

            [Test]
            public async Task EmptyForMissingMetadata()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                TestDocument document = new TestDocument(new MetadataItems
                {
                    { "Foo", "Bar" }
                });
                KeyValuePair<string, string>[] args = new KeyValuePair<string, string>[]
                {
                    new KeyValuePair<string, string>(null, "Fizz")
                };
                Core.Shortcodes.Metadata.Meta shortcode = new Core.Shortcodes.Metadata.Meta();

                // When
                TestDocument result = (TestDocument)await shortcode.ExecuteAsync(args, string.Empty, document, context);

                // Then
                result.Content.ShouldBeEmpty();
            }
        }
    }
}
