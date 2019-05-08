using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Wyam.Common.Configuration;
using Wyam.Common.Documents;
using Wyam.Common.Execution;
using Wyam.Core.Modules.Control;
using Wyam.Core.Modules.Extensibility;
using Wyam.Testing;
using Wyam.Testing.Documents;
using Wyam.Testing.Execution;

namespace Wyam.Core.Tests.Modules.Control
{
    [TestFixture]
    [NonParallelizable]
    public class FrontMatterFixture : BaseFixture
    {
        public class ExecuteTests : FrontMatterFixture
        {
            [Test]
            public async Task DefaultCtorSplitsAtDashes()
            {
                // Given
                IExecutionContext context = new TestExecutionContext();
                IDocument[] inputs =
                {
                    new TestDocument(@"FM1
FM2
---
Content1
Content2")
                };
                string frontMatterContent = null;
                FrontMatter frontMatter = new FrontMatter(new ExecuteDocument(Config.FromDocument(async x =>
                {
                    frontMatterContent = await x.GetStringAsync();
                    return new[] { x };
                })));

                // When
                IEnumerable<IDocument> documents = await frontMatter.ExecuteAsync(inputs, context);

                // Then
                Assert.AreEqual(1, documents.Count());
                Assert.AreEqual(
                    @"FM1
FM2
", frontMatterContent);
                Assert.AreEqual(
                    @"Content1
Content2", await documents.First().GetStringAsync());
            }

            [Test]
            public async Task EmptyFirstLineWithDelimiterTreatsAsFrontMatter()
            {
                // Given
                IExecutionContext context = new TestExecutionContext();
                IDocument[] inputs =
                {
                    new TestDocument(@"
---
FM1
FM2
---
Content1
Content2")
                };
                string frontMatterContent = null;
                FrontMatter frontMatter = new FrontMatter(new ExecuteDocument(Config.FromDocument(async x =>
                {
                    frontMatterContent = await x.GetStringAsync();
                    return new[] { x };
                })));

                // When
                IEnumerable<IDocument> documents = await frontMatter.ExecuteAsync(inputs, context);

                // Then
                Assert.AreEqual(1, documents.Count());
                Assert.AreEqual(
                    @"
", frontMatterContent);
                Assert.AreEqual(
                    @"FM1
FM2
---
Content1
Content2", await documents.First().GetStringAsync());
            }

            [Test]
            public async Task EmptyFirstLineWithoutDelimiterTreatsAsFrontMatter()
            {
                // Given
                IExecutionContext context = new TestExecutionContext();
                IDocument[] inputs =
                {
                    new TestDocument(@"
FM1
FM2
---
Content1
Content2")
                };
                string frontMatterContent = null;
                FrontMatter frontMatter = new FrontMatter(new ExecuteDocument(Config.FromDocument(async x =>
                {
                    frontMatterContent = await x.GetStringAsync();
                    return new[] { x };
                })));

                // When
                IEnumerable<IDocument> documents = await frontMatter.ExecuteAsync(inputs, context);

                // Then
                Assert.AreEqual(1, documents.Count());
                Assert.AreEqual(
                    @"
FM1
FM2
", frontMatterContent);
                Assert.AreEqual(
                    @"Content1
Content2", await documents.First().GetStringAsync());
            }

            [Test]
            public async Task DashStringDoesNotSplitAtNonmatchingDashes()
            {
                // Given
                IExecutionContext context = new TestExecutionContext();
                IDocument[] inputs =
                {
                    new TestDocument(@"FM1
FM2
---
Content1
Content2")
                };
                bool executed = false;
                FrontMatter frontMatter = new FrontMatter("-", new ExecuteDocument(Config.FromDocument(x =>
                {
                    executed = true;
                    return new[] { x };
                })));

                // When
                IEnumerable<IDocument> documents = await frontMatter.ExecuteAsync(inputs, context);

                // Then
                Assert.AreEqual(1, documents.Count());
                Assert.IsFalse(executed);
                Assert.AreEqual(
                    @"FM1
FM2
---
Content1
Content2", await documents.First().GetStringAsync());
            }

            [Test]
            public async Task MatchingStringSplitsAtCorrectLocation()
            {
                // Given
                IExecutionContext context = new TestExecutionContext();
                IDocument[] inputs =
                {
                    new TestDocument(@"FM1
FM2
ABC
Content1
Content2")
                };
                string frontMatterContent = null;
                FrontMatter frontMatter = new FrontMatter("ABC", new ExecuteDocument(Config.FromDocument(async x =>
                {
                    frontMatterContent = await x.GetStringAsync();
                    return new[] { x };
                })));

                // When
                IEnumerable<IDocument> documents = await frontMatter.ExecuteAsync(inputs, context);

                // Then
                Assert.AreEqual(1, documents.Count());
                Assert.AreEqual(
                    @"FM1
FM2
", frontMatterContent);
                Assert.AreEqual(
                    @"Content1
Content2", await documents.First().GetStringAsync());
            }

            [Test]
            public async Task SingleCharWithRepeatedDelimiterSplitsAtCorrectLocation()
            {
                // Given
                IExecutionContext context = new TestExecutionContext();
                IDocument[] inputs =
                {
                    new TestDocument(@"FM1
FM2
!!!!
Content1
Content2")
                };
                string frontMatterContent = null;
                FrontMatter frontMatter = new FrontMatter('!', new ExecuteDocument(Config.FromDocument(async x =>
                {
                    frontMatterContent = await x.GetStringAsync();
                    return new[] { x };
                })));

                // When
                IEnumerable<IDocument> documents = await frontMatter.ExecuteAsync(inputs, context);

                // Then
                Assert.AreEqual(1, documents.Count());
                Assert.AreEqual(
                    @"FM1
FM2
", frontMatterContent);
                Assert.AreEqual(
                    @"Content1
Content2", await documents.First().GetStringAsync());
            }

            [Test]
            public async Task SingleCharWithRepeatedDelimiterWithTrailingSpacesSplitsAtCorrectLocation()
            {
                // Given
                IExecutionContext context = new TestExecutionContext();
                IDocument[] inputs =
                {
                    new TestDocument(@"FM1
FM2
!!!!  
Content1
Content2")
                };
                string frontMatterContent = null;
                FrontMatter frontMatter = new FrontMatter('!', new ExecuteDocument(Config.FromDocument(async x =>
                {
                    frontMatterContent = await x.GetStringAsync();
                    return new[] { x };
                })));

                // When
                IEnumerable<IDocument> documents = await frontMatter.ExecuteAsync(inputs, context);

                // Then
                Assert.AreEqual(1, documents.Count());
                Assert.AreEqual(
                    @"FM1
FM2
", frontMatterContent);
                Assert.AreEqual(
                    @"Content1
Content2", await documents.First().GetStringAsync());
            }

            [Test]
            public async Task SingleCharWithRepeatedDelimiterWithLeadingSpacesDoesNotSplit()
            {
                // Given
                IExecutionContext context = new TestExecutionContext();
                IDocument[] inputs =
                {
                    new TestDocument(@"FM1
FM2
  !!!!
Content1
Content2")
                };
                bool executed = false;
                FrontMatter frontMatter = new FrontMatter('!', new ExecuteDocument(Config.FromDocument(x =>
                {
                    executed = true;
                    return new[] { x };
                })));

                // When
                IEnumerable<IDocument> documents = await frontMatter.ExecuteAsync(inputs, context);

                // Then
                Assert.AreEqual(1, documents.Count());
                Assert.IsFalse(executed);
                Assert.AreEqual(
                    @"FM1
FM2
  !!!!
Content1
Content2", await documents.First().GetStringAsync());
            }

            [Test]
            public async Task SingleCharWithRepeatedDelimiterWithExtraLinesSplitsAtCorrectLocation()
            {
                // Given
                IExecutionContext context = new TestExecutionContext();
                IDocument[] inputs =
                {
                     new TestDocument(@"FM1
FM2

!!!!

Content1
Content2")
                };
                string frontMatterContent = null;
                FrontMatter frontMatter = new FrontMatter('!', new ExecuteDocument(Config.FromDocument(async x =>
                {
                    frontMatterContent = await x.GetStringAsync();
                    return new[] { x };
                })));

                // When
                IEnumerable<IDocument> documents = await frontMatter.ExecuteAsync(inputs, context);

                // Then
                Assert.AreEqual(1, documents.Count());
                Assert.AreEqual(
                    @"FM1
FM2

", frontMatterContent);
                Assert.AreEqual(
                    @"
Content1
Content2", await documents.First().GetStringAsync());
            }

            [Test]
            public async Task SingleCharWithSingleDelimiterSplitsAtCorrectLocation()
            {
                // Given
                IExecutionContext context = new TestExecutionContext();
                IDocument[] inputs =
                {
                    new TestDocument(@"FM1
FM2
!
Content1
Content2")
                };
                string frontMatterContent = null;
                FrontMatter frontMatter = new FrontMatter('!', new ExecuteDocument(Config.FromDocument(async x =>
                {
                    frontMatterContent = await x.GetStringAsync();
                    return new[] { x };
                })));

                // When
                IEnumerable<IDocument> documents = await frontMatter.ExecuteAsync(inputs, context);

                // Then
                Assert.AreEqual(1, documents.Count());
                Assert.AreEqual(
                    @"FM1
FM2
", frontMatterContent);
                Assert.AreEqual(
                    @"Content1
Content2", await documents.First().GetStringAsync());
            }

            [Test]
            public async Task MultipleInputDocumentsResultsInMultipleOutputs()
            {
                // Given
                IExecutionContext context = new TestExecutionContext();
                IDocument[] inputs =
                {
                    new TestDocument(@"AA
-
XX"),
                    new TestDocument(@"BB
-
YY")
                };
                string frontMatterContent = string.Empty;
                FrontMatter frontMatter = new FrontMatter(new ExecuteDocument(Config.FromDocument(async x =>
                {
                    frontMatterContent += await x.GetStringAsync();
                    return new[] { x };
                })));

                // When
                IEnumerable<IDocument> documents = await frontMatter.ExecuteAsync(inputs, context);

                // Then
                Assert.AreEqual(2, documents.Count());
                Assert.AreEqual(
                    @"AA
BB
", frontMatterContent);
                Assert.AreEqual("XX", await documents.First().GetStringAsync());
                Assert.AreEqual("YY", await documents.Skip(1).First().GetStringAsync());
            }

            [Test]
            public async Task DefaultCtorIgnoresDelimiterOnFirstLine()
            {
                // Given
                IExecutionContext context = new TestExecutionContext();
                IDocument[] inputs =
                {
                    new TestDocument(@"---
FM1
FM2
---
Content1
Content2")
                };
                string frontMatterContent = null;
                FrontMatter frontMatter = new FrontMatter(new ExecuteDocument(Config.FromDocument(async x =>
                {
                    frontMatterContent = await x.GetStringAsync();
                    return new[] { x };
                })));

                // When
                IEnumerable<IDocument> documents = await frontMatter.ExecuteAsync(inputs, context);

                // Then
                Assert.AreEqual(1, documents.Count());
                Assert.AreEqual(
                    @"FM1
FM2
", frontMatterContent);
                Assert.AreEqual(
                    @"Content1
Content2", await documents.First().GetStringAsync());
            }

            [Test]
            public async Task NoIgnoreDelimiterOnFirstLine()
            {
                // Given
                IExecutionContext context = new TestExecutionContext();
                IDocument[] inputs =
                {
                    new TestDocument(@"---
FM1
FM2
---
Content1
Content2")
                };
                string frontMatterContent = null;
                FrontMatter frontMatter = new FrontMatter(new ExecuteDocument(Config.FromDocument(async x =>
                {
                    frontMatterContent = await x.GetStringAsync();
                    return new[] { x };
                }))).IgnoreDelimiterOnFirstLine(false);

                // When
                IEnumerable<IDocument> documents = await frontMatter.ExecuteAsync(inputs, context);

                // Then
                Assert.AreEqual(1, documents.Count());
                Assert.AreEqual("\n", frontMatterContent);
                Assert.AreEqual(
                    @"FM1
FM2
---
Content1
Content2", await documents.First().GetStringAsync());
            }
        }
    }
}
