using System.IO;
using System.Text;

namespace Statiq.Markdown.EscapeAt
{
    internal class EscapeAtWriter : TextWriter
    {
        private readonly TextWriter _writer;

        private bool _previousSlash;

        public override Encoding Encoding => _writer.Encoding;

        public EscapeAtWriter(TextWriter writer)
        {
            _writer = writer;
        }

        public override void Write(char value)
        {
            switch (value)
            {
                case '\\':
                    if (_previousSlash)
                    {
                        // If this is a second, etc. slash, go ahead and write the previous one
                        _writer.Write('\\');
                    }
                    _previousSlash = true;
                    return;
                case '@':
                    _writer.Write(_previousSlash ? "@" : "&#64;");
                    _previousSlash = false;
                    return;
                default:
                    if (_previousSlash)
                    {
                        _writer.Write('\\');
                    }
                    _previousSlash = false;
                    break;
            }
            _writer.Write(value);
        }

        public override void Flush()
        {
            if (_previousSlash)
            {
                _writer.Write('\\');
            }
            _previousSlash = false;
        }
    }
}