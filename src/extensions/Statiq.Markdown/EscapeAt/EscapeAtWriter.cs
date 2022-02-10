using System.IO;
using System.Text;

namespace Statiq.Markdown.EscapeAt
{
    internal class EscapeAtWriter : TextWriter
    {
        private readonly TextWriter _writer;

        private char _previousChar;

        private bool _processingInstruction;

        public override Encoding Encoding => _writer.Encoding;

        public EscapeAtWriter(TextWriter writer)
        {
            _writer = writer;
        }

        public override void Write(char value)
        {
            if (!_processingInstruction)
            {
                switch (value)
                {
                    case '\\':
                        if (_previousChar == '\\')
                        {
                            // If this is a second, etc. slash, go ahead and write the previous one
                            _writer.Write('\\');
                        }
                        _previousChar = '\\';
                        return;
                    case '@':
                        _writer.Write(_previousChar == '\\' ? "@" : "&#64;");
                        _previousChar = '@';
                        return;
                    case '?':
                        if (_previousChar == '<')
                        {
                            _processingInstruction = true;
                        }
                        break;
                    default:
                        if (_previousChar == '\\')
                        {
                            _writer.Write('\\');
                        }
                        break;
                }
            }
            else if (value == '>' && _previousChar == '?')
            {
                _processingInstruction = false;
            }

            _previousChar = value;
            _writer.Write(value);
        }

        public override void Flush()
        {
            if (_previousChar == '\\')
            {
                _writer.Write('\\');
            }
        }
    }
}