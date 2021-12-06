using Markdig.Helpers;
using Markdig.Parsers;

namespace Statiq.Markdown
{
    public class EscapeAtParser : InlineParser
    {
        public EscapeAtParser()
        {
            OpeningCharacters = new[] { '@' };
        }

        public override bool Match(InlineProcessor processor, ref StringSlice slice)
        {
            if (slice.CurrentChar == '@' && slice.PeekCharExtra(-1) != '\\')
            {
                processor.Inline = new EscapeAtInline()
                {
                    Span =
                    {
                        Start = processor.GetSourcePosition(slice.Start, out int line, out int column),
                    },
                    Line = line,
                    Column = column
                };
                processor.Inline.Span.End = processor.Inline.Span.Start + 1;
                slice.Start += 1;
                return true;
            }
            return false;
        }
    }
}