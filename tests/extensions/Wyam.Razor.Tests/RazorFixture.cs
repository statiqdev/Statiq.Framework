using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Shouldly;
using Wyam.Common.Configuration;
using Wyam.Common.Documents;
using Wyam.Common.Execution;
using Wyam.Common.IO;
using Wyam.Common.Meta;
using Wyam.Common.Tracing;
using Wyam.Common.Util;
using Wyam.Core.Execution;
using Wyam.Testing;
using Wyam.Testing.Documents;
using Wyam.Testing.Execution;
using Wyam.Testing.IO;

namespace Wyam.Razor.Tests
{
    [TestFixture]
    [Parallelizable(ParallelScope.Self | ParallelScope.Children)]
    public class RazorFixture : BaseFixture
    {
        public class ExecuteTests : RazorFixture
        {
            [Test]
            public async Task SimpleTemplate()
            {
                // Given
                Engine engine = new Engine();
                IExecutionContext context = GetExecutionContext(engine);
                IDocument document = new TestDocument("@for(int c = 0 ; c < 5 ; c++) { <p>@c</p> }");
                Razor razor = new Razor();

                // When
                List<IDocument> results = await razor.ExecuteAsync(new[] { document }, context).ToListAsync();  // Make sure to materialize the result list

                // Then
                results.Single().Content.ShouldBe(" <p>0</p>  <p>1</p>  <p>2</p>  <p>3</p>  <p>4</p> ");
            }

            [Test]
            [Parallelizable(ParallelScope.None)]
            public async Task Tracing()
            {
                // Given
                Engine engine = new Engine();
                IExecutionContext context = GetExecutionContext(engine);
                IDocument document = new TestDocument(@"@{ Trace.Information(""Test""); }");
                TraceListener traceListener = new TraceListener();
                Trace.AddListener(traceListener);
                Razor razor = new Razor();

                // When
                List<IDocument> results = await razor.ExecuteAsync(new[] { document }, context).ToListAsync();  // Make sure to materialize the result list

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
                IExecutionContext context = GetExecutionContext(engine);
                IDocument document = new TestDocument(@"<p>@Metadata[""MyKey""]</p>", new MetadataItems
                {
                    { "MyKey", "MyValue" }
                });
                Razor razor = new Razor();

                // When
                List<IDocument> results = await razor.ExecuteAsync(new[] { document }, context).ToListAsync();

                // Then
                results.Single().Content.ShouldBe("<p>MyValue</p>");
            }

            [Test]
            public async Task Document()
            {
                // Given
                Engine engine = new Engine();
                IExecutionContext context = GetExecutionContext(engine);
                IDocument document = new TestDocument("<p>@Document.Source</p>")
                {
                    Source = new FilePath("/Temp/temp.txt")
                };
                Razor razor = new Razor();

                // When
                List<IDocument> results = await razor.ExecuteAsync(new[] { document }, context).ToListAsync();

                // Then
                results.Single().Content.ShouldBe("<p>file:///Temp/temp.txt</p>");
            }

            [Test]
            public async Task DocumentAsModel()
            {
                // Given
                Engine engine = new Engine();
                IExecutionContext context = GetExecutionContext(engine);
                IDocument document = GetDocument("/Temp/temp.txt", "<p>@Model.Source</p>");
                Razor razor = new Razor();

                // When
                List<IDocument> results = await razor.ExecuteAsync(new[] { document }, context).ToListAsync();

                // Then
                results.Single().Content.ShouldBe("<p>file:///Temp/temp.txt</p>");
            }

