using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Shouldly;
using Statiq.Common;
using Statiq.Testing;

namespace Statiq.Core.Tests.Shortcodes.Control
{
    [TestFixture]
    public class IfShortcodeFixture : BaseFixture
    {
        public class ExecuteTests : ForEachShortcodeFixture
        {
            [Test]
            public async Task ReturnsNullIfMissingKey()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                TestDocument document = new TestDocument();
                KeyValuePair<string, string>[] args = new KeyValuePair<string, string>[]
                {
                    new KeyValuePair<string, string>("Key", "Foo")
                };
                IfShortcode shortcode = new IfShortcode();

                // When
                IDocument result = await shortcode.ExecuteAsync(args, "Fizzbuzz", document, context);

                // Then
                result.ShouldBeNull();
            }

            [Test]
            public async Task ReturnsDocumentIfEqual()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                TestDocument document = new TestDocument(new MetadataItems
                {
                    { "Foo", "bar" }
                });
                KeyValuePair<string, string>[] args = new KeyValuePair<string, string>[]
                {
                    new KeyValuePair<string, string>("Key", "Foo"),
                    new KeyValuePair<string, string>("Value", "bar")
                };
                IfShortcode shortcode = new IfShortcode();

                // When
                IDocument result = await shortcode.ExecuteAsync(args, "Fizzbuzz", document, context);

                // Then
                result.ShouldBeOfType<TestDocument>().Content.ShouldBe("Fizzbuzz");
            }

            [Test]
            public async Task ReturnsNullIfNotEqual()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                TestDocument document = new TestDocument(new MetadataItems
                {
                    { "Foo", "bar" }
                });
                KeyValuePair<string, string>[] args = new KeyValuePair<string, string>[]
                {
                    new KeyValuePair<string, string>("Key", "Foo"),
                    new KeyValuePair<string, string>("Value", "buz")
                };
                IfShortcode shortcode = new IfShortcode();

                // When
                IDocument result = await shortcode.ExecuteAsync(args, "Fizzbuzz", document, context);

                // Then
                result.ShouldBeNull();
            }

            [Test]
            public async Task ReturnsDocumentIfConvertedEqual()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                TestDocument document = new TestDocument(new MetadataItems
                {
                    { "Foo", 1 }
                });
                KeyValuePair<string, string>[] args = new KeyValuePair<string, string>[]
                {
                    new KeyValuePair<string, string>("Key", "Foo"),
                    new KeyValuePair<string, string>("Value", "1")
                };
                IfShortcode shortcode = new IfShortcode();

                // When
                IDocument result = await shortcode.ExecuteAsync(args, "Fizzbuzz", document, context);

                // Then
                result.ShouldBeOfType<TestDocument>().Content.ShouldBe("Fizzbuzz");
            }

            [Test]
            public async Task ReturnsNullIfConvertedNotEqual()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                TestDocument document = new TestDocument(new MetadataItems
                {
                    { "Foo", 1 }
                });
                KeyValuePair<string, string>[] args = new KeyValuePair<string, string>[]
                {
                    new KeyValuePair<string, string>("Key", "Foo"),
                    new KeyValuePair<string, string>("Value", "2")
                };
                IfShortcode shortcode = new IfShortcode();

                // When
                IDocument result = await shortcode.ExecuteAsync(args, "Fizzbuzz", document, context);

                // Then
                result.ShouldBeNull();
            }

            [Test]
            public async Task ReturnsDocumentIfTrue()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                TestDocument document = new TestDocument(new MetadataItems
                {
                    { "Foo", true }
                });
                KeyValuePair<string, string>[] args = new KeyValuePair<string, string>[]
                {
                    new KeyValuePair<string, string>("Key", "Foo")
                };
                IfShortcode shortcode = new IfShortcode();

                // When
                IDocument result = await shortcode.ExecuteAsync(args, "Fizzbuzz", document, context);

                // Then
                result.ShouldBeOfType<TestDocument>().Content.ShouldBe("Fizzbuzz");
            }

            [Test]
            public async Task ReturnsDocumentIfConvertedTrue()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                TestDocument document = new TestDocument(new MetadataItems
                {
                    { "Foo", "true" }
                });
                KeyValuePair<string, string>[] args = new KeyValuePair<string, string>[]
                {
                    new KeyValuePair<string, string>("Key", "Foo")
                };
                IfShortcode shortcode = new IfShortcode();

                // When
                IDocument result = await shortcode.ExecuteAsync(args, "Fizzbuzz", document, context);

                // Then
                result.ShouldBeOfType<TestDocument>().Content.ShouldBe("Fizzbuzz");
            }

            [Test]
            public async Task ReturnsNullIfFalse()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                TestDocument document = new TestDocument(new MetadataItems
                {
                    { "Foo", false }
                });
                KeyValuePair<string, string>[] args = new KeyValuePair<string, string>[]
                {
                    new KeyValuePair<string, string>("Key", "Foo")
                };
                IfShortcode shortcode = new IfShortcode();

                // When
                IDocument result = await shortcode.ExecuteAsync(args, "Fizzbuzz", document, context);

                // Then
                result.ShouldBeNull();
            }

            [Test]
            public async Task ReturnsNullIfConvertedFalse()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                TestDocument document = new TestDocument(new MetadataItems
                {
                    { "Foo", "false" }
                });
                KeyValuePair<string, string>[] args = new KeyValuePair<string, string>[]
                {
                    new KeyValuePair<string, string>("Key", "Foo")
                };
                IfShortcode shortcode = new IfShortcode();

                // When
                IDocument result = await shortcode.ExecuteAsync(args, "Fizzbuzz", document, context);

                // Then
                result.ShouldBeNull();
            }

            [Test]
            public async Task ReturnsNullIfCanNotConvertToBool()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                TestDocument document = new TestDocument(new MetadataItems
                {
                    { "Foo", "abc" }
                });
                KeyValuePair<string, string>[] args = new KeyValuePair<string, string>[]
                {
                    new KeyValuePair<string, string>("Key", "Foo")
                };
                IfShortcode shortcode = new IfShortcode();

                // When
                IDocument result = await shortcode.ExecuteAsync(args, "Fizzbuzz", document, context);

                // Then
                result.ShouldBeNull();
            }

            [Test]
            public async Task ReturnsNullIfCanNotConvert()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                TestDocument document = new TestDocument(new MetadataItems
                {
                    { "Foo", new DateTime(2019, 1, 1) }
                });
                KeyValuePair<string, string>[] args = new KeyValuePair<string, string>[]
                {
                    new KeyValuePair<string, string>("Key", "Foo"),
                    new KeyValuePair<string, string>("Value", "abc")
                };
                IfShortcode shortcode = new IfShortcode();

                // When
                IDocument result = await shortcode.ExecuteAsync(args, "Fizzbuzz", document, context);

                // Then
                result.ShouldBeNull();
            }

            [Test]
            public async Task ReturnsDocumentIfScriptEqual()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                context.ScriptHelper = new ScriptHelper(context);
                TestDocument document = new TestDocument()
                {
                    { "Foo", 122 }
                };
                KeyValuePair<string, string>[] args = new KeyValuePair<string, string>[]
                {
                    new KeyValuePair<string, string>("Key", "=> (int)Foo + 1"),
                    new KeyValuePair<string, string>("Value", "123")
                };
                IfShortcode shortcode = new IfShortcode();

                // When
                IDocument result = await shortcode.ExecuteAsync(args, "Fizzbuzz", document, context);

                // Then
                result.ShouldBeOfType<TestDocument>().Content.ShouldBe("Fizzbuzz");
            }
        }
    }
}
