using System;
using System.Threading.Tasks;
using NUnit.Framework;
using Shouldly;
using Statiq.Common;
using Statiq.Testing;

namespace Statiq.Sass.Tests
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
                TestExecutionContext context = new TestExecutionContext
                {
                    FileSystem = fileSystem
                };
                TestDocument document = new TestDocument(
                    new FilePath("/input/assets/test.scss"),
                    new FilePath("assets/test.scss"),
                    input);

                Sass sass = new Sass().WithCompactOutputStyle();

                // When
                TestDocument result = await ExecuteAsync(document, context, sass).SingleAsync();

                // Then
                result.Content.ShouldBe(output);
                result.Destination.FullPath.ShouldBe("assets/test.css");
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
                TestExecutionContext context = new TestExecutionContext
                {
                    FileSystem = fileSystem
                };
                TestDocument document = new TestDocument(
                    new FilePath("/input/assets/test.scss"),
                    new FilePath("assets/test.scss"),
                    input);

                Sass sass = new Sass();

                // When
                TestDocument result = await ExecuteAsync(document, context, sass).SingleAsync();

                // Then
                result.Content.ShouldBeEmpty();
                result.Destination.FullPath.ShouldBe("assets/test.css");
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
                TestExecutionContext context = new TestExecutionContext
                {
                    FileSystem = fileSystem
                };
                TestDocument document = new TestDocument(
                    new FilePath("/input/assets/test.scss"),
                    new FilePath("assets/test.scss"),
                    input);

                Sass sass = new Sass();

                // When, Then
                await Should.ThrowAsync<Exception>(async () => await ExecuteAsync(document, context, sass));
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
                TestExecutionContext context = new TestExecutionContext
                {
                    FileSystem = fileSystem
                };
                TestDocument document = new TestDocument(
                    new FilePath("/input/assets/test.scss"),
                    new FilePath("assets/test.scss"),
                    input);

                Sass sass = new Sass().IncludeSourceComments(false).WithCompactOutputStyle();

                // When
                TestDocument result = await ExecuteAsync(document, context, sass).SingleAsync();

                // Then
                result.Content.ShouldBe(output);
                result.Destination.FullPath.ShouldBe("assets/test.css");
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
                TestExecutionContext context = new TestExecutionContext
                {
                    FileSystem = fileSystem
                };
                TestDocument document = new TestDocument(
                    new FilePath("/input/assets/test.scss"),
                    new FilePath("assets/test.scss"),
                    input);

                Sass sass = new Sass().IncludeSourceComments(false).WithCompactOutputStyle();

                // When
                TestDocument result = await ExecuteAsync(document, context, sass).SingleAsync();

                // Then
                result.Content.ShouldBe(output);
                result.Destination.FullPath.ShouldBe("assets/test.css");
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
                TestExecutionContext context = new TestExecutionContext
                {
                    FileSystem = fileSystem
                };
                TestDocument document = new TestDocument(
                    new FilePath("/input/assets/test.scss"),
                    new FilePath("assets/test.scss"),
                    input);

                Sass sass = new Sass().IncludeSourceComments(false).WithCompactOutputStyle();

                // When
                TestDocument result = await ExecuteAsync(document, context, sass).SingleAsync();

                // Then
                result.Content.ShouldBe(output);
                result.Destination.FullPath.ShouldBe("assets/test.css");
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
                TestExecutionContext context = new TestExecutionContext
                {
                    FileSystem = fileSystem
                };
                TestDocument document = new TestDocument(
                    new FilePath("/input/assets/test.scss"),
                    new FilePath("assets/test.scss"),
                    input);

                Sass sass = new Sass().IncludeSourceComments(false).WithCompactOutputStyle();

                // When
                TestDocument result = await ExecuteAsync(document, context, sass).SingleAsync();

                // Then
                result.Content.ShouldBe(output);
                result.Destination.FullPath.ShouldBe("assets/test.css");
            }

            // TODO: Change above test to just use exact file name
            // TODO: Test include with missing extension
            // TODO: Test include with _ prefix
            // TODO: Test include with missing extension and _ prefix
        }
    }
}
