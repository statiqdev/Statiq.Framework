using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Shouldly;
using Statiq.Common;
using Statiq.Core;
using Statiq.Testing;

namespace Statiq.Razor.Tests
{
    [TestFixture]
    public class RazorFixture : BaseFixture
    {
        public class ExecuteTests : RazorFixture
        {
            [Test]
            public async Task SimpleTemplate()
            {
                // Given
                Engine engine = new Engine();
                TestExecutionContext context = GetExecutionContext(engine);
                TestDocument document = new TestDocument("@for(int c = 0 ; c < 5 ; c++) { <p>@c</p> }");
                Razor razor = new Razor();

                // When
                TestDocument result = await ExecuteAsync(document, context, razor).SingleAsync();

                // Then
                result.Content.ShouldBe(" <p>0</p>  <p>1</p>  <p>2</p>  <p>3</p>  <p>4</p> ");
            }

            [Test]
            [NonParallelizable]
            public async Task Tracing()
            {
                // Given
                Engine engine = new Engine();
                TestExecutionContext context = GetExecutionContext(engine);
                TestDocument document = new TestDocument(@"@{ Trace.Information(""Test""); }");
                TraceListener traceListener = new TraceListener();
                Trace.AddListener(traceListener);
                Razor razor = new Razor();

                // When
                await ExecuteAsync(document, context, razor).SingleAsync();

                // Then
                Trace.RemoveListener(traceListener);
                traceListener.Messages.ShouldContain("Test");
            }

            public class TraceListener : System.Diagnostics.TextWriterTraceListener
            {
                public List<string> Messages { get; set; } = new List<string>();

                public override void TraceEvent(System.Diagnostics.TraceEventCache eventCache, string source, System.Diagnostics.TraceEventType eventType, int id, string message)
                {
                    LogMessage(eventType, message);
                }

                public override void TraceEvent(System.Diagnostics.TraceEventCache eventCache, string source, System.Diagnostics.TraceEventType eventType, int id, string format, params object[] args)
                {
                    LogMessage(eventType, string.Format(format, args));
                }

                private void LogMessage(System.Diagnostics.TraceEventType eventType, string message)
                {
                    Messages.Add(message);
                }
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
                Razor razor = new Razor();

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
                TestDocument document = new TestDocument(new FilePath("/Temp/temp.txt"), (FilePath)null, "<p>@Document.Source</p>");
                Razor razor = new Razor();

                // When
                TestDocument result = await ExecuteAsync(document, context, razor).SingleAsync();

                // Then
                result.Content.ShouldBe("<p>/Temp/temp.txt</p>");
            }

            [Test]
            public async Task DocumentAsModel()
            {
                // Given
                Engine engine = new Engine();
                TestExecutionContext context = GetExecutionContext(engine);
                TestDocument document = GetDocument("/Temp/temp.txt", "<p>@Model.Source</p>");
                Razor razor = new Razor();

                // When
                TestDocument result = await ExecuteAsync(document, context, razor).SingleAsync();

                // Then
                result.Content.ShouldBe("<p>/Temp/temp.txt</p>");
            }

