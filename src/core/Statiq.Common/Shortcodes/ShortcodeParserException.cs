using System;

namespace Statiq.Common.Shortcodes
{
    public class ShortcodeParserException : Exception
    {
        public ShortcodeParserException(string message)
            : base(message)
        {
        }
    }
}
