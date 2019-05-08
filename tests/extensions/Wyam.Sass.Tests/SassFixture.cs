using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Shouldly;
using Wyam.Common.Documents;
using Wyam.Common.Execution;
using Wyam.Common.IO;
using Wyam.Common.Meta;
using Wyam.Common.Util;
using Wyam.Testing;
using Wyam.Testing.Documents;
using Wyam.Testing.Execution;
using Wyam.Testing.IO;

namespace Wyam.Sass.Tests
{
    [TestFixture]
    [Parallelizable(ParallelScope.Self | ParallelScope.Children)]
    public class SassFixture : BaseFixture
    {
        public class ExecuteTests : SassFixture
        {
            [Test]
            public async Task Convert()
            {
                // Given
                const string input = @"
$font-stack:    Helvetica, sans-serif;
$primary-color: #333;

body {
  font: 100% $font-stack;
  color: $primary-color;
}";

                const string output = "body { font: 100% Helvetica, sans-serif; color: #333; }\n";

                TestFileProvider fileProvider = new TestFileProvider();
                fileProvider.AddDirectory("/");
                fileProvider.AddDirectory("/input");
                fileProvider.AddDirectory("/input/assets");
                fileProvider.AddFile("/input/assets/test.scss", input);
                TestFileSystem fileSystem = new TestFileSystem
                {
                    FileProvider = fileProvider
                };
                IExecutionContext context = new TestExecutionContext
                {
                    FileSystem = fileSystem
                };
                IDocument document = new TestDocument(
                    new MetadataItems
                    {
                        { Keys.RelativeFilePath, new FilePath("assets/test.scss") }
                    },
                    input);

                Sass sass = new Sass().WithCompactOutputStyle();

                // When
                List<IDocument> results = await sass.ExecuteAsync(new[] { document }, context).ToListAsync(); // Make sure to materialize the result list

                // Then
                Assert.That(await results.SelectAsync(async x => await x.GetStringAsync()), Is.EqualTo(new[] { output }));
                Assert.That(results.Select(x => x.FilePath(Keys.RelativeFilePath).FullPath), Is.EqualTo(new[] { "assets/test.css" }));
            }

            [Test]
            public async Task EmptyOutputForEmptyContent()
            {
                // Given
                string input = string.Empty;

                TestFileProvider fileProvider = new TestFileProvider();
                fileProvider.AddDirectory("/");
                fileProvider.AddDirectory("/input");
                fileProvider.AddDirectory("/input/assets");
                fileProvider.AddFile("/input/assets/test.scss", input);
                TestFileSystem fileSystem = new TestFileSystem
                {
                    FileProvider = fileProvider
                };
                IExecutionContext context = new TestExecutionContext
                {
                    FileSystem = fileSystem
                };
                IDocument document = new TestDocument(
                    new MetadataItems
                    {
                        { Keys.RelativeFilePath, new FilePath("assets/test.scss") }
                    },
                    input);

                Sass sass = new Sass();

                // When
                List<IDocument> results = await sass.ExecuteAsync(new[] { document }, context).ToListAsync(); // Make sure to materialize the result list

                // Then
                Assert.That(await results.SelectAsync(async x => await x.GetStringAsync()), Is.EqualTo(new[] { string.Empty }));
                Assert.That(results.Select(x => x.FilePath(Keys.RelativeFilePath).FullPath), Is.EqualTo(new[] { "assets/test.css" }));
            }

