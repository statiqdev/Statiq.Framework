using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Shouldly;
using Statiq.Common;
using Statiq.Core;
using Statiq.Testing;

namespace Statiq.Razor.Tests
{
    [TestFixture]
    public class RenderRazorFixture : BaseFixture
    {
        public class ExecuteTests : RenderRazorFixture
        {
            [Test]
            public async Task SimpleTemplate()
            {
                // Given
                Engine engine = new Engine();
                TestExecutionContext context = GetExecutionContext(engine);
                TestDocument document = new TestDocument("@for(int c = 0 ; c < 5 ; c++) { <p>@c</p> }");
                RenderRazor razor = new RenderRazor();

                // When
                TestDocument result = await ExecuteAsync(document, context, razor).SingleAsync();

                // Then
                result.Content.ShouldBe(" <p>0</p>  <p>1</p>  <p>2</p>  <p>3</p>  <p>4</p> ");
            }

            [Test]
            public async Task SimpleTemplateWithPreregisteredServices()
            {
                // Given
                Engine engine = new Engine();
                TestExecutionContext context = GetExecutionContext(engine);
                context.Services.AddRazor(null, context.ClassCatalog);
                TestDocument document = new TestDocument("@for(int c = 0 ; c < 5 ; c++) { <p>@c</p> }");
                RenderRazor razor = new RenderRazor();

                // When
                TestDocument result = await ExecuteAsync(document, context, razor).SingleAsync();

                // Then
                result.Content.ShouldBe(" <p>0</p>  <p>1</p>  <p>2</p>  <p>3</p>  <p>4</p> ");
            }

            [Test]
            public async Task Metadata()
            {
                // Given
                Engine engine = new Engine();
                TestExecutionContext context = GetExecutionContext(engine);
                TestDocument document = new TestDocument(@"<p>@Metadata[""MyKey""]</p>")
                {
                    { "MyKey", "MyValue" }
                };
                RenderRazor razor = new RenderRazor();

                // When
                TestDocument result = await ExecuteAsync(document, context, razor).SingleAsync();

                // Then
                result.Content.ShouldBe("<p>MyValue</p>");
            }

            [Test]
            public async Task Document()
            {
                // Given
                Engine engine = new Engine();
                TestExecutionContext context = GetExecutionContext(engine);
                TestDocument document = new TestDocument(
                    new NormalizedPath("/input/Temp/temp.txt"),
                    (NormalizedPath)null,
                    "<p>@Document.Source</p>");
                RenderRazor razor = new RenderRazor();

                // When
                TestDocument result = await ExecuteAsync(document, context, razor).SingleAsync();

                // Then
                result.Content.ShouldBe("<p>/input/Temp/temp.txt</p>");
            }

            [Test]
            public async Task DocumentAsModel()
            {
                // Given
                Engine engine = new Engine();
                TestExecutionContext context = GetExecutionContext(engine);
                TestDocument document = GetDocument(
                    "/input/Temp/temp.txt",
                    "<p>@Model.Source</p>");
                RenderRazor razor = new RenderRazor();

                // When
                TestDocument result = await ExecuteAsync(document, context, razor).SingleAsync();

                // Then
                result.Content.ShouldBe("<p>/input/Temp/temp.txt</p>");
            }

