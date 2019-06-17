using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Statiq.Common.Configuration;
using Statiq.Common.Documents;
using Statiq.Common.Execution;
using Statiq.Core.Modules.Control;
using Statiq.Core.Modules.Extensibility;
using Statiq.Testing;
using Statiq.Testing.Documents;
using Statiq.Testing.Execution;

namespace Statiq.Core.Tests.Modules.Control
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
                TestExecutionContext context = new TestExecutionContext();
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
                TestExecutionContext context = new TestExecutionContext();
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
                TestExecutionContext context = new TestExecutionContext();
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
                TestExecutionContext context = new TestExecutionContext();
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
                TestExecutionContext context = new TestExecutionContext();
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
                TestExecutionContext context = new TestExecutionContext();
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
                TestExecutionContext context = new TestExecutionContext();
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
                TestExecutionContext context = new TestExecutionContext();
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
                TestExecutionContext context = new TestExecutionContext();
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
                TestExecutionContext context = new TestExecutionContext();
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
                TestExecutionContext context = new TestExecutionContext();
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
                TestExecutionContext context = new TestExecutionContext();
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
                TestExecutionContext context = new TestExecutionContext();
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
