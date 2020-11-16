using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Shouldly;
using Statiq.Common;
using Statiq.Testing;

namespace Statiq.Core.Tests.Modules.Control
{
    [TestFixture]
    public class ExtractFrontMatterFixture : BaseFixture
    {
        public class ExecuteTests : ExtractFrontMatterFixture
        {
            [Test]
            public async Task DefaultCtorSplitsAtDashes()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                TestDocument[] inputs =
                {
                    new TestDocument(@"FM1
FM2
---
Content1
Content2")
                };
                string frontMatterContent = null;
                ExtractFrontMatter frontMatter = new ExtractFrontMatter(new ExecuteConfig(Config.FromDocument(async x =>
                {
                    frontMatterContent = await x.GetContentStringAsync();
                    return new[] { x };
                })));

                // When
                IEnumerable<IDocument> documents = await ExecuteAsync(inputs, context, frontMatter);

                // Then
                documents.Count().ShouldBe(1);
                frontMatterContent.ShouldBe(@"FM1
FM2
");
                (await documents.First().GetContentStringAsync()).ShouldBe(@"Content1
Content2");
            }

            [Test]
            public async Task EmptyFirstLineWithDelimiterTreatsAsFrontMatter()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                TestDocument[] inputs =
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
                ExtractFrontMatter frontMatter = new ExtractFrontMatter(new ExecuteConfig(Config.FromDocument(async x =>
                {
                    frontMatterContent = await x.GetContentStringAsync();
                    return new[] { x };
                })));

                // When
                IEnumerable<IDocument> documents = await ExecuteAsync(inputs, context, frontMatter);

                // Then
                documents.Count().ShouldBe(1);
                frontMatterContent.ShouldBe(
                    @"
");
                (await documents.First().GetContentStringAsync()).ShouldBe(
                    @"FM1
FM2
---
Content1
Content2");
            }

            [Test]
            public async Task EmptyFirstLineWithoutDelimiterTreatsAsFrontMatter()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                TestDocument[] inputs =
                {
                    new TestDocument(@"
FM1
FM2
---
Content1
Content2")
                };
                string frontMatterContent = null;
                ExtractFrontMatter frontMatter = new ExtractFrontMatter(new ExecuteConfig(Config.FromDocument(async x =>
                {
                    frontMatterContent = await x.GetContentStringAsync();
                    return new[] { x };
                })));

                // When
                IEnumerable<IDocument> documents = await ExecuteAsync(inputs, context, frontMatter);

                // Then
                documents.Count().ShouldBe(1);
                frontMatterContent.ShouldBe(
                    @"
FM1
FM2
");
                (await documents.First().GetContentStringAsync()).ShouldBe(
                    @"Content1
Content2");
            }

            [Test]
            public async Task DashStringDoesNotSplitAtNonmatchingDashes()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                TestDocument[] inputs =
                {
                    new TestDocument(@"FM1
FM2
---
Content1
Content2")
                };
                bool executed = false;
                ExtractFrontMatter frontMatter = new ExtractFrontMatter("-", new ExecuteConfig(Config.FromDocument(x =>
                {
                    executed = true;
                    return new[] { x };
                })));

                // When
                IEnumerable<IDocument> documents = await ExecuteAsync(inputs, context, frontMatter);

                // Then
                documents.Count().ShouldBe(1);
                executed.ShouldBeFalse();
                (await documents.First().GetContentStringAsync()).ShouldBe(
                    @"FM1
FM2
---
Content1
Content2");
            }

            [Test]
            public async Task MatchingStringSplitsAtCorrectLocation()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                TestDocument[] inputs =
                {
                    new TestDocument(@"FM1
FM2
ABC
Content1
Content2")
                };
                string frontMatterContent = null;
                ExtractFrontMatter frontMatter = new ExtractFrontMatter("ABC", new ExecuteConfig(Config.FromDocument(async x =>
                {
                    frontMatterContent = await x.GetContentStringAsync();
                    return new[] { x };
                })));

                // When
                IEnumerable<IDocument> documents = await ExecuteAsync(inputs, context, frontMatter);

                // Then
                documents.Count().ShouldBe(1);
                frontMatterContent.ShouldBe(
                    @"FM1
FM2
");
                (await documents.First().GetContentStringAsync()).ShouldBe(
                    @"Content1
Content2");
            }

            [Test]
            public async Task SingleCharWithRepeatedDelimiterSplitsAtCorrectLocation()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                TestDocument[] inputs =
                {
                    new TestDocument(@"FM1
FM2
!!!!
Content1
Content2")
                };
                string frontMatterContent = null;
                ExtractFrontMatter frontMatter = new ExtractFrontMatter('!', new ExecuteConfig(Config.FromDocument(async x =>
                {
                    frontMatterContent = await x.GetContentStringAsync();
                    return new[] { x };
                })));

                // When
                IEnumerable<IDocument> documents = await ExecuteAsync(inputs, context, frontMatter);

                // Then
                documents.Count().ShouldBe(1);
                frontMatterContent.ShouldBe(
                    @"FM1
FM2
");
                (await documents.First().GetContentStringAsync()).ShouldBe(
                    @"Content1
Content2");
            }

            [Test]
            public async Task SingleCharWithRepeatedDelimiterWithTrailingSpacesSplitsAtCorrectLocation()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                TestDocument[] inputs =
                {
                    new TestDocument(@"FM1
FM2
!!!!  
Content1
Content2")
                };
                string frontMatterContent = null;
                ExtractFrontMatter frontMatter = new ExtractFrontMatter('!', new ExecuteConfig(Config.FromDocument(async x =>
                {
                    frontMatterContent = await x.GetContentStringAsync();
                    return new[] { x };
                })));

                // When
                IEnumerable<IDocument> documents = await ExecuteAsync(inputs, context, frontMatter);

                // Then
                documents.Count().ShouldBe(1);
                frontMatterContent.ShouldBe(
                    @"FM1
FM2
");
                (await documents.First().GetContentStringAsync()).ShouldBe(
                    @"Content1
Content2");
            }

            [Test]
            public async Task SingleCharWithRepeatedDelimiterWithLeadingSpacesDoesNotSplit()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                TestDocument[] inputs =
                {
                    new TestDocument(@"FM1
FM2
  !!!!
Content1
Content2")
                };
                bool executed = false;
                ExtractFrontMatter frontMatter = new ExtractFrontMatter('!', new ExecuteConfig(Config.FromDocument(x =>
                {
                    executed = true;
                    return new[] { x };
                })));

                // When
                IEnumerable<IDocument> documents = await ExecuteAsync(inputs, context, frontMatter);

                // Then
                documents.Count().ShouldBe(1);
                executed.ShouldBeFalse();
                (await documents.First().GetContentStringAsync()).ShouldBe(
                    @"FM1
FM2
  !!!!
Content1
Content2");
            }

            [Test]
            public async Task SingleCharWithRepeatedDelimiterWithExtraLinesSplitsAtCorrectLocation()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                TestDocument[] inputs =
                {
                     new TestDocument(@"FM1
FM2

!!!!

Content1
Content2")
                };
                string frontMatterContent = null;
                ExtractFrontMatter frontMatter = new ExtractFrontMatter('!', new ExecuteConfig(Config.FromDocument(async x =>
                {
                    frontMatterContent = await x.GetContentStringAsync();
                    return new[] { x };
                })));

                // When
                IEnumerable<IDocument> documents = await ExecuteAsync(inputs, context, frontMatter);

                // Then
                documents.Count().ShouldBe(1);
                frontMatterContent.ShouldBe(
                    @"FM1
FM2

");
                (await documents.First().GetContentStringAsync()).ShouldBe(
                    @"
Content1
Content2");
            }

            [Test]
            public async Task SingleCharWithSingleDelimiterSplitsAtCorrectLocation()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                TestDocument[] inputs =
                {
                    new TestDocument(@"FM1
FM2
!
Content1
Content2")
                };
                string frontMatterContent = null;
                ExtractFrontMatter frontMatter = new ExtractFrontMatter('!', new ExecuteConfig(Config.FromDocument(async x =>
                {
                    frontMatterContent = await x.GetContentStringAsync();
                    return new[] { x };
                })));

                // When
                IEnumerable<IDocument> documents = await ExecuteAsync(inputs, context, frontMatter);

                // Then
                documents.Count().ShouldBe(1);
                frontMatterContent.ShouldBe(
                    @"FM1
FM2
");
                (await documents.First().GetContentStringAsync()).ShouldBe(
                    @"Content1
Content2");
            }

            [Test]
            public async Task MultipleInputDocumentsResultsInMultipleOutputs()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                TestDocument[] inputs =
                {
                    new TestDocument(@"AA
-
XX"),
                    new TestDocument(@"BB
-
YY")
                };
                string frontMatterContent = string.Empty;
                ExtractFrontMatter frontMatter = new ExtractFrontMatter(new ExecuteConfig(Config.FromDocument(async x =>
                {
                    frontMatterContent += await x.GetContentStringAsync();
                    return new[] { x };
                })));

                // When
                IEnumerable<IDocument> documents = await ExecuteAsync(inputs, context, frontMatter);

                // Then
                documents.Count().ShouldBe(2);
                frontMatterContent.ShouldBe(
                    @"AA
BB
", frontMatterContent);
                (await documents.First().GetContentStringAsync()).ShouldBe("XX");
                (await documents.Skip(1).First().GetContentStringAsync()).ShouldBe("YY");
            }

            [Test]
            public async Task DefaultCtorIgnoresDelimiterOnFirstLine()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                TestDocument[] inputs =
                {
                    new TestDocument(@"---
FM1
FM2
---
Content1
Content2")
                };
                string frontMatterContent = null;
                ExtractFrontMatter frontMatter = new ExtractFrontMatter(new ExecuteConfig(Config.FromDocument(async x =>
                {
                    frontMatterContent = await x.GetContentStringAsync();
                    return new[] { x };
                })));

                // When
                IEnumerable<IDocument> documents = await ExecuteAsync(inputs, context, frontMatter);

                // Then
                documents.Count().ShouldBe(1);
                frontMatterContent.ShouldBe(
                    @"FM1
FM2
", frontMatterContent);
                (await documents.First().GetContentStringAsync()).ShouldBe(
                    @"Content1
Content2");
            }

            [Test]
            public async Task NoIgnoreDelimiterOnFirstLine()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                TestDocument[] inputs =
                {
                    new TestDocument(@"---
FM1
FM2
---
Content1
Content2")
                };
                string frontMatterContent = null;
                ExtractFrontMatter frontMatter = new ExtractFrontMatter(new ExecuteConfig(Config.FromDocument(async x =>
                {
                    frontMatterContent = await x.GetContentStringAsync();
                    return new[] { x };
                }))).IgnoreDelimiterOnFirstLine(false);

                // When
                IEnumerable<IDocument> documents = await ExecuteAsync(inputs, context, frontMatter);

                // Then
                documents.Count().ShouldBe(1);
                frontMatterContent.ShouldBe("\n");
                (await documents.First().GetContentStringAsync()).ShouldBe(
                    @"FM1
FM2
---
Content1
Content2");
            }

            [Test]
            public async Task IgnoreSettingIgnoredWhenStartDelimiter()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                TestDocument[] inputs =
                {
                    new TestDocument(@"---
FM1
FM2
---
Content1
Content2")
                };
                bool executed = false;
                ExtractFrontMatter frontMatter = new ExtractFrontMatter(new ExecuteConfig(Config.FromDocument(x =>
                {
                    executed = true;
                    return new[] { x };
                })))
                    .IgnoreDelimiterOnFirstLine(true)
                    .RequireStartDelimiter('+');

                // When
                IEnumerable<IDocument> documents = await ExecuteAsync(inputs, context, frontMatter);

                // Then
                documents.Count().ShouldBe(1);
                executed.ShouldBeFalse();
                (await documents.First().GetContentStringAsync()).ShouldBe(
                    @"---
FM1
FM2
---
Content1
Content2");
            }

            [Test]
            public async Task StartDelimiterNotFound()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                TestDocument[] inputs =
                {
                    new TestDocument(@"---
FM1
FM2
---
Content1
Content2")
                };
                bool executed = false;
                ExtractFrontMatter frontMatter = new ExtractFrontMatter(new ExecuteConfig(Config.FromDocument(x =>
                {
                    executed = true;
                    return new[] { x };
                })))
                    .IgnoreDelimiterOnFirstLine(true)
                    .RequireStartDelimiter('+');

                // When
                IEnumerable<IDocument> documents = await ExecuteAsync(inputs, context, frontMatter);

                // Then
                documents.Count().ShouldBe(1);
                executed.ShouldBeFalse();
                (await documents.First().GetContentStringAsync()).ShouldBe(
                    @"---
FM1
FM2
---
Content1
Content2");
            }

            [Test]
            public async Task EmptyFirstLineWithStartDelimiterShouldNotMatch()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                TestDocument[] inputs =
                {
                    new TestDocument(@"
+++
FM1
FM2
---
Content1
Content2")
                };
                bool executed = false;
                ExtractFrontMatter frontMatter = new ExtractFrontMatter(new ExecuteConfig(Config.FromDocument(x =>
                {
                    executed = true;
                    return new[] { x };
                }))).RequireStartDelimiter('+');

                // When
                IEnumerable<IDocument> documents = await ExecuteAsync(inputs, context, frontMatter);

                // Then
                documents.Count().ShouldBe(1);
                executed.ShouldBeFalse();
                (await documents.First().GetContentStringAsync()).ShouldBe(
                    @"
+++
FM1
FM2
---
Content1
Content2");
            }

            [Test]
            public async Task NoEndDelimiter()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                TestDocument[] inputs =
                {
                    new TestDocument(@"+++
FM1
FM2
+++
Content1
Content2")
                };
                bool executed = false;
                ExtractFrontMatter frontMatter = new ExtractFrontMatter(new ExecuteConfig(Config.FromDocument(x =>
                {
                    executed = true;
                    return new[] { x };
                }))).RequireStartDelimiter('+');

                // When
                IEnumerable<IDocument> documents = await ExecuteAsync(inputs, context, frontMatter);

                // Then
                documents.Count().ShouldBe(1);
                executed.ShouldBeFalse();
                (await documents.First().GetContentStringAsync()).ShouldBe(
                    @"+++
FM1
FM2
+++
Content1
Content2");
            }

            [Test]
            public async Task StartDelimiterAsStringShouldMatch()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                TestDocument[] inputs =
                {
                    new TestDocument(@"123
FM1
FM2
---
Content1
Content2")
                };
                string frontMatterContent = null;
                ExtractFrontMatter frontMatter = new ExtractFrontMatter(new ExecuteConfig(Config.FromDocument(async x =>
                {
                    frontMatterContent = await x.GetContentStringAsync();
                    return new[] { x };
                }))).RequireStartDelimiter("123");

                // When
                IEnumerable<IDocument> documents = await ExecuteAsync(inputs, context, frontMatter);

                // Then
                documents.Count().ShouldBe(1);
                frontMatterContent.ShouldBe(
                    @"FM1
FM2
");
                (await documents.First().GetContentStringAsync()).ShouldBe(
                    @"Content1
Content2");
            }

            [Test]
            public async Task StartDelimiterAsRepeatedStringShouldNotMatch()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                TestDocument[] inputs =
                {
                    new TestDocument(@"123123
FM1
FM2
---
Content1
Content2")
                };
                bool executed = false;
                ExtractFrontMatter frontMatter = new ExtractFrontMatter(new ExecuteConfig(Config.FromDocument(x =>
                {
                    executed = true;
                    return new[] { x };
                }))).RequireStartDelimiter("123");

                // When
                IEnumerable<IDocument> documents = await ExecuteAsync(inputs, context, frontMatter);

                // Then
                documents.Count().ShouldBe(1);
                executed.ShouldBeFalse();
                (await documents.First().GetContentStringAsync()).ShouldBe(
                    @"123123
FM1
FM2
---
Content1
Content2");
            }

            [Test]
            public async Task StartDelimiterAsCharShouldMatch()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                TestDocument[] inputs =
                {
                    new TestDocument(@"+
FM1
FM2
---
Content1
Content2")
                };
                string frontMatterContent = null;
                ExtractFrontMatter frontMatter = new ExtractFrontMatter(new ExecuteConfig(Config.FromDocument(async x =>
                {
                    frontMatterContent = await x.GetContentStringAsync();
                    return new[] { x };
                }))).RequireStartDelimiter('+');

                // When
                IEnumerable<IDocument> documents = await ExecuteAsync(inputs, context, frontMatter);

                // Then
                documents.Count().ShouldBe(1);
                frontMatterContent.ShouldBe(
                    @"FM1
FM2
");
                (await documents.First().GetContentStringAsync()).ShouldBe(
                    @"Content1
Content2");
            }

            [Test]
            public async Task StartDelimiterAsRepeatedCharShouldMatch()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                TestDocument[] inputs =
                {
                    new TestDocument(@"+++
FM1
FM2
---
Content1
Content2")
                };
                string frontMatterContent = null;
                ExtractFrontMatter frontMatter = new ExtractFrontMatter(new ExecuteConfig(Config.FromDocument(async x =>
                {
                    frontMatterContent = await x.GetContentStringAsync();
                    return new[] { x };
                }))).RequireStartDelimiter('+');

                // When
                IEnumerable<IDocument> documents = await ExecuteAsync(inputs, context, frontMatter);

                // Then
                documents.Count().ShouldBe(1);
                frontMatterContent.ShouldBe(
                    @"FM1
FM2
");
                (await documents.First().GetContentStringAsync()).ShouldBe(
                    @"Content1
Content2");
            }

            [Test]
            public async Task StartDelimiterAsRepeatedCharWithExtraCharsShouldNotMatch()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                TestDocument[] inputs =
                {
                    new TestDocument(@"+++---
FM1
FM2
---
Content1
Content2")
                };
                bool executed = false;
                ExtractFrontMatter frontMatter = new ExtractFrontMatter(new ExecuteConfig(Config.FromDocument(x =>
                {
                    executed = true;
                    return new[] { x };
                }))).RequireStartDelimiter('+');

                // When
                IEnumerable<IDocument> documents = await ExecuteAsync(inputs, context, frontMatter);

                // Then
                documents.Count().ShouldBe(1);
                executed.ShouldBeFalse();
                (await documents.First().GetContentStringAsync()).ShouldBe(
                    @"+++---
FM1
FM2
---
Content1
Content2");
            }

            [Test]
            public async Task PreservesFrontMatter()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                TestDocument[] inputs =
                {
                    new TestDocument(@"FM1
FM2
---
Content1
Content2")
                };
                string frontMatterContent = null;
                ExtractFrontMatter frontMatter = new ExtractFrontMatter(new ExecuteConfig(Config.FromDocument(async x =>
                {
                    frontMatterContent = await x.GetContentStringAsync();
                    return new[] { x };
                }))).PreserveFrontMatter();

                // When
                IEnumerable<IDocument> documents = await ExecuteAsync(inputs, context, frontMatter);

                // Then
                documents.Count().ShouldBe(1);
                frontMatterContent.ShouldBe(
                    @"FM1
FM2
", frontMatterContent);
                (await documents.First().GetContentStringAsync()).ShouldBe(
                    @"FM1
FM2
---
Content1
Content2");
            }
        }
    }
}
