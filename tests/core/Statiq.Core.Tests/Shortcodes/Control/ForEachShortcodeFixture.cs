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
    public class ForEachShortcodeFixture : BaseFixture
    {
        public class ExecuteTests : ForEachShortcodeFixture
        {
            [Test]
            public async Task ThrowsForInvalidValueKey()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                TestDocument document = new TestDocument();
                KeyValuePair<string, string>[] args = new KeyValuePair<string, string>[]
                {
                    new KeyValuePair<string, string>("Key", "Foo"),
                    new KeyValuePair<string, string>("ValueKey", string.Empty)
                };
                ForEachShortcode shortcode = new ForEachShortcode();

                // When, Then
                await Should.ThrowAsync<ShortcodeArgumentException>(async () => await shortcode.ExecuteAsync(args, string.Empty, document, context));
            }

            [Test]
            public async Task IteratesItems()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                TestDocument document = new TestDocument(new MetadataItems
                {
                    { "Foo", new int[] { 5, 6, 7 } }
                });
                KeyValuePair<string, string>[] args = new KeyValuePair<string, string>[]
                {
                    new KeyValuePair<string, string>("Key", "Foo"),
                    new KeyValuePair<string, string>("ValueKey", "Bar")
                };
                ForEachShortcode shortcode = new ForEachShortcode();

                // When
                IEnumerable<IDocument> result = await shortcode.ExecuteAsync(args, "Fizzbuzz", document, context);

                // Then
                result.Cast<TestDocument>().ShouldAllBe(x => x.Content == "Fizzbuzz");
                result.Select(x => x.Get("Bar")).ShouldBe(new object[] { 5, 6, 7 });
            }

            [Test]
            public async Task IteratesSingleItem()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                TestDocument document = new TestDocument(new MetadataItems
                {
                    { "Foo", 5 }
                });
                KeyValuePair<string, string>[] args = new KeyValuePair<string, string>[]
                {
                    new KeyValuePair<string, string>("Key", "Foo"),
                    new KeyValuePair<string, string>("ValueKey", "Bar")
                };
                ForEachShortcode shortcode = new ForEachShortcode();

                // When
                IEnumerable<IDocument> result = await shortcode.ExecuteAsync(args, "Fizzbuzz", document, context);

                // Then
                result.Cast<TestDocument>().ShouldAllBe(x => x.Content == "Fizzbuzz");
                result.Select(x => x.Get("Bar")).ShouldBe(new object[] { 5 });
            }

            [Test]
            public async Task MissingKey()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                TestDocument document = new TestDocument();
                KeyValuePair<string, string>[] args = new KeyValuePair<string, string>[]
                {
                    new KeyValuePair<string, string>("Key", "Foo"),
                    new KeyValuePair<string, string>("ValueKey", "Bar")
                };
                ForEachShortcode shortcode = new ForEachShortcode();

                // When
                IEnumerable<IDocument> result = await shortcode.ExecuteAsync(args, "Fizzbuzz", document, context);

                // Then
                result.ShouldBeNull();
            }

            [Test]
            public async Task IteratesScript()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                context.ScriptHelper = new ScriptHelper(context);
                TestDocument document = new TestDocument(new MetadataItems
                {
                    { "Foo", 5 }
                });
                KeyValuePair<string, string>[] args = new KeyValuePair<string, string>[]
                {
                    new KeyValuePair<string, string>("Key", "=> new int[] { 1, 2, (int)Foo + 1 }"),
                    new KeyValuePair<string, string>("ValueKey", "Bar")
                };
                ForEachShortcode shortcode = new ForEachShortcode();

                // When
                IEnumerable<IDocument> result = await shortcode.ExecuteAsync(args, "Fizzbuzz", document, context);

                // Then
                result.Cast<TestDocument>().ShouldAllBe(x => x.Content == "Fizzbuzz");
                result.Select(x => x.Get("Bar")).ShouldBe(new object[] { 1, 2, 6 });
            }
        }
    }
}
