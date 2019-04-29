using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Wyam.App;
using Wyam.Common.Configuration;
using Wyam.Common.Execution;
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
                .AddPipeline(
                    "Sample",
                    new ReadFiles("*.md"),
                    new FrontMatter(new Yaml()),
                    new Markdown(),
                    new ReplaceIn("{{CONTENT}}", new ReadFiles("template.html")),
                    new Replace("{{TITLE}}", Config.FromDocument(doc => doc.Get("Title", "Default Title"))),
                    new Replace("{{DESC}}", Config.FromDocument(doc => doc.Get("Description", "Default Description"))),
                    new WriteFiles(".html"))
                .AddPipeline("AsAction", (p, s) =>
                {
                    p.Process.Add(new ReadFiles("*.md"));
                    p.Process.Add(new FrontMatter(new Yaml()));
                })
                .RunAsync();
    }
}