            [Test]
            public async Task AlternateModel()
            {
                // Given
                Engine engine = new Engine();
                TestExecutionContext context = GetExecutionContext(engine);
                TestDocument document = GetDocument(
                    "/input/Temp/temp.txt",
                    @"@model IList<int>
<p>@Model.Count</p>");
                IList<int> model = new[] { 1, 2, 3 };
                RenderRazor razor = new RenderRazor().WithModel(Config.FromValue(model));

                // When
                TestDocument result = await ExecuteAsync(document, context, razor).SingleAsync();

                // Then
                result.Content.ShouldBe("<p>3</p>");
            }

            [Test]
            public async Task ViewData()
            {
                // Given
                Engine engine = new Engine();
                TestExecutionContext context = GetExecutionContext(engine);
                TestDocument document = GetDocument(
                    "/input/Temp/temp.txt",
                    @"<p>@ViewData[""Test""]</p>");
                const string model = "View data";
                RenderRazor razor = new RenderRazor().WithViewData("Test", Config.FromValue(model));

                // When
                TestDocument result = await ExecuteAsync(document, context, razor).SingleAsync();

                // Then
                result.Content.ShouldBe("<p>View data</p>");
            }

            [Test]
            public async Task ViewDataMultiple()
            {
                // Given
                Engine engine = new Engine();
                TestExecutionContext context = GetExecutionContext(engine);
                TestDocument document = GetDocument(
                    "/input/Temp/temp.txt",
                    @"@{ var numbers = (int[])ViewData[""List""]; }
<p>@ViewData[""Text""] @numbers[2]</p>");
                const string text = "Number :";
                IList<int> list = new[] { 1, 2, 3 };
                RenderRazor razor = new RenderRazor()
                    .WithViewData("Text", Config.FromValue(text))
                    .WithViewData("List", Config.FromValue(list));

                // When
                TestDocument result = await ExecuteAsync(document, context, razor).SingleAsync();

                // Then
                result.Content.ShouldBe("<p>Number : 3</p>");
            }

            [Test]
            public async Task ViewDataException()
            {
                // Given
                Engine engine = new Engine();
                TestExecutionContext context = GetExecutionContext(engine);
                TestDocument document = GetDocument(
                    "/input/Temp/temp.txt",
                    @"<p></p>");

                RenderRazor razor = new RenderRazor()
                    .WithViewData("Test", Config.FromContext(ctx => throw new InvalidOperationException("Kaboom")));

                // When
                InvalidOperationException exception = await Should.ThrowAsync<InvalidOperationException>(() => ExecuteAsync(document, context, razor).SingleAsync());

                // Then
                exception.Message.ShouldContain("'Test'");
            }

            [Test]
            public async Task AlternateModelWithLayout()
            {
                // Given
                Engine engine = new Engine();
                TestExecutionContext context = GetExecutionContext(engine);
                TestDocument document = GetDocument(
                    "/input/ViewStartAndLayout/Test.cshtml",
                    @"@model int[]
<p>This is a test</p>");
                IList<int> model = new[] { 1, 2, 3 };
                RenderRazor razor = new RenderRazor().WithModel(Config.FromValue(model));

                // When
                TestDocument result = await ExecuteAsync(document, context, razor).SingleAsync();

                // Then
                result.Content.ShouldBe(
                    @"LAYOUT2
<p>This is a test</p>",
                    StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public async Task LoadLayoutFile()
            {
                // Given
                Engine engine = new Engine();
                TestExecutionContext context = GetExecutionContext(engine);
                TestDocument document = GetDocument(
                    "/input/Layout/Test.cshtml",
                    @"@{
	Layout = ""_Layout.cshtml"";
}
<p>This is a test</p>");
                RenderRazor razor = new RenderRazor();

                // When
                TestDocument result = await ExecuteAsync(document, context, razor).SingleAsync();

                // Then
                result.Content.ShouldBe(
                    @"LAYOUT
<p>This is a test</p>",
                    StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public async Task RenderModuleDefinedLayoutFile()
            {
                // Given
                Engine engine = new Engine();
                TestExecutionContext context = GetExecutionContext(engine);
                TestDocument document = GetDocument(
                    "/input/Layout/Test.cshtml",
                    "<p>This is a test</p>");
                RenderRazor razor = new RenderRazor().WithLayout((NormalizedPath)"_Layout.cshtml");

                // When
                TestDocument result = await ExecuteAsync(document, context, razor).SingleAsync();

                // Then
                result.Content.ShouldBe(
                    @"LAYOUT
<p>This is a test</p>",
                    StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public async Task RenderModuleDefinedRelativeLayoutFile()
            {
                // Given
                Engine engine = new Engine();
                TestExecutionContext context = GetExecutionContext(engine);
                TestDocument document = GetDocument(
                    "/input/Layout/Test.cshtml",
                    "<p>This is a test</p>");
                RenderRazor razor = new RenderRazor().WithLayout((NormalizedPath)"../ViewStartAndLayout/_Layout.cshtml");

                // When
                TestDocument result = await ExecuteAsync(document, context, razor).SingleAsync();

                // Then
                result.Content.ShouldBe(
                    @"LAYOUT2
<p>This is a test</p>",
                    StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public async Task RenderModuleDefinedViewStartFile()
            {
                // Given
                Engine engine = new Engine();
                TestExecutionContext context = GetExecutionContext(engine);
                TestDocument document = GetDocument(
                    "/input/Layout/Test.cshtml",
                    "<p>This is a test</p>");
                RenderRazor razor = new RenderRazor().WithViewStart((NormalizedPath)"/ViewStartAndLayout/_ViewStart.cshtml");

                // When
                TestDocument result = await ExecuteAsync(document, context, razor).SingleAsync();

                // Then
                result.Content.ShouldBe(
                    @"LAYOUT2
<p>This is a test</p>",
                    StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public async Task RenderModuleDefinedLayoutFileOverridesViewStart()
            {
                // Given
                Engine engine = new Engine();
                TestExecutionContext context = GetExecutionContext(engine);
                TestDocument document = GetDocument(
                    "/input/ViewStartAndLayout/Test.cshtml",
                    "<p>This is a test</p>");
                RenderRazor razor = new RenderRazor().WithLayout((NormalizedPath)"../Layout/_Layout.cshtml");

                // When
                TestDocument result = await ExecuteAsync(document, context, razor).SingleAsync();

                // Then
                result.Content.ShouldBe(
                    @"LAYOUT
<p>This is a test</p>",
                    StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public async Task LoadViewStartAndLayoutFile()
            {
                // Given
                Engine engine = new Engine();
                TestExecutionContext context = GetExecutionContext(engine);
                TestDocument document = GetDocument(
                    "/input/ViewStartAndLayout/Test.cshtml",
                    "<p>This is a test</p>");
                RenderRazor razor = new RenderRazor();

                // When
                TestDocument result = await ExecuteAsync(document, context, razor).SingleAsync();

                // Then
                result.Content.ShouldBe(
                    @"LAYOUT2
<p>This is a test</p>",
                    StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public async Task AlternateViewStartPath()
            {
                // Given
                Engine engine = new Engine();
                TestExecutionContext context = GetExecutionContext(engine);
                TestDocument document = GetDocument(
                    "/input/AlternateViewStartPath/Test.cshtml",
                    "<p>This is a test</p>");
                RenderRazor razor = new RenderRazor().WithViewStart((NormalizedPath)"/AlternateViewStart/_ViewStart.cshtml");

                // When
                TestDocument result = await ExecuteAsync(document, context, razor).SingleAsync();

                // Then
                result.Content.ShouldBe(
                    @"LAYOUT3
<p>This is a test</p>",
                    StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public async Task AlternateViewStartPathWithRelativeLayout()
            {
                // Given
                Engine engine = new Engine();
                TestExecutionContext context = GetExecutionContext(engine);
                TestDocument document = GetDocument(
                    "/input/AlternateViewStartPath/Test.cshtml",
                    "<p>This is a test</p>");
                RenderRazor razor = new RenderRazor().WithViewStart((NormalizedPath)"/AlternateViewStart/_ViewStartRelativeLayout.cshtml");

                // When
                TestDocument result = await ExecuteAsync(document, context, razor).SingleAsync();

                // Then
                result.Content.ShouldBe(
                    @"LAYOUT3
<p>This is a test</p>",
                    StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public async Task AlternateRelativeViewStartPathWithRelativeLayout()
            {
                // Given
                Engine engine = new Engine();
                TestExecutionContext context = GetExecutionContext(engine);
                TestDocument document = GetDocument(
                    "/input/AlternateViewStartPath/Test.cshtml",
                    "<p>This is a test</p>");
                RenderRazor razor = new RenderRazor().WithViewStart((NormalizedPath)"AlternateViewStart/_ViewStartRelativeLayout.cshtml");

                // When
                TestDocument result = await ExecuteAsync(document, context, razor).SingleAsync();

                // Then
                result.Content.ShouldBe(
                    @"LAYOUT3
<p>This is a test</p>",
                    StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public async Task IgnoresUnderscoresByDefault()
            {
                // Given
                Engine engine = new Engine();
                TestExecutionContext context = GetExecutionContext(engine);
                TestDocument document1 = GetDocument(
                    "/input/IgnoreUnderscores/Test.cshtml",
                    @"@{
	Layout = ""_Layout.cshtml"";
}
<p>This is a test</p>");
                TestDocument document2 = GetDocument(
                    "/input/IgnoreUnderscores/_Layout.cshtml",
                    @"LAYOUT4
@RenderBody()");
                RenderRazor razor = new RenderRazor();

                // When
                TestDocument result = await ExecuteAsync(new[] { document1, document2 }, context, razor).SingleAsync();

                // Then
                result.Content.ShouldBe(
                    @"LAYOUT4
<p>This is a test</p>",
                    StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public async Task AlternateIgnorePrefix()
            {
                // Given
                Engine engine = new Engine();
                TestExecutionContext context = GetExecutionContext(engine);
                TestDocument document1 = GetDocument(
                    "/input/AlternateIgnorePrefix/Test.cshtml",
                    "<p>This is a test</p>");
                TestDocument document2 = GetDocument(
                    "/input/AlternateIgnorePrefix/IgnoreMe.cshtml",
                    "<p>Ignore me</p>");
                RenderRazor razor = new RenderRazor().IgnorePrefix("Ignore");

                // When
                TestDocument result = await ExecuteAsync(new[] { document1, document2 }, context, razor).SingleAsync();

                // Then
                result.Content.ShouldBe("<p>This is a test</p>");
            }

            [Test]
            public async Task RenderLayoutSection()
            {
                // Given
                Engine engine = new Engine();
                TestExecutionContext context = GetExecutionContext(engine);
                TestDocument document = GetDocument(
                    "/input/LayoutWithSection/Test.cshtml",
                    @"@{
	Layout = ""_Layout.cshtml"";
}
@section MySection {
<p>Section Content</p>
}
<p>This is a test</p>");
                RenderRazor razor = new RenderRazor();

                // When
                TestDocument result = await ExecuteAsync(document, context, razor).SingleAsync();

                // Then
                result.Content.ShouldBe(
                    @"LAYOUT5

<p>Section Content</p>

<p>This is a test</p>",
                    StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public async Task RenderLayoutSectionOnMultipleExecution()
            {
                // Given
                Engine engine = new Engine();
                TestExecutionContext context = GetExecutionContext(engine);
                TestDocument document = GetDocument(
                    "/input/LayoutWithSection/Test.cshtml",
                    @"@{
	Layout = ""_Layout.cshtml"";
}
@section MySection {
<p>Section Content</p>
}
<p>This is a test</p>");
                RenderRazor razor = new RenderRazor();

                // When
                IReadOnlyList<IDocument> results1 = await ExecuteAsync(document, context, razor);
                IReadOnlyList<IDocument> results2 = await ExecuteAsync(document, context, razor);

                // Then
                (await results1.Single().GetContentStringAsync()).ShouldBe(
                    @"LAYOUT5

<p>Section Content</p>

<p>This is a test</p>",
                    StringCompareShould.IgnoreLineEndings);

                (await results2.Single().GetContentStringAsync()).ShouldBe(
                    @"LAYOUT5

<p>Section Content</p>

<p>This is a test</p>",
                    StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public async Task RenderRelativePartial()
            {
                // Given
                Engine engine = new Engine();
                TestExecutionContext context = GetExecutionContext(engine);
                TestDocument document = GetDocument(
                    "/input/RelativePartial/Test.cshtml",
                    @"<p>Before</p>
@Html.Partial(""_Partial.cshtml"")
<p>After</p>");
                RenderRazor razor = new RenderRazor();

                // When
                TestDocument result = await ExecuteAsync(document, context, razor).SingleAsync();

                // Then
                result.Content.ShouldBe(
                    @"<p>Before</p>
<div>Hi!</div>
<p>After</p>",
                    StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public async Task RenderRelativePartialFromLayout()
            {
                // Given
                Engine engine = new Engine();
                TestExecutionContext context = GetExecutionContext(engine);
                TestDocument document = GetDocument(
                    "/input/RelativePartial/Test.cshtml",
                    @"@{
	Layout = ""_Layout.cshtml"";
}
<p>Testing</p>");
                RenderRazor razor = new RenderRazor();

                // When
                TestDocument result = await ExecuteAsync(document, context, razor).SingleAsync();

                // Then
                result.Content.ShouldBe(
                    @"<p>Layout before</p>
<div>Hi!</div>
<p>Testing</p>
<p>Layout after</p>",
                    StringCompareShould.IgnoreLineEndings);
            }

            // Note that the absolute root is the root of the virtual input folder
            [Test]
            public async Task RenderAbsolutePartial()
            {
                // Given
                Engine engine = new Engine();
                TestExecutionContext context = GetExecutionContext(engine);
                TestDocument document = GetDocument(
                    "/input/RootRelativePartial/Test.cshtml",
                    @"<p>Before</p>
@Html.Partial(""/RelativePartial/_Partial.cshtml"")
<p>After</p>");
                RenderRazor razor = new RenderRazor();

                // When
                TestDocument result = await ExecuteAsync(document, context, razor).SingleAsync();

                // Then
                result.Content.ShouldBe(
                    @"<p>Before</p>
<div>Hi!</div>
<p>After</p>",
                    StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public async Task PartialTagHelper()
            {
                // Given
                Engine engine = new Engine();
                TestExecutionContext context = GetExecutionContext(engine);
                TestDocument document = GetDocument(
                    "/input/RelativePartial/Test.cshtml",
                    @"@addTagHelper *, Microsoft.AspNetCore.Mvc.TagHelpers
<p>Before</p>
<partial name=""_Partial.cshtml"">
<p>After</p>");
                RenderRazor razor = new RenderRazor();

                // When
                TestDocument result = await ExecuteAsync(document, context, razor).SingleAsync();

                // Then
                result.Content.ShouldBe(
                    @"<p>Before</p>
<div>Hi!</div>
<p>After</p>",
                    StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public async Task CustomTagHelper()
            {
                // Given
                Engine engine = new Engine();
                TestExecutionContext context = GetExecutionContext(engine);
                TestDocument document = GetDocument(
                    "/input/RelativePartial/Test.cshtml",
                    $@"@addTagHelper *, {typeof(RenderRazorFixture).Assembly.GetName().Name}
<p>Before</p>
<email>bar</email>
<p>After</p>");
                RenderRazor razor = new RenderRazor();

                // When
                TestDocument result = await ExecuteAsync(document, context, razor).SingleAsync();

                // Then
                result.Content.ShouldBe(
                    @"<p>Before</p>
<a href=""mailto:bar@foo.com"">bar@foo.com</a>
<p>After</p>",
                    StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public async Task IgnoresTildeLinks()
            {
                // Given
                Engine engine = new Engine();
                TestExecutionContext context = GetExecutionContext(engine);
                TestDocument document = new TestDocument("<p><a href=\"~/foo\">Foo</a></p>");
                RenderRazor razor = new RenderRazor();

                // When
                TestDocument result = await ExecuteAsync(document, context, razor).SingleAsync();

                // Then
                result.Content.ShouldBe("<p><a href=\"~/foo\">Foo</a></p>");
            }

            private TestDocument GetDocument(string source, string content) => new TestDocument(new NormalizedPath(source), (NormalizedPath)null, content);

            private TestExecutionContext GetExecutionContext(Engine engine)
            {
                TestExecutionContext context = new TestExecutionContext
                {
                    Namespaces = new TestNamespacesCollection(engine.Namespaces.ToArray()),
                    FileSystem = GetFileSystem()
                };
                context.Services.AddSingleton<IReadOnlyFileSystem>(context.FileSystem);
                return context;
            }

            private TestFileSystem GetFileSystem() => new TestFileSystem()
            {
                RootPath = NormalizedPath.AbsoluteRoot,
                InputPaths = new PathCollection("input"),
                FileProvider = GetFileProvider()
            };

            private TestFileProvider GetFileProvider() => new TestFileProvider
            {
                {
                    "/input/Layout/_Layout.cshtml",
                    @"LAYOUT
@RenderBody()"
                },
                {
                    "/input/ViewStartAndLayout/_ViewStart.cshtml",
                    @"@{
	Layout = ""_Layout.cshtml"";
}"
                },
                {
                    "/input/ViewStartAndLayout/_Layout.cshtml",
                    @"LAYOUT2
@RenderBody()"
                },
                {
                    "/input/AlternateViewStart/_ViewStart.cshtml",
                    @"@{
	Layout = @""/AlternateViewStart/_Layout.cshtml"";
}"
                },
                {
                    "/input/AlternateViewStart/_ViewStartRelativeLayout.cshtml",
                    @"@{
	Layout = @""_Layout.cshtml"";
}"
                },
                {
                    "/input/AlternateViewStart/_Layout.cshtml",
                    @"LAYOUT3
@RenderBody()"
                },
                {
                    "/input/IgnoreUnderscores/_Layout.cshtml",
                    @"LAYOUT4
@RenderBody()"
                },
                {
                    "/input/LayoutWithSection/_Layout.cshtml",
                    @"LAYOUT5
@RenderSection(""MySection"", false)
@RenderBody()"
                },
                {
                    "/input/RelativePartial/_Partial.cshtml",
                    "<div>Hi!</div>"
                },
                {
                    "/input/RelativePartial/_Layout.cshtml",
                    @"<p>Layout before</p>
@Html.Partial(""_Partial.cshtml"")
@RenderBody()
<p>Layout after</p>"
                }
            };
        }
    }
}