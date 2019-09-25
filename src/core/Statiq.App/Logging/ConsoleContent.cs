using System;

namespace Statiq.App
{
    internal struct ConsoleContent
    {
        public ConsoleContent(ConsoleColor foreground, ConsoleColor background, ReadOnlyMemory<char> message)
        {
            Foreground = foreground;
            Background = background;
            Message = message;
        }

        public ConsoleContent(ReadOnlyMemory<char> message)
        {
            Foreground = ConsoleColor.Gray;
            Background = ConsoleColor.Black;
            Message = message;
        }

        public ConsoleColor Foreground { get; }

        public ConsoleColor Background { get; }

        public ReadOnlyMemory<char> Message { get; }
    }
}
