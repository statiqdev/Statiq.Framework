using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Shouldly;
using Wyam.Common.Documents;
using Wyam.Common.Meta;
using Wyam.Common.Util;
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
                TestExecutionContext context = new TestExecutionContext();
                TestDocument document = new TestDocument(input);
                Markdown markdown = new Markdown();

                // When
                IList<IDocument> results = await markdown.ExecuteAsync(new[] { document }, context).ToListAsync();  // Make sure to materialize the result list

                // Then
                results.Single().Content.ShouldBe(output, StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public async Task CanUseExternalExtensionDirectly()
            {
                TestMarkdownExtension extension = new TestMarkdownExtension();
                Markdown markdown = new Markdown().UseExtension(extension);

                // When
                await markdown.ExecuteAsync(new[] { new TestDocument(string.Empty) }, new TestExecutionContext()).ToListAsync();  // Make sure to materialize the result list

                // Then
                extension.ReceivedSetup.ShouldBeTrue();
            }

            [Test]
            public async Task CanUseExternalExtension()
            {
                const string input = "![Alt text](/path/to/img.jpg)";
                const string output = @"<p><img src=""/path/to/img.jpg"" class=""ui spaced image"" alt=""Alt text"" /></p>
";
                TestExecutionContext context = new TestExecutionContext();
                TestDocument document = new TestDocument(input);
                Type[] o = { typeof(TestMarkdownExtension) };
                IEnumerable<Type> cast = o as IEnumerable<Type>;
                Markdown markdown = new Markdown().UseExtensions(cast);

                // When
                IList<IDocument> results = await markdown.ExecuteAsync(new[] { document }, context).ToListAsync();  // Make sure to materialize the result list

                // Then
                results.Single().Content.ShouldBe(output, StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public async Task CanUseMultipleExternalExtensions()
            {
                const string input = "![Alt text](/path/to/img.jpg)";
                const string output = @"<p><img src=""/path/to/img.jpg"" class=""ui spaced image second"" alt=""Alt text"" /></p>
";

                TestExecutionContext context = new TestExecutionContext();
                TestDocument document = new TestDocument(input);
                Type[] o =
                {
                    typeof(TestMarkdownExtension),
                    typeof(AlternateTestMarkdownExtension)
                };
                IEnumerable<Type> cast = o;
                Markdown markdown = new Markdown().UseExtensions(cast);

                // When
                IList<IDocument> results = await markdown.ExecuteAsync(new[] { document }, context).ToListAsync();

                // Then
                results.Single().Content.ShouldBe(output, StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public async Task DoesNotRenderSpecialAttributesByDefault()
            {
                // Given
                const string input = "[link](url){#id .class}";
                const string output = @"<p><a href=""url"">link</a>{#id .class}</p>
";
                TestExecutionContext context = new TestExecutionContext();
                TestDocument document = new TestDocument(input);
                Markdown markdown = new Markdown();

                // When
                IList<IDocument> results = await markdown.ExecuteAsync(new[] { document }, context).ToListAsync();  // Make sure to materialize the result list

                // Then
                results.Single().Content.ShouldBe(output, StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public async Task DoesRenderSpecialAttributesIfExtensionsActive()
            {
                // Given
                const string input = "[link](url){#id .class}";
                const string output = @"<p><a href=""url"" id=""id"" class=""class"">link</a></p>
";
                TestExecutionContext context = new TestExecutionContext();
                TestDocument document = new TestDocument(input);
                Markdown markdown = new Markdown().UseExtensions();

                // When
                IList<IDocument> results = await markdown.ExecuteAsync(new[] { document }, context).ToListAsync();  // Make sure to materialize the result list

                // Then
                results.Single().Content.ShouldBe(output, StringCompareShould.IgnoreLineEndings);
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
                TestExecutionContext context = new TestExecutionContext();
                TestDocument document = new TestDocument(input);
                Markdown markdown = new Markdown();

                // When
                IList<IDocument> results = await markdown.ExecuteAsync(new[] { document }, context).ToListAsync();  // Make sure to materialize the result list

                // Then
                results.Single().Content.ShouldBe(output, StringCompareShould.IgnoreLineEndings);
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
                TestExecutionContext context = new TestExecutionContext();
                TestDocument document = new TestDocument(input);
                Markdown markdown = new Markdown().UseConfiguration("definitionlists");

                // When
                IList<IDocument> results = await markdown.ExecuteAsync(new[] { document }, context).ToListAsync();  // Make sure to materialize the result list

                // Then
                results.Single().Content.ShouldBe(output, StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public async Task EscapesAtByDefault()
            {
                // Given
                const string input = "Looking @Good, Man!";
                const string output = @"<p>Looking &#64;Good, Man!</p>
";
                TestExecutionContext context = new TestExecutionContext();
                TestDocument document = new TestDocument(input);
                Markdown markdown = new Markdown();

                // When
                IList<IDocument> results = await markdown.ExecuteAsync(new[] { document }, context).ToListAsync();  // Make sure to materialize the result list

                // Then
                results.Single().Content.ShouldBe(output, StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public async Task UnescapesDoubleAt()
            {
                // Given
                const string input = @"Looking @Good, \\@Man!";
                const string output = @"<p>Looking &#64;Good, @Man!</p>
";
                TestExecutionContext context = new TestExecutionContext();
                TestDocument document = new TestDocument(input);
                Markdown markdown = new Markdown();

                // When
                IList<IDocument> results = await markdown.ExecuteAsync(new[] { document }, context).ToListAsync();  // Make sure to materialize the result list

                // Then
                results.Single().Content.ShouldBe(output, StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public async Task DoesNotEscapeAtIfDisabled()
            {
                // Given
                const string input = "Looking @Good, Man!";
                const string output = @"<p>Looking @Good, Man!</p>
";
                TestExecutionContext context = new TestExecutionContext();
                TestDocument document = new TestDocument(input);
                Markdown markdown = new Markdown().EscapeAt(false);

                // When
                IList<IDocument> results = await markdown.ExecuteAsync(new[] { document }, context).ToListAsync();  // Make sure to materialize the result list

                // Then
                results.Single().Content.ShouldBe(output, StringCompareShould.IgnoreLineEndings);
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
                TestExecutionContext context = new TestExecutionContext();
                TestDocument document = new TestDocument(new MetadataItems
                {
                    { "meta", input }
                });
                Markdown markdown = new Markdown("meta");

                // When
                IList<IDocument> results = await markdown.ExecuteAsync(new[] { document }, context).ToListAsync();  // Make sure to materialize the result list

                // Then
                results.Single().String("meta").ShouldBe(output, StringCompareShould.IgnoreLineEndings);
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
                TestExecutionContext context = new TestExecutionContext();
                TestDocument document = new TestDocument(new MetadataItems
                {
                    { "meta", input }
                });
                Markdown markdown = new Markdown("meta", "meta2");

                // When
                IList<IDocument> results = await markdown.ExecuteAsync(new[] { document }, context).ToListAsync();  // Make sure to materialize the result list

                // Then
                results.Single().String("meta2").ShouldBe(output, StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public async Task DoesNothingIfMetadataKeyDoesNotExist()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                TestDocument document = new TestDocument();
                Markdown markdown = new Markdown("meta");

                // When
                IList<IDocument> results = await markdown.ExecuteAsync(new[] { document }, context).ToListAsync();  // Make sure to materialize the result list

                // Then
                results.ShouldBe(new[] { document });
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
                IList<IDocument> results = await markdown.ExecuteAsync(new[] { document }, context).ToListAsync();  // Make sure to materialize the result list

                // Then
                results.Single().Content.ShouldBe(output, StringCompareShould.IgnoreLineEndings);
            }
        }
    }
}