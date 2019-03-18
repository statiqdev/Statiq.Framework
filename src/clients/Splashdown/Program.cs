using System;
using Wyam.App;
using Wyam.Common.Execution;
using Wyam.Core.Modules.Contents;
using Wyam.Core.Modules.Control;
using Wyam.Core.Modules.IO;
using Wyam.Markdown;
using Wyam.Yaml;

namespace Splashdown
{
    public class Program
    {
        public static int Main(string[] args) =>
            Bootstrapper
                .CreateDefault(args)
                .AddPipeline(
                    new ReadFiles("*.md"),
                    new FrontMatter(new Yaml()),
                    new Markdown(),
                    new ReplaceIn("{{CONTENT}}", new ReadFiles("template.html")),
                    new Replace("{{TITLE}}", (doc, _) => doc.Get("Title", "Default Title")),
                    new Replace("{{DESC}}", (doc, _) => doc.Get("Description", "Default Description")),
                    new WriteFiles(".html"))
                .Run();
    }
}