            [Test]
            public async Task AlternateModel()
            {
                // Given
                Engine engine = new Engine();
                IExecutionContext context = GetExecutionContext(engine);
                IDocument document = GetDocument("C:/Temp/temp.txt", @"@model IList<int>
<p>@Model.Count</p>");
                IList<int> model = new[] { 1, 2, 3 };
                Razor razor = new Razor().WithModel(Config.FromValue(model));

                // When
                List<IDocument> results = await razor.ExecuteAsync(new[] { document }, context).ToListAsync();

                // Then
                results.Single().Content.ShouldBe("<p>3</p>");
            }

            [Test]
            public async Task LoadLayoutFile()
            {
                // Given
                Engine engine = new Engine();
                IExecutionContext context = GetExecutionContext(engine);
                IDocument document = GetDocument(
                    "/Layout/Test.cshtml",
                    @"@{
	Layout = ""_Layout.cshtml"";
}
<p>This is a test</p>");
                Razor razor = new Razor();

                // When
                List<IDocument> results = await razor.ExecuteAsync(new[] { document }, context).ToListAsync();

                // Then
                results.Single().Content.ShouldBe(
                    @"LAYOUT
<p>This is a test</p>",
                    StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public async Task RenderModuleDefinedLayoutFile()
            {
                // Given
                Engine engine = new Engine();
                IExecutionContext context = GetExecutionContext(engine);
                IDocument document = GetDocument(
                    "/Layout/Test.cshtml",
                    "<p>This is a test</p>");
                Razor razor = new Razor().WithLayout((FilePath)"_Layout.cshtml");

                // When
                List<IDocument> results = await razor.ExecuteAsync(new[] { document }, context).ToListAsync();

                // Then
                results.Single().Content.ShouldBe(
                    @"LAYOUT
<p>This is a test</p>",
                    StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public async Task LoadViewStartAndLayoutFile()
            {
                // Given
                Engine engine = new Engine();
                IExecutionContext context = GetExecutionContext(engine);
                IDocument document = GetDocument(
                    "/ViewStartAndLayout/Test.cshtml",
                    "<p>This is a test</p>");
                Razor razor = new Razor();

                // When
                List<IDocument> results = await razor.ExecuteAsync(new[] { document }, context).ToListAsync();

                // Then
                results.Single().Content.ShouldBe(
                    @"LAYOUT2
<p>This is a test</p>",
                    StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public async Task AlternateViewStartPath()
            {
                // Given
                Engine engine = new Engine();
                IExecutionContext context = GetExecutionContext(engine);
                IDocument document = GetDocument(
                    "/AlternateViewStartPath/Test.cshtml",
                    "<p>This is a test</p>");
                Razor razor = new Razor().WithViewStart((FilePath)"/AlternateViewStart/_ViewStart.cshtml");

                // When
                List<IDocument> results = await razor.ExecuteAsync(new[] { document }, context).ToListAsync();

                // Then
                results.Single().Content.ShouldBe(
                    @"LAYOUT3
<p>This is a test</p>",
                    StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public async Task AlternateViewStartPathWithRelativeLayout()
            {
                // Given
                Engine engine = new Engine();
                IExecutionContext context = GetExecutionContext(engine);
                IDocument document = GetDocument(
                    "/AlternateViewStartPath/Test.cshtml",
                    "<p>This is a test</p>");
                Razor razor = new Razor().WithViewStart((FilePath)"/AlternateViewStart/_ViewStartRelativeLayout.cshtml");

                // When
                List<IDocument> results = await razor.ExecuteAsync(new[] { document }, context).ToListAsync();

                // Then
                results.Single().Content.ShouldBe(
                    @"LAYOUT3
<p>This is a test</p>",
                    StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public async Task AlternateRelativeViewStartPathWithRelativeLayout()
            {
                // Given
                Engine engine = new Engine();
                IExecutionContext context = GetExecutionContext(engine);
                IDocument document = GetDocument(
                    "/AlternateViewStartPath/Test.cshtml",
                    "<p>This is a test</p>");
                Razor razor = new Razor().WithViewStart((FilePath)"AlternateViewStart/_ViewStartRelativeLayout.cshtml");

                // When
                List<IDocument> results = await razor.ExecuteAsync(new[] { document }, context).ToListAsync();

                // Then
                results.Single().Content.ShouldBe(
                    @"LAYOUT3
<p>This is a test</p>",
                    StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public async Task IgnoresUnderscoresByDefault()
            {
                // Given
                Engine engine = new Engine();
                IExecutionContext context = GetExecutionContext(engine);
                IDocument document1 = GetDocument(
                    "/IgnoreUnderscores/Test.cshtml",
                    @"@{
	Layout = ""_Layout.cshtml"";
}
<p>This is a test</p>");
                IDocument document2 = GetDocument(
                    "/IgnoreUnderscores/_Layout.cshtml",
                    @"LAYOUT4
@RenderBody()");
                Razor razor = new Razor();

                // When
                List<IDocument> results = await razor.ExecuteAsync(new[] { document1, document2 }, context).ToListAsync();

                // Then
                results.Single().Content.ShouldBe(
                    @"LAYOUT4
<p>This is a test</p>",
                    StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public async Task AlternateIgnorePrefix()
            {
                // Given
                Engine engine = new Engine();
                IExecutionContext context = GetExecutionContext(engine);
                IDocument document1 = GetDocument(
                    "/AlternateIgnorePrefix/Test.cshtml",
                    "<p>This is a test</p>");
                IDocument document2 = GetDocument(
                    "/AlternateIgnorePrefix/IgnoreMe.cshtml",
                    "<p>Ignore me</p>");
                Razor razor = new Razor().IgnorePrefix("Ignore");

                // When
                List<IDocument> results = await razor.ExecuteAsync(new[] { document1, document2 }, context).ToListAsync();

                // Then
                results.Single().Content.ShouldBe("<p>This is a test</p>");
            }

            [Test]
            public async Task RenderLayoutSection()
            {
                // Given
                Engine engine = new Engine();
                IExecutionContext context = GetExecutionContext(engine);
                IDocument document = GetDocument(
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
                List<IDocument> results = await razor.ExecuteAsync(new[] { document }, context).ToListAsync();

                // Then
                results.Single().Content.ShouldBe(
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
                IExecutionContext context = GetExecutionContext(engine);
                IDocument document = GetDocument(
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
                results1.Single().Content.ShouldBe(
                    @"LAYOUT5

<p>Section Content</p>

<p>This is a test</p>",
                    StringCompareShould.IgnoreLineEndings);

                results2.Single().Content.ShouldBe(
                    @"LAYOUT5

<p>Section Content</p>

<p>This is a test</p>",
                    StringCompareShould.IgnoreLineEndings);
            }

            private IDocument GetDocument(string source, string content)
            {
                TestDocument document = new TestDocument(content, new[]
                {
                    new KeyValuePair<string, object>(Keys.RelativeFilePath, new FilePath(source)),
                    new KeyValuePair<string, object>(Keys.SourceFileName, new FilePath(source).FileName)
                });
                document.Source = new FilePath(source);
                return document;
            }

            private IExecutionContext GetExecutionContext(Engine engine)
            {
                TestExecutionContext context = new TestExecutionContext();
                context.Namespaces = engine.Namespaces;
                context.FileSystem = GetFileSystem();
                return context;
            }

            private IReadOnlyFileSystem GetFileSystem()
            {
                TestFileProvider fileProvider = GetFileProvider();
                TestFileSystem fileSystem = new TestFileSystem()
                {
                    RootPath = new DirectoryPath("/"),
                    InputPaths = new PathCollection<DirectoryPath>()
                    {
                        new DirectoryPath("/")
                    },
                    FileProvider = fileProvider
                };
                return fileSystem;
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