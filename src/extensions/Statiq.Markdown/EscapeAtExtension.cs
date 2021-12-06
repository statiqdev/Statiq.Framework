using Markdig;
using Markdig.Renderers;

namespace Statiq.Markdown
{
    public class EscapeAtExtension : IMarkdownExtension
    {
        public void Setup(MarkdownPipelineBuilder pipeline)
        {
            pipeline.InlineParsers.AddIfNotAlready(new EscapeAtParser());
        }

        public void Setup(MarkdownPipeline pipeline, IMarkdownRenderer renderer)
        {
            renderer.ObjectRenderers.AddIfNotAlready(new EscapeAtInlineRenderer());
        }
    }
}