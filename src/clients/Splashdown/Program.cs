using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Wyam.App;
using Wyam.Common.Configuration;
using Wyam.Common.Execution;
using Wyam.Common.IO;
using Wyam.Common.Meta;
using Wyam.Common.Modules;
using Wyam.Core.Modules.Contents;
using Wyam.Core.Modules.Control;
using Wyam.Core.Modules.IO;
using Wyam.Markdown;
using Wyam.Yaml;

namespace Splashdown
{
    public class Program
    {
        // Use with the YamlFrontMatter example from that folder
        // dotnet run --project ..\..\src\clients\Splashdown\Splashdown.csproj -- preview
        [SuppressMessage("Roslynator.CSharp.Analyzers", "RCS1046", Justification = "Main does not accept async suffix")]
        public static async Task<int> Main(string[] args) =>
            await Bootstrapper
                .CreateDefault(args)
                .BuildPipeline("First", builder =>
                    builder
                        .WithReadModules(new ReadFiles("*.md"))
                        .WithProcessModules(
                            new FrontMatter(new Yaml()),
                            new Markdown(),
                            new ReplaceIn("{{CONTENT}}", new ReadFiles("template.html")),
                            new Replace("{{TITLE}}", Config.FromDocument(doc => doc.Get("Title", "Default Title"))),
                            new Replace("{{DESC}}", Config.FromDocument(doc => doc.Get("Description", "Default Description"))))
                        .WithWriteModules(new WriteFiles(".html")))
                .AddIsolatedPipeline(
                    "Second",
                    "*.md",
                    Config.FromDocument(doc => (FilePath)$"{doc.Source.FileName}2.html"),
                    new FrontMatter(new Yaml()),
                    new Markdown(),
                    new ReplaceIn("{{CONTENT}}", new ReadFiles("template.html")),
                    new Replace("{{TITLE}}", Config.FromDocument(doc => doc.Get("Title", "Default Title"))),
                    new Replace("{{DESC}}", Config.FromDocument(doc => doc.Get("Description", "Default Description"))))
                .RunAsync();
    }
}