            [Test]
            public async Task ConvertingBadSassFails()
            {
                // Given
                const string input = @"
$font-stack:    Helvetica, sans-serif
$primary-color: #333

body {
  font: 100% $font-stack;
  color: $primary-color;
}";
                TestFileProvider fileProvider = new TestFileProvider();
                fileProvider.AddDirectory("/");
                fileProvider.AddDirectory("/input");
                fileProvider.AddDirectory("/input/assets");
                fileProvider.AddFile("/input/assets/test.scss", input);
                TestFileSystem fileSystem = new TestFileSystem
                {
                    FileProvider = fileProvider
                };
                IExecutionContext context = new TestExecutionContext
                {
                    FileSystem = fileSystem
                };
                IDocument document = new TestDocument(
                    new MetadataItems
                    {
                        { Keys.RelativeFilePath, new FilePath("assets/test.scss") }
                    },
                    input);

                Sass sass = new Sass();

                // When, Then
                await Should.ThrowAsync<Exception>(async () =>
                {
                    await sass.ExecuteAsync(new[] { document }, context).ToListAsync(); // Make sure to materialize the result list
                });
            }

            [Test]
            public async Task NestedImports()
            {
                // Given
                const string outerImport = @"
$font-stack:    Helvetica, sans-serif;";
                const string innerImport = @"
@import 'outer-import.scss';
$primary-color: #333;";
                const string input = @"
@import 'libs/_inner-import.scss';

body {
  font: 100% $font-stack;
  color: $primary-color;
}";
                const string output = "body { font: 100% Helvetica, sans-serif; color: #333; }\n";

                TestFileProvider fileProvider = new TestFileProvider();
                fileProvider.AddDirectory("/");
                fileProvider.AddDirectory("/input");
                fileProvider.AddDirectory("/input/assets");
                fileProvider.AddDirectory("/input/assets/libs");
                fileProvider.AddFile("/input/assets/test.scss", input);
                fileProvider.AddFile("/input/assets/libs/_outer-import.scss", outerImport);
                fileProvider.AddFile("/input/assets/libs/_inner-import.scss", innerImport);
                TestFileSystem fileSystem = new TestFileSystem
                {
                    FileProvider = fileProvider
                };
                IExecutionContext context = new TestExecutionContext
                {
                    FileSystem = fileSystem
                };
                IDocument document = new TestDocument(
                    new MetadataItems
                    {
                        { Keys.RelativeFilePath, new FilePath("assets/test.scss") }
                    },
                    input);

                Sass sass = new Sass().IncludeSourceComments(false).WithCompactOutputStyle();

                // When
                List<IDocument> results = await sass.ExecuteAsync(new[] { document }, context).ToListAsync(); // Make sure to materialize the result list

                // Then
                Assert.That(await results.SelectAsync(async x => await x.GetStringAsync()), Is.EqualTo(new[] { output }));
                Assert.That(results.Select(x => x.FilePath(Keys.RelativeFilePath).FullPath), Is.EqualTo(new[] { "assets/test.css" }));
            }

            [Test]
            public async Task ImportWithoutExtension()
            {
                // Given
                const string import = @"
$font-stack:    Helvetica, sans-serif;
$primary-color: #333;";
                const string input = @"
@import 'libs/_test-import';

body {
  font: 100% $font-stack;
  color: $primary-color;
}";
                const string output = "body { font: 100% Helvetica, sans-serif; color: #333; }\n";

                TestFileProvider fileProvider = new TestFileProvider();
                fileProvider.AddDirectory("/");
                fileProvider.AddDirectory("/input");
                fileProvider.AddDirectory("/input/assets");
                fileProvider.AddDirectory("/input/assets/libs");
                fileProvider.AddFile("/input/assets/test.scss", input);
                fileProvider.AddFile("/input/assets/libs/_test-import.scss", import);
                TestFileSystem fileSystem = new TestFileSystem
                {
                    FileProvider = fileProvider
                };
                IExecutionContext context = new TestExecutionContext
                {
                    FileSystem = fileSystem
                };
                IDocument document = new TestDocument(
                    new MetadataItems
                    {
                        { Keys.RelativeFilePath, new FilePath("assets/test.scss") }
                    },
                    input);

                Sass sass = new Sass().IncludeSourceComments(false).WithCompactOutputStyle();

                // When
                List<IDocument> results = await sass.ExecuteAsync(new[] { document }, context).ToListAsync(); // Make sure to materialize the result list

                // Then
                Assert.That(await results.SelectAsync(async x => await x.GetStringAsync()), Is.EqualTo(new[] { output }));
                Assert.That(results.Select(x => x.FilePath(Keys.RelativeFilePath).FullPath), Is.EqualTo(new[] { "assets/test.css" }));
            }

            [Test]
            public async Task ImportWithoutPrefix()
            {
                // Given
                const string import = @"
$font-stack:    Helvetica, sans-serif;
$primary-color: #333;";
                const string input = @"
@import 'libs/test-import.scss';

body {
  font: 100% $font-stack;
  color: $primary-color;
}";
                const string output = "body { font: 100% Helvetica, sans-serif; color: #333; }\n";

                TestFileProvider fileProvider = new TestFileProvider();
                fileProvider.AddDirectory("/");
                fileProvider.AddDirectory("/input");
                fileProvider.AddDirectory("/input/assets");
                fileProvider.AddDirectory("/input/assets/libs");
                fileProvider.AddFile("/input/assets/test.scss", input);
                fileProvider.AddFile("/input/assets/libs/_test-import.scss", import);
                TestFileSystem fileSystem = new TestFileSystem
                {
                    FileProvider = fileProvider
                };
                IExecutionContext context = new TestExecutionContext
                {
                    FileSystem = fileSystem
                };
                IDocument document = new TestDocument(
                    new MetadataItems
                    {
                        { Keys.RelativeFilePath, new FilePath("assets/test.scss") }
                    },
                    input);

                Sass sass = new Sass().IncludeSourceComments(false).WithCompactOutputStyle();

                // When
                List<IDocument> results = await sass.ExecuteAsync(new[] { document }, context).ToListAsync(); // Make sure to materialize the result list

                // Then
                Assert.That(await results.SelectAsync(async x => await x.GetStringAsync()), Is.EqualTo(new[] { output }));
                Assert.That(results.Select(x => x.FilePath(Keys.RelativeFilePath).FullPath), Is.EqualTo(new[] { "assets/test.css" }));
            }

            [Test]
            public async Task ImportWithoutPrefixOrExtension()
            {
                // Given
                const string import = @"
$font-stack:    Helvetica, sans-serif;
$primary-color: #333;";
                const string input = @"
@import 'libs/test-import';

body {
  font: 100% $font-stack;
  color: $primary-color;
}";
                const string output = "body { font: 100% Helvetica, sans-serif; color: #333; }\n";

                TestFileProvider fileProvider = new TestFileProvider();
                fileProvider.AddDirectory("/");
                fileProvider.AddDirectory("/input");
                fileProvider.AddDirectory("/input/assets");
                fileProvider.AddDirectory("/input/assets/libs");
                fileProvider.AddFile("/input/assets/test.scss", input);
                fileProvider.AddFile("/input/assets/libs/_test-import.scss", import);
                TestFileSystem fileSystem = new TestFileSystem
                {
                    FileProvider = fileProvider
                };
                IExecutionContext context = new TestExecutionContext
                {
                    FileSystem = fileSystem
                };
                IDocument document = new TestDocument(
                    new MetadataItems
                    {
                        { Keys.RelativeFilePath, new FilePath("assets/test.scss") }
                    },
                    input);

                Sass sass = new Sass().IncludeSourceComments(false).WithCompactOutputStyle();

                // When
                List<IDocument> results = await sass.ExecuteAsync(new[] { document }, context).ToListAsync(); // Make sure to materialize the result list

                // Then
                Assert.That(await results.SelectAsync(async x => await x.GetStringAsync()), Is.EqualTo(new[] { output }));
                Assert.That(results.Select(x => x.FilePath(Keys.RelativeFilePath).FullPath), Is.EqualTo(new[] { "assets/test.css" }));
            }

            // TODO: Change above test to just use exact file name
            // TODO: Test include with missing extension
            // TODO: Test include with _ prefix
            // TODO: Test include with missing extension and _ prefix
        }
    }
}
