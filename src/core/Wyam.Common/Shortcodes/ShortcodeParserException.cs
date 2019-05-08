using System;

namespace Wyam.Common.Shortcodes
{
    public class ShortcodeParserException : Exception
    {
        public ShortcodeParserException(string message)
            : base(message)
        {
        }
    }
}
