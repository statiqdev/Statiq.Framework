using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Shouldly;
using Wyam.Common;
using Wyam.Common.Documents;
using Wyam.Common.Meta;
using Wyam.Testing;
using Wyam.Testing.Documents;
using Wyam.Testing.Execution;

namespace Wyam.Markdown.Tests
{
    [TestFixture]
    [Parallelizable(ParallelScope.Self | ParallelScope.Children)]
    public class MarkdownFixture : BaseFixture
    {
        public class ExecuteTests : MarkdownFixture
        {
            [Test]
            public async Task RendersMarkdown()
            {
                // Given
                const string input = @"Line 1
*Line 2*
# Line 3";
                const string output = @"<p>Line 1
<em>Line 2</em></p>
<h1>Line 3</h1>
";
                TestDocument document = new TestDocument(input);
                Markdown markdown = new Markdown();

                // When
                TestDocument result = await ExecuteAsync(document, markdown).SingleAsync();

                // Then
                result.Content.ShouldBe(output, StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public async Task CanUseExternalExtensionDirectly()
            {
                TestDocument document = new TestDocument();
                TestMarkdownExtension extension = new TestMarkdownExtension();
                Markdown markdown = new Markdown().UseExtension(extension);

                // When
                await ExecuteAsync(document, markdown);

                // Then
                extension.ReceivedSetup.ShouldBeTrue();
            }

            [Test]
            public async Task CanUseExternalExtension()
            {
                const string input = "![Alt text](/path/to/img.jpg)";
                const string output = @"<p><img src=""/path/to/img.jpg"" class=""ui spaced image"" alt=""Alt text"" /></p>
";
                TestDocument document = new TestDocument(input);
                Type[] o = { typeof(TestMarkdownExtension) };
                IEnumerable<Type> cast = o as IEnumerable<Type>;
                Markdown markdown = new Markdown().UseExtensions(cast);

                // When
                TestDocument result = await ExecuteAsync(document, markdown).SingleAsync();

                // Then
                result.Content.ShouldBe(output, StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public async Task CanUseMultipleExternalExtensions()
            {
                const string input = "![Alt text](/path/to/img.jpg)";
                const string output = @"<p><img src=""/path/to/img.jpg"" class=""ui spaced image second"" alt=""Alt text"" /></p>
";
                TestDocument document = new TestDocument(input);
                Type[] o =
                {
                    typeof(TestMarkdownExtension),
                    typeof(AlternateTestMarkdownExtension)
                };
                IEnumerable<Type> cast = o;
                Markdown markdown = new Markdown().UseExtensions(cast);

                // When
                TestDocument result = await ExecuteAsync(document, markdown).SingleAsync();

                // Then
                result.Content.ShouldBe(output, StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public async Task DoesNotRenderSpecialAttributesByDefault()
            {
                // Given
                const string input = "[link](url){#id .class}";
                const string output = @"<p><a href=""url"">link</a>{#id .class}</p>
";
                TestDocument document = new TestDocument(input);
                Markdown markdown = new Markdown();

                // When
                TestDocument result = await ExecuteAsync(document, markdown).SingleAsync();

                // Then
                result.Content.ShouldBe(output, StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public async Task DoesRenderSpecialAttributesIfExtensionsActive()
            {
                // Given
                const string input = "[link](url){#id .class}";
                const string output = @"<p><a href=""url"" id=""id"" class=""class"">link</a></p>
";
                TestDocument document = new TestDocument(input);
                Markdown markdown = new Markdown().UseExtensions();

                // When
                TestDocument result = await ExecuteAsync(document, markdown).SingleAsync();

                // Then
                result.Content.ShouldBe(output, StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public async Task DoesNotRenderDefinitionListWithoutExtensions()
            {
                // Given
                const string input = @"Apple
:   Pomaceous fruit of plants of the genus Malus in 
    the family Rosaceae.";
                const string output = @"<p>Apple
:   Pomaceous fruit of plants of the genus Malus in
the family Rosaceae.</p>
";
                TestDocument document = new TestDocument(input);
                Markdown markdown = new Markdown();

                // When
                TestDocument result = await ExecuteAsync(document, markdown).SingleAsync();

                // Then
                result.Content.ShouldBe(output, StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public async Task DoesRenderDefintionListWithSpecificConfiguration()
            {
                // Given
                const string input = @"Apple
:   Pomaceous fruit of plants of the genus Malus in 
    the family Rosaceae.";
                const string output = @"<dl>
<dt>Apple</dt>
<dd>Pomaceous fruit of plants of the genus Malus in
the family Rosaceae.</dd>
</dl>
";
                TestDocument document = new TestDocument(input);
                Markdown markdown = new Markdown().UseConfiguration("definitionlists");

                // When
                TestDocument result = await ExecuteAsync(document, markdown).SingleAsync();

                // Then
                result.Content.ShouldBe(output, StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public async Task EscapesAtByDefault()
            {
                // Given
                const string input = "Looking @Good, Man!";
                const string output = @"<p>Looking &#64;Good, Man!</p>
";
                TestDocument document = new TestDocument(input);
                Markdown markdown = new Markdown();

                // When
                TestDocument result = await ExecuteAsync(document, markdown).SingleAsync();

                // Then
                result.Content.ShouldBe(output, StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public async Task UnescapesDoubleAt()
            {
                // Given
                const string input = @"Looking @Good, \\@Man!";
                const string output = @"<p>Looking &#64;Good, @Man!</p>
";
                TestDocument document = new TestDocument(input);
                Markdown markdown = new Markdown();

                // When
                TestDocument result = await ExecuteAsync(document, markdown).SingleAsync();

                // Then
                result.Content.ShouldBe(output, StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public async Task DoesNotEscapeAtIfDisabled()
            {
                // Given
                const string input = "Looking @Good, Man!";
                const string output = @"<p>Looking @Good, Man!</p>
";
                TestDocument document = new TestDocument(input);
                Markdown markdown = new Markdown().EscapeAt(false);

                // When
                TestDocument result = await ExecuteAsync(document, markdown).SingleAsync();

                // Then
                result.Content.ShouldBe(output, StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public async Task RendersMarkdownFromMetadata()
            {
                // Given
                const string input = @"Line 1
*Line 2*
# Line 3";
                const string output = @"<p>Line 1
<em>Line 2</em></p>
<h1>Line 3</h1>
";
                TestDocument document = new TestDocument(new MetadataItems
                {
                    { "meta", input }
                });
                Markdown markdown = new Markdown("meta");

                // When
                TestDocument result = await ExecuteAsync(document, markdown).SingleAsync();

                // Then
                result.String("meta").ShouldBe(output, StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public async Task RendersMarkdownFromMetadataToNewKey()
            {
                // Given
                const string input = @"Line 1
*Line 2*
# Line 3";
                const string output = @"<p>Line 1
<em>Line 2</em></p>
<h1>Line 3</h1>
";
                TestDocument document = new TestDocument(new MetadataItems
                {
                    { "meta", input }
                });
                Markdown markdown = new Markdown("meta", "meta2");

                // When
                TestDocument result = await ExecuteAsync(document, markdown).SingleAsync();

                // Then
                result.String("meta2").ShouldBe(output, StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public async Task DoesNothingIfMetadataKeyDoesNotExist()
            {
                // Given
                TestDocument document = new TestDocument();
                Markdown markdown = new Markdown("meta");

                // When
                TestDocument result = await ExecuteAsync(document, markdown).SingleAsync();

                // Then
                result.ShouldBe(document);
            }

            [Test]
            public async Task UsePrependLinkRootSetting()
            {
                // Given
                const string input = "This is a [link](/link.html)";
                string output = @"<p>This is a <a href=""/virtual-dir/link.html"">link</a></p>" + Environment.NewLine;
                TestExecutionContext context = new TestExecutionContext();
                context.Settings[Keys.LinkRoot] = "/virtual-dir";
                TestDocument document = new TestDocument(input);
                Markdown markdown = new Markdown().PrependLinkRoot(true);

                // When
                TestDocument result = await ExecuteAsync(document, context, markdown).SingleAsync();

                // Then
                result.Content.ShouldBe(output, StringCompareShould.IgnoreLineEndings);
            }
        }
    }
}