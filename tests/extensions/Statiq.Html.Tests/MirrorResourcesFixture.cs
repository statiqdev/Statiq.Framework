using System.Threading.Tasks;
using NUnit.Framework;
using Shouldly;
using Statiq.Common;
using Statiq.Testing;

namespace Statiq.Html.Tests
{
    [TestFixture]
    public class MirrorResourcesFixture : BaseFixture
    {
        public class ExecuteTests : MirrorResourcesFixture
        {
            [Test]
            public async Task ReplacesScriptResource()
            {
                // Given
                TestDocument document = new TestDocument(
                    @"<html>
                      <head>
                        <script src=""https://cdn.jsdelivr.net/npm/es6-promise/dist/es6-promise.auto.min.js""></script>
                      </head>
                      <body>
                      </body>
                    </html>");
                MirrorResources module = new MirrorResources();

                // When
                TestDocument result = await ExecuteAsync(document, module).SingleAsync();

                // Then
                result.Content.ShouldBe(
                    @"<html><head>
                        <script src=""/mirror/cdn.jsdelivr.net/npm/es6-promise/dist/es6-promise.auto.min.js""></script>
                      </head>
                      <body>
                      
                    </body></html>", StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public async Task ReplacesLinkResource()
            {
                // Given
                TestDocument document = new TestDocument(
                    @"<html>
                      <head>
                        <link rel=""stylesheet"" href=""https://cdn.jsdelivr.net/npm/@@progress/kendo-theme-bootstrap@3.2.0/dist/all.min.css"" />
                      </head>
                      <body>
                      </body>
                    </html>");
                MirrorResources module = new MirrorResources();

                // When
                TestDocument result = await ExecuteAsync(document, module).SingleAsync();

                // Then
                result.Content.ShouldBe(
                    @"<html><head>
                        <link rel=""stylesheet"" href=""/mirror/cdn.jsdelivr.net/npm/@@progress/kendo-theme-bootstrap@3.2.0/dist/all.min.css"">
                      </head>
                      <body>
                      
                    </body></html>", StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public async Task DoesNotReplaceRelativePaths()
            {
                // Given
                TestDocument document = new TestDocument(
                    @"<html>
                      <head>
                        <link rel=""apple-touch-icon"" sizes=""120x120"" href=""/apple-touch-icon.png"">
                      </head>
                      <body>
                      </body>
                    </html>");
                MirrorResources module = new MirrorResources();

                // When
                TestDocument result = await ExecuteAsync(document, module).SingleAsync();

                // Then
                result.Content.ShouldBe(
                    @"<html>
                      <head>
                        <link rel=""apple-touch-icon"" sizes=""120x120"" href=""/apple-touch-icon.png"">
                      </head>
                      <body>
                      </body>
                    </html>", StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public async Task DoesNotReplaceDataNoMirrorAttribute()
            {
                // Given
                TestDocument document = new TestDocument(
                    @"<html>
                      <head>
                        <script src=""https://cdn.jsdelivr.net/npm/es6-promise/dist/es6-promise.auto.min.js"" data-no-mirror></script>
                        <link rel=""stylesheet"" href=""https://cdn.jsdelivr.net/npm/@@progress/kendo-theme-bootstrap@3.2.0/dist/all.min.css"" data-no-mirror />
                      </head>
                      <body>
                      </body>
                    </html>");
                MirrorResources module = new MirrorResources();

                // When
                TestDocument result = await ExecuteAsync(document, module).SingleAsync();

                // Then
                result.Content.ShouldBe(
                    @"<html>
                      <head>
                        <script src=""https://cdn.jsdelivr.net/npm/es6-promise/dist/es6-promise.auto.min.js"" data-no-mirror></script>
                        <link rel=""stylesheet"" href=""https://cdn.jsdelivr.net/npm/@@progress/kendo-theme-bootstrap@3.2.0/dist/all.min.css"" data-no-mirror />
                      </head>
                      <body>
                      </body>
                    </html>", StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public async Task UsesCustomMirrorPath()
            {
                // Given
                TestDocument document = new TestDocument(
                    @"<html>
                      <head>
                        <script src=""https://cdn.jsdelivr.net/npm/es6-promise/dist/es6-promise.auto.min.js""></script>
                      </head>
                      <body>
                      </body>
                    </html>");
                MirrorResources module = new MirrorResources(_ => new NormalizedPath("/foo/bar.js"));

                // When
                TestDocument result = await ExecuteAsync(document, module).SingleAsync();

                // Then
                result.Content.ShouldBe(
                    @"<html><head>
                        <script src=""/foo/bar.js""></script>
                      </head>
                      <body>
                      
                    </body></html>", StringCompareShould.IgnoreLineEndings);
            }
        }
    }
}
