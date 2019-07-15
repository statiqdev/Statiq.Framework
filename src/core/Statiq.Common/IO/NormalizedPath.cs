using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace Statiq.Common
{
    /// <summary>
    /// Provides properties and instance methods for working with paths.
    /// </summary>
    public abstract class NormalizedPath : IComparable<NormalizedPath>, IComparable, IEquatable<NormalizedPath>
    {
        // Initially based on code from Cake (http://cakebuild.net/)

        /// <summary>
        /// The type of string comparison to perform when comparing paths.
        /// </summary>
        /// <remarks>
        /// This defaults to <see cref="StringComparison.Ordinal"/> on Linux platforms and
        /// <see cref="StringComparison.OrdinalIgnoreCase"/> on Windows and MacOS.
        /// </remarks>
        public static StringComparison PathComparisonType { get; set; } =
            RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;

        public const string Dot = ".";
        public const string DotDot = "..";
        public const string Slash = "/";

        private const string WhitespaceChars = "\r\n\t";

        /// <summary>
        /// Initializes a new instance of the <see cref="NormalizedPath" /> class.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="pathKind">Specifies whether the path is relative, absolute, or indeterminate.</param>
        protected NormalizedPath(string path, PathKind pathKind)
        {
            _ = path ?? throw new ArgumentNullException(nameof(path));

            ReadOnlySpan<char> fullPath = GetFullPath(path);
            IsAbsolute = GetIsAbsolute(pathKind, fullPath);
            (FullPath, Segments) = GetFullPathAndSegments(fullPath);
        }

        private static ReadOnlySpan<char> GetFullPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentException("Path cannot be empty");
            }

            // Normalize slashes and trim whitespace
            // Leave spaces since they're valid path chars
            ReadOnlySpan<char> fullPath = path
                .Replace('\\', '/')
                .AsSpan()
                .Trim(WhitespaceChars.AsSpan());

            // Remove relative part of a path, but only if it's not the only part
            if (fullPath.Length > 2 && fullPath[0] == '.' && fullPath[1] == '/')
            {
                fullPath = fullPath.Slice(2);
            }

            // Remove trailing slashes (as long as this isn't just a slash)
            if (fullPath.Length > 1)
            {
                fullPath = fullPath.TrimEnd('/');
            }

            // Add a backslash if this is a windows root (I.e., ends with :)
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                && fullPath.Length > 1
                && fullPath[fullPath.Length - 1] == ':')
            {
                Span<char> pathSpan = new char[fullPath.Length + 1];
                fullPath.CopyTo(pathSpan);
                pathSpan[pathSpan.Length - 1] = '/';
                fullPath = pathSpan;
            }

            return fullPath;
        }

        private static bool GetIsAbsolute(PathKind pathKind, ReadOnlySpan<char> path)
        {
            switch (pathKind)
            {
                case PathKind.RelativeOrAbsolute:
                    return System.IO.Path.IsPathRooted(path);
                case PathKind.Absolute:
                    return true;
                case PathKind.Relative:
                    return false;
            }
            return false;
        }

        // Internal for testing
        // Splits the path on /, collapses it, and then pools the segments
        internal static (string, ReadOnlyMemory<char>[]) GetFullPathAndSegments(ReadOnlySpan<char> path)
        {
            // Return the current path (.) if the path is null or empty
            if (path == default || path.IsEmpty)
            {
                return (Dot, new ReadOnlyMemory<char>[] { Dot.AsMemory() });
            }

            // If the path is only one character, we don't need to do anything
            if (path.Length == 1)
            {
                switch (path[0])
                {
                    case '.':
                        return (Dot, new ReadOnlyMemory<char>[] { Dot.AsMemory() });
                    case '/':
                        return (Slash, new ReadOnlyMemory<char>[] { });  // segments should be empty if the path is just a slash
                }

                string pathString = path.ToString();
                return (pathString, new ReadOnlyMemory<char>[] { pathString.AsMemory() });
            }

            // Special case if path is just a windows drive
            // (it will always have a trailing slash because that got added earlier)
            // The segment should not have the trailing slash
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                && path.Length > 2
                && (path[path.Length - 1] == '/' && path[path.Length - 2] == ':'))
            {
                string pathString = path.ToString();
                return (pathString, new ReadOnlyMemory<char>[] { pathString.AsMemory().Slice(0, path.Length - 1) });
            }

            // Note if the path starts with a slash because we'll add it back in
            bool pathStartsWithSlash = path[0] == '/';

            // Split the path
            List<(int, int)> splits = new List<(int, int)>();
            int length = 0;
            for (int c = 0; c < path.Length; c++)
            {
                if (path[c] == '/')
                {
                    if (length == 0)
                    {
                        continue;
                    }

                    splits.Add((c - length, length));
                    length = 0;
                    continue;
                }

                length++;
            }
            if (length > 0)
            {
                splits.Add((path.Length - length, length));
            }

            // Collapse the path
            length = pathStartsWithSlash ? 1 : 0;
            List<(int, int)> segments = new List<(int, int)>();
            for (int c = 0; c < splits.Count; c++)
            {
                ReadOnlySpan<char> segment = path.Slice(splits[c].Item1, splits[c].Item2);

                // Crawl up, but only if we have a segment to pop, otherwise preserve the ".."
                // Also don't pop a ".." or a "." (which would only appear in the first segment)
                if (segments.Count > 0)
                {
                    ReadOnlySpan<char> last = path.Slice(segments[segments.Count - 1].Item1, segments[segments.Count - 1].Item2);

                    if (
                        segment.Equals(DotDot.AsSpan(), StringComparison.OrdinalIgnoreCase)
                        && !last.Equals(DotDot.AsSpan(), StringComparison.OrdinalIgnoreCase)
                        && (c > 1 || !last.Equals(Dot.AsSpan(), StringComparison.OrdinalIgnoreCase))
                        && (segments.Count > 1 || !System.IO.Path.IsPathRooted(last)))
                    {
                        length -= segments[segments.Count - 1].Item2;
                        segments.RemoveAt(segments.Count - 1);
                        continue;
                    }
                }

                // If this is a ".", skip it unless it's the first segment
                if (segment.Equals(Dot.AsSpan(), StringComparison.OrdinalIgnoreCase)
                    && (c != 0 || pathStartsWithSlash))
                {
                    continue;
                }

                // Push this segment
                length += splits[c].Item2;
                segments.Add(splits[c]);
            }

            // If there's nothing in the stack, figure out if we started with a slash
            if (segments.Count == 0)
            {
                return pathStartsWithSlash
                    ? (Slash, new ReadOnlyMemory<char>[] { Slash.AsMemory() })
                    : (Dot, new ReadOnlyMemory<char>[] { Dot.AsMemory() });
            }

            // Combine the segments back into a string
            string fullPath = string.Create(length + (segments.Count - 1), path.ToArray(), (chars, pathChars) =>
            {
                int i = 0;
                for (int c = 0; c < segments.Count; c++)
                {
                    // Add a slash prefix if the path started with one
                    if (c != 0 || pathStartsWithSlash)
                    {
                        chars[i++] = '/';
                    }

                    // Copy character over to the path string
                    int offset = 0;
                    for (; offset < segments[c].Item2; offset++, i++)
                    {
                        chars[i] = pathChars[segments[c].Item1 + offset];
                    }

                    // Record the new start location and length so we can generate new slices for the final segments
                    segments[c] = (i - offset, segments[c].Item2);
                }
            });

            // Get memory slices and return
            ReadOnlyMemory<char> fullPathMemory = fullPath.AsMemory();
            ReadOnlyMemory<char>[] fullPathSegments = segments
                .Select(x => fullPathMemory.Slice(x.Item1, x.Item2))
                .ToArray();
            return (fullPath, fullPathSegments);
        }

        /// <summary>
        /// Gets the full path.
        /// </summary>
        /// <value>The full path.</value>
        public string FullPath { get; }

        /// <summary>
        /// Gets a value indicating whether this path is relative.
        /// </summary>
        /// <value>
        /// <c>true</c> if this path is relative; otherwise, <c>false</c>.
        /// </value>
        public bool IsRelative => !IsAbsolute;

        /// <summary>
        /// Gets or sets a value indicating whether this path is absolute.
        /// </summary>
        /// <value>
        /// <c>true</c> if this path is absolute; otherwise, <c>false</c>.
        /// </value>
        public bool IsAbsolute { get; }

        /// <summary>
        /// Gets the segments making up the path. These are slices of the
        /// <see cref="FullPath"/> and can be converted to either
        /// <see cref="ReadOnlySpan{T}"/> or <see cref="string"/> as needed.
        /// This does not include directory separator characters
        /// or the leading slash if there is one.
        /// </summary>
        /// <remarks>
        /// Be careful when comparing segments or sequences of segments.
        /// The default equality comparison behavior for
        /// <see cref="ReadOnlyMemory{T}"/> is to compare the reference
        /// equality of the underlying array, not the value equality
        /// of the items in the memory. If you want to compare two
        /// segments, use <see cref="MemoryExtensions.SequenceEqual(ReadOnlyMemory{char}, ReadOnlyMemory{char})"/>.
        /// </remarks>
        /// <value>The segments making up the path.</value>
        public ReadOnlyMemory<char>[] Segments { get; }

        /// <summary>
        /// Gets the root of this path or "." if this is a relative path
        /// or there is no root.
        /// </summary>
        /// <value>
        /// The root of this path.
        /// </value>
        public DirectoryPath Root
        {
            get
            {
                string directory = IsAbsolute ? System.IO.Path.GetPathRoot(FullPath) : Dot;
                if (string.IsNullOrWhiteSpace(directory))
                {
                    directory = Dot;
                }
                return new DirectoryPath(directory);
            }
        }

        /// <summary>
        /// Returns a <see cref="string" /> that represents this path.
        /// </summary>
        /// <returns>
        /// A <see cref="string" /> that represents this instance.
        /// </returns>
        public override string ToString() => FullPath;

        /// <inheritdoc />
        public override int GetHashCode()
        {
            HashCode hash = default;
            hash.Add(IsAbsolute);
            hash.Add(FullPath.GetHashCode(PathComparisonType));
            return hash.ToHashCode();
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            NormalizedPath other = obj as NormalizedPath;

            // Special case for string, attempt to create like-typed path from the value
            if (other == null && obj is string path)
            {
                if (this is FilePath)
                {
                    other = new FilePath(path);
                }
                else if (this is DirectoryPath)
                {
                    other = new DirectoryPath(path);
                }
            }

            return other != null && Equals(other);
        }

        public bool Equals(NormalizedPath other) =>
            other != null
            && IsAbsolute == other.IsAbsolute
            && FullPath.Equals(other.FullPath, PathComparisonType);

        /// <inheritdoc />
        public int CompareTo(object obj) => !(obj is NormalizedPath path) ? 1 : CompareTo(path);

        /// <inheritdoc />
        public int CompareTo(NormalizedPath other)
        {
            if (other == null)
            {
                return 1;
            }

            if (GetType() != other.GetType())
            {
                throw new ArgumentException("Paths are not the same type");
            }

            int absoluteCompare = IsAbsolute.CompareTo(other.IsAbsolute);
            return absoluteCompare == 0
                ? string.Compare(FullPath, other.FullPath, PathComparisonType)
                : absoluteCompare;
        }
    }
}