            [Test]
            public async Task AlternateModel()
            {
                // Given
                Engine engine = new Engine();
                TestExecutionContext context = GetExecutionContext(engine);
                TestDocument document = GetDocument("/Temp/temp.txt", @"@model IList<int>
<p>@Model.Count</p>");
                IList<int> model = new[] { 1, 2, 3 };
                Razor razor = new Razor().WithModel(Config.FromValue(model));

                // When
                TestDocument result = await ExecuteAsync(document, context, razor).SingleAsync();

                // Then
                result.Content.ShouldBe("<p>3</p>");
            }

            [Test]
            public async Task LoadLayoutFile()
            {
                // Given
                Engine engine = new Engine();
                TestExecutionContext context = GetExecutionContext(engine);
                TestDocument document = GetDocument(
                    "/Layout/Test.cshtml",
                    @"@{
	Layout = ""_Layout.cshtml"";
}
<p>This is a test</p>");
                Razor razor = new Razor();

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
                    "/Layout/Test.cshtml",
                    "<p>This is a test</p>");
                Razor razor = new Razor().WithLayout((FilePath)"_Layout.cshtml");

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
                    "/ViewStartAndLayout/Test.cshtml",
                    "<p>This is a test</p>");
                Razor razor = new Razor();

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
                    "/AlternateViewStartPath/Test.cshtml",
                    "<p>This is a test</p>");
                Razor razor = new Razor().WithViewStart((FilePath)"/AlternateViewStart/_ViewStart.cshtml");

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
                    "/AlternateViewStartPath/Test.cshtml",
                    "<p>This is a test</p>");
                Razor razor = new Razor().WithViewStart((FilePath)"/AlternateViewStart/_ViewStartRelativeLayout.cshtml");

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
                    "/AlternateViewStartPath/Test.cshtml",
                    "<p>This is a test</p>");
                Razor razor = new Razor().WithViewStart((FilePath)"AlternateViewStart/_ViewStartRelativeLayout.cshtml");

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
                    "/IgnoreUnderscores/Test.cshtml",
                    @"@{
	Layout = ""_Layout.cshtml"";
}
<p>This is a test</p>");
                TestDocument document2 = GetDocument(
                    "/IgnoreUnderscores/_Layout.cshtml",
                    @"LAYOUT4
@RenderBody()");
                Razor razor = new Razor();

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
                    "/AlternateIgnorePrefix/Test.cshtml",
                    "<p>This is a test</p>");
                TestDocument document2 = GetDocument(
                    "/AlternateIgnorePrefix/IgnoreMe.cshtml",
                    "<p>Ignore me</p>");
                Razor razor = new Razor().IgnorePrefix("Ignore");

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
                    "/LayoutWithSection/Test.cshtml",
                    @"@{
	Layout = ""_Layout.cshtml"";
}
@section MySection {
<p>Section Content</p>
}
<p>This is a test</p>");
                Razor razor = new Razor();

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
                    "/LayoutWithSection/Test.cshtml",
                    @"@{
	Layout = ""_Layout.cshtml"";
}
@section MySection {
<p>Section Content</p>
}
<p>This is a test</p>");
                Razor razor = new Razor();

                // When
                List<IDocument> results1 = await razor.ExecuteAsync(new[] { document }, context).ToListAsync();
                List<IDocument> results2 = await razor.ExecuteAsync(new[] { document }, context).ToListAsync();

                // Then
                (await results1.Single().GetStringAsync()).ShouldBe(
                    @"LAYOUT5

<p>Section Content</p>

<p>This is a test</p>",
                    StringCompareShould.IgnoreLineEndings);

                (await results2.Single().GetStringAsync()).ShouldBe(
                    @"LAYOUT5

<p>Section Content</p>

<p>This is a test</p>",
                    StringCompareShould.IgnoreLineEndings);
            }

            private TestDocument GetDocument(string source, string content) => new TestDocument(new FilePath(source), (FilePath)null, content);

            private TestExecutionContext GetExecutionContext(Engine engine)
            {
                return new TestExecutionContext
                {
                    Namespaces = engine.Namespaces,
                    FileSystem = GetFileSystem()
                };
            }

            private TestFileSystem GetFileSystem()
            {
                TestFileProvider fileProvider = GetFileProvider();
                return new TestFileSystem()
                {
                    RootPath = new DirectoryPath("/"),
                    InputPaths = new PathCollection<DirectoryPath>()
                    {
                        new DirectoryPath("/")
                    },
                    FileProvider = fileProvider
                };
            }

            private TestFileProvider GetFileProvider()
            {
                TestFileProvider fileProvider = new TestFileProvider();

                fileProvider.AddDirectory("/");
                fileProvider.AddDirectory("/AlternateIgnorePrefix");
                fileProvider.AddDirectory("/AlternateViewStart");
                fileProvider.AddDirectory("/AlternateViewStartPath");
                fileProvider.AddDirectory("/IgnoreUnderscores");
                fileProvider.AddDirectory("/Layout");
                fileProvider.AddDirectory("/LayoutWithSection");
                fileProvider.AddDirectory("/SimpleTemplate");
                fileProvider.AddDirectory("/ViewStartAndLayout");

                fileProvider.AddFile(
                    "/Layout/_Layout.cshtml",
                    @"LAYOUT
@RenderBody()");
                fileProvider.AddFile(
                    "/ViewStartAndLayout/_ViewStart.cshtml",
                    @"@{
	Layout = ""_Layout.cshtml"";
}");
                fileProvider.AddFile(
                    "/ViewStartAndLayout/_Layout.cshtml",
                    @"LAYOUT2
@RenderBody()");
                fileProvider.AddFile(
                    "/AlternateViewStart/_ViewStart.cshtml",
                    @"@{
	Layout = @""/AlternateViewStart/_Layout.cshtml"";
}");
                fileProvider.AddFile(
                    "/AlternateViewStart/_ViewStartRelativeLayout.cshtml",
                    @"@{
	Layout = @""_Layout.cshtml"";
}");
                fileProvider.AddFile(
                    "/AlternateViewStart/_Layout.cshtml",
                    @"LAYOUT3
@RenderBody()");
                fileProvider.AddFile(
                    "/IgnoreUnderscores/_Layout.cshtml",
                    @"LAYOUT4
@RenderBody()");
                fileProvider.AddFile(
                    "/LayoutWithSection/_Layout.cshtml",
                    @"LAYOUT5
@RenderSection(""MySection"", false)
@RenderBody()");

                return fileProvider;
            }
        }
    }
}