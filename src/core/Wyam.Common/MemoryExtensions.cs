using System;
using System.Collections.Generic;
using System.Text;
using Wyam.Common.Util;

namespace Wyam.Common
{
    public static class MemoryExtensions
    {
        public static bool StartsWith(this ReadOnlyMemory<char> item, ReadOnlyMemory<char> value) =>
            value.Length > item.Length ? false : item.Slice(0, value.Length).Span.SequenceEqual(value.Span);

        public static bool StartsWith(this IEnumerable<ReadOnlyMemory<char>> items, IEnumerable<ReadOnlyMemory<char>> values) =>
            items.StartsWith(values, new MemoryStringEqualityComparer());

        public static bool SequenceEqual(this ReadOnlyMemory<char> item, ReadOnlyMemory<char> value) =>
            item.Span.SequenceEqual(value.Span);
    }
}
