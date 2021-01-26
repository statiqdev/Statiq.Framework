using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;

namespace Statiq.Common
{
    /// <summary>
    /// Provides properties and instance methods for working with paths.
    /// </summary>
    public readonly struct NormalizedPath : IDisplayable, IComparable<NormalizedPath>, IComparable, IEquatable<NormalizedPath>
    {
        // Initially based on code from Cake (http://cakebuild.net/)

        /// <summary>
        /// The type of string comparison to perform when comparing paths.
        /// </summary>
        /// <remarks>
        /// This defaults to <see cref="StringComparison.Ordinal"/> on Linux platforms and
        /// <see cref="StringComparison.OrdinalIgnoreCase"/> on Windows and MacOS.
        /// </remarks>
        public static StringComparison DefaultComparisonType { get; set; } =
            RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;

        public const string Dot = ".";
        public const string DotDot = "..";
        public const string Slash = "/";

        private const string WhitespaceChars = "\r\n\t";

        // Cache string-based paths so we don't have to recalculate them
        private static readonly ConcurrentCache<(string, PathKind), PathData> _cache = new ConcurrentCache<(string, PathKind), PathData>();

        private static readonly char[] InvalidPathChars = Path.GetInvalidPathChars();

        private static readonly PathData NullData = new PathData(null, null, false);

        private static readonly PathData EmptyData = new PathData(string.Empty, Array.Empty<ReadOnlyMemory<char>>(), false);

        private static readonly PathData SlashData = new PathData(Slash, Array.Empty<ReadOnlyMemory<char>>(), true);

        private static readonly PathData DotData = new PathData(Dot, false);

        private static readonly PathData DotDotData = new PathData(DotDot, false);

        public static readonly NormalizedPath Null = new NormalizedPath(NullData);

        public static readonly NormalizedPath Current = new NormalizedPath(Dot);

        public static readonly NormalizedPath Up = new NormalizedPath(DotDot);

        public static readonly NormalizedPath AbsoluteRoot = new NormalizedPath(Slash);

        public static readonly NormalizedPath Empty = new NormalizedPath(string.Empty);

        private readonly PathData _data;

        private readonly struct PathData
        {
            public PathData(string fullPath, bool isAbsolute)
                : this(fullPath, new ReadOnlyMemory<char>[] { fullPath.AsMemory() }, isAbsolute)
            {
            }

            public PathData(string fullPath, ReadOnlyMemory<char>[] segments, bool isAbsolute)
            {
                Segments = segments;
                IsAbsolute = isAbsolute;

                FullPath = fullPath;

                // Cache the parent since it's expensive
                Parent = new Lazy<NormalizedPath>(
                () =>
                {
                    string directory = Path.GetDirectoryName(fullPath);
                    if (directory.IsNullOrWhiteSpace())
                    {
                        return isAbsolute ? Null : Empty;
                    }
                    return new NormalizedPath(directory);
                },
                LazyThreadSafetyMode.ExecutionAndPublication);
            }

            /// <summary>
            /// The full path as a string. Attempts were made at storing the raw ReadOnlyMemory version
            /// of the full path and only converting to a string lazily, but every path ended up converting
            /// to a string at some point so might as well get it over with and store it directly.
            /// </summary>
            public readonly string FullPath { get; }

            public readonly ReadOnlyMemory<char>[] Segments { get; }

            public readonly bool IsAbsolute { get; }

            public readonly Lazy<NormalizedPath> Parent { get; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NormalizedPath" /> class.
        /// </summary>
        /// <param name="path">The path.</param>
        public NormalizedPath(string path)
            : this(path, PathKind.RelativeOrAbsolute)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NormalizedPath" /> class.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="pathKind">Specifies whether the path is relative, absolute, or indeterminate.</param>
        public NormalizedPath(string path, PathKind pathKind)
            : this(_cache.GetOrAdd((path.ThrowIfNull(nameof(path)), pathKind), data => NormalizePath(data.Item1, data.Item2)))
        {
        }

        private NormalizedPath(PathData data)
        {
            _data = data;
        }

        private static PathData NormalizePath(string path, PathKind pathKind)
        {
            // Checks for known paths
            if (path.Length == 0)
            {
                if (pathKind == PathKind.Absolute)
                {
                    throw new ArgumentException("An empty path cannot be absolute");
                }
                return EmptyData;
            }
            if (path == Slash || path == "\\")
            {
                if (pathKind == PathKind.Relative)
                {
                    throw new ArgumentException("An absolute root path cannot be relative");
                }
                return SlashData;
            }
            if (path == Dot)
            {
                if (pathKind == PathKind.Absolute)
                {
                    throw new ArgumentException("A dotted relative path cannot be absolute");
                }
                return DotData;
            }
            if (path == DotDot)
            {
                if (pathKind == PathKind.Absolute)
                {
                    throw new ArgumentException("A dotted relative path cannot be absolute");
                }
                return DotDotData;
            }

            // Normalize slashes, remove invalid chars, and trim whitespace
            Memory<char> memory = path.ToMemory();
            memory.Replace('\\', '/');
            memory = memory.Trim(WhitespaceChars.AsSpan());
            memory.Replace(InvalidPathChars, '-');

            // Remove relative part of a path, but only if it's not the only part
            if (memory.Length > 2 && memory.Span[0] == '.' && memory.Span[1] == '/')
            {
                memory = memory[2..];
            }

            // Remove trailing slashes (as long as this isn't just a slash)
            if (memory.Length > 1)
            {
                memory = memory.TrimEnd('/');
            }

            // Add a backslash if this is a windows root (I.e., ends with :)
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                && memory.Length > 1
                && memory.Span[^1] == ':')
            {
                memory = memory.Append('/');
            }

            // Determine if the path is absolute
            bool isAbsolute = pathKind switch
            {
                PathKind.RelativeOrAbsolute => Path.IsPathRooted(memory.Span),
                PathKind.Absolute => true,
                PathKind.Relative => false,
                _ => false,
            };

            // If the path contains no slashes, use it wholesale
            if (!memory.Span.Contains('/'))
            {
                return new PathData(memory.ToString(), isAbsolute);
            }

            // If the path is only one character, we can return special segments
            if (memory.Length == 1)
            {
                if (memory.Span[0] == '.')
                {
                    return DotData;
                }
                if (memory.Span[0] == '/')
                {
                    // Segments should be empty if the path is just a slash
                    return SlashData;
                }
                return new PathData(memory.ToString(), isAbsolute);
            }

            // Special case if path is a windows drive
            // (it will always have a trailing slash because that got added earlier)
            // The segment should not have the trailing slash
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                && memory.Length > 2
                && memory.Span[^2] == ':'
                && memory.Span[^1] == '/')
            {
                return new PathData(memory.ToString(), new ReadOnlyMemory<char>[] { memory[0..^1] }, isAbsolute);
            }

            // Collapse the path by crawling up ".." and removing "." and copying into a new span
            int currentLength = 0;
            bool startsWithSlash = memory.Span[0] == '/';
            for (int c = startsWithSlash ? 1 : 0; c < memory.Length; c++)
            {
                if (memory.Span[c] == '/')
                {
                    if (currentLength == 0)
                    {
                        // If this is a double slash, remove it
                        memory = memory.Remove(c, 1);
                        c--;
                    }
                    else
                    {
                        // Otherwise this marks a segment
                        (int removeStartIndex, int removeLength) = ProcessSegment(ref memory, c - currentLength, currentLength);
                        if (removeLength > 0)
                        {
                            memory = memory.Remove(removeStartIndex, removeLength);
                            c -= removeLength;
                        }
                        currentLength = 0;
                    }
                }
                else
                {
                    currentLength++;
                }
            }
            if (currentLength > 0)
            {
                // There is a final segment
                (int removeStartIndex, int removeLength) = ProcessSegment(ref memory, memory.Length - currentLength, currentLength);
                if (removeLength > 0)
                {
                    memory = memory.Remove(removeStartIndex, removeLength);
                }
            }

            // Remove the initial slash if we didn't originally start with one
            if (memory.Length > 0 && memory.Span[0] == '/' && !startsWithSlash)
            {
                memory = memory[1..];
            }

            // Returns the number of characters to backtrack or 0 to keep going
            static (int RemoveStartIndex, int RemoveLength) ProcessSegment(ref Memory<char> pathMemory, int start, int length)
            {
                Span<char> pathSpan = pathMemory.Span;

                // If this is a "..", remove this and the previous segment, but only if this isn't following an initial "./", "../", "/./", or "/../"
                if (start > 1
                    && length == 2
                    && pathSpan[start] == '.'
                    && pathSpan[start + 1] == '.'
                    && !(start - 2 >= 0 && pathSpan[start - 2] == '.' && pathSpan[start - 1] == '/')
                    && !(start - 3 >= 0 && pathSpan[start - 3] == '.' && pathSpan[start - 2] == '.' && pathSpan[start - 1] == '/')
                    && !(start - 3 >= 0 && pathSpan[start - 3] == '/' && pathSpan[start - 2] == '.' && pathSpan[start - 1] == '/')
                    && !(start - 4 >= 0 && pathSpan[start - 4] == '/' && pathSpan[start - 3] == '.' && pathSpan[start - 2] == '.' && pathSpan[start - 1] == '/')
                    && !(start - 3 == 0 && pathSpan[0] != '/' && Path.IsPathRooted(pathSpan.Slice(start - 3, 3))))
                {
                    int removeStart = start - 2;
                    while (removeStart > 0 && pathSpan[removeStart] != '/')
                    {
                        removeStart--;
                    }
                    return (removeStart, start - removeStart + length);
                }

                // If this is a ".", remove it unless it's the first segment
                if (start > 0
                    && length == 1
                    && pathSpan[start] == '.')
                {
                    // Special case for initial "/." without anything else
                    if (start == 1 && pathSpan[0] == '/' && pathSpan.Length == 2)
                    {
                        return (1, 1);
                    }

                    // Otherwise remove the "." and preceding "/"
                    return (start - 1, 2);
                }

                return default;
            }

            // Do one more check if it's a windows root
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                && memory.Length > 1
                && memory.Span[^1] == ':')
            {
                // Windows path without trailing slash so add one (but not to the segments)
                memory = memory.Append('/');
                return new PathData(memory.ToString(), new ReadOnlyMemory<char>[] { memory[0..^1] }, isAbsolute);
            }
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                && memory.Length > 2
                && memory.Span[^1] == '/'
                && memory.Span[^2] == ':')
            {
                // Windows path with trailing slash so remove from the segment
                return new PathData(memory.ToString(), new ReadOnlyMemory<char>[] { memory[0..^1] }, isAbsolute);
            }

            // One last check to make sure this isn't a short path
            if (memory.Length == 0 || (memory.Length == 1 && memory.Span[0] == '/'))
            {
                return new PathData(memory.ToString(), new ReadOnlyMemory<char>[] { }, isAbsolute);
            }

            // Get the segment count
            int count = 0;
            for (int c = 1; c < memory.Length; c++)
            {
                if (memory.Span[c] == '/')
                {
                    count++;
                }
            }
            if (memory.Span[^1] != '/')
            {
                count++;
            }
            int length = 0;
            int index = 0;

            // Slice the segments
            ReadOnlyMemory<char>[] segments = new ReadOnlyMemory<char>[count];
            for (int c = 0; c < memory.Length; c++)
            {
                if (memory.Span[c] == '/')
                {
                    if (length == 0)
                    {
                        continue;
                    }

                    segments[index++] = memory.Slice(c - length, length);
                    length = 0;
                }
                else
                {
                    length++;
                }
            }
            if (length > 0)
            {
                segments[index] = memory.Slice(memory.Length - length, length);
            }

            return new PathData(memory.ToString(), segments, isAbsolute);
        }

        /// <summary>
        /// Gets the full path as a string.
        /// </summary>
        /// <value>The full path.</value>
        public string FullPath => _data.FullPath;

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
        public bool IsAbsolute => _data.IsAbsolute;

        /// <summary>
        /// Indicates if this is a null path.
        /// </summary>
        public bool IsNull => FullPath is null;

        /// <summary>
        /// Indicates if this is a null or empty path.
        /// </summary>
        public bool IsNullOrEmpty => IsNull || FullPath.Length == 0;

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
        /// segments, <c>SequenceEqual</c>.
        /// </remarks>
        /// <value>The segments making up the path.</value>
        public ReadOnlyMemory<char>[] Segments => _data.Segments;

        /// <inheritdoc />
        public string ToDisplayString() => IsNull ? string.Empty : FullPath;

        /// <summary>
        /// Returns a <see cref="string" /> that represents this path.
        /// </summary>
        /// <returns>
        /// A <see cref="string" /> that represents this path.
        /// </returns>
        public override string ToString() => FullPath;

        /// <inheritdoc />
        public override int GetHashCode() => GetHashCode(DefaultComparisonType);

        public int GetHashCode(StringComparison comparisonType)
        {
            if (IsNull)
            {
                return 0;
            }
            HashCode hash = default;
            hash.Add(IsAbsolute);
            hash.Add(FullPath.GetHashCode(comparisonType));
            return hash.ToHashCode();
        }

        /// <inheritdoc />
        public override bool Equals(object obj) => Equals(obj, DefaultComparisonType);

        public bool Equals(object obj, StringComparison comparisonType) =>
            obj is NormalizedPath path && Equals(path, comparisonType);

        public bool Equals(NormalizedPath other) => Equals(other, DefaultComparisonType);

        public bool Equals(in NormalizedPath other, StringComparison comparisonType)
        {
            if (other.IsNull)
            {
                return IsNull;
            }
            if (IsNull)
            {
                return false;
            }
            return IsAbsolute == other.IsAbsolute
                && FullPath.Equals(other.FullPath, comparisonType);
        }

        public static bool operator ==(in NormalizedPath a, in NormalizedPath b) => a.Equals(b);

        public static bool operator !=(in NormalizedPath a, in NormalizedPath b) => !a.Equals(b);

        public static bool operator ==(in NormalizedPath a, object b) => a.Equals(b);

        public static bool operator !=(in NormalizedPath a, object b) => !a.Equals(b);

        /// <inheritdoc />
        public int CompareTo(object obj) => !(obj is NormalizedPath path) ? 1 : CompareTo(path);

        /// <inheritdoc />
        public int CompareTo(NormalizedPath other)
        {
            if (other.IsNull)
            {
                return IsNull ? 0 : 1;
            }
            int absoluteCompare = IsAbsolute.CompareTo(other.IsAbsolute);
            return absoluteCompare == 0
                ? string.Compare(FullPath, other.FullPath, DefaultComparisonType)
                : absoluteCompare;
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="string"/> to <see cref="NormalizedPath"/>.
        /// </summary>
        /// <param name="path">The path as a string.</param>
        /// <returns>The result of the conversion.</returns>
        public static implicit operator NormalizedPath(string path) => path is null ? Null : new NormalizedPath(path);

        /// <summary>
        /// Performs an explicit conversion from <see cref="NormalizedPath"/> to <see cref="string"/>.
        /// </summary>
        /// <remarks>
        /// No implicit conversion to string on purpose, don't want bugs due to choosing string overloads over what should be paths
        /// </remarks>
        /// <param name="path">The path.</param>
        /// <returns>The result as a string of the conversion.</returns>
        public static explicit operator string(in NormalizedPath path) => path.ToString();

        /// <summary>
        /// Combines two paths into a new path.
        /// </summary>
        /// <param name="path1">The first path to combine.</param>
        /// <param name="path2">The second path to combine.</param>
        /// <returns>The combined paths.</returns>
        public static NormalizedPath Combine(in NormalizedPath path1, in NormalizedPath path2)
        {
            path1.ThrowIfNull(nameof(path1));
            path2.ThrowIfNull(nameof(path2));

            if (path1.IsNullOrEmpty && path2.IsNullOrEmpty)
            {
                return Empty;
            }

            // Return the right path if absolute
            if (path2.IsAbsolute)
            {
                return path2;
            }

            // Return the other path if one of them is empty
            if (path1.IsNullOrEmpty)
            {
                return path2;
            }
            if (path2.IsNullOrEmpty)
            {
                return path1;
            }

            return new NormalizedPath(string.Concat(path1.FullPath, Slash, path2.FullPath));
        }

        public static NormalizedPath Combine(in NormalizedPath path1, in NormalizedPath path2, in NormalizedPath path3) =>
            Combine(Combine(path1, path2), path3);

        public static NormalizedPath Combine(in NormalizedPath path1, in NormalizedPath path2, in NormalizedPath path3, in NormalizedPath path4) =>
            Combine(Combine(path1, path2), Combine(path3, path4));

        public static NormalizedPath Combine(params NormalizedPath[] paths)
        {
            NormalizedPath path = paths[0];
            for (int i = 1; i < paths.Length; i++)
            {
                path = Combine(path, paths[i]);
            }
            return path;
        }

        /// <summary>
        /// Combines the current path with another <see cref="NormalizedPath"/>.
        /// If the provided <see cref="NormalizedPath"/> is not relative, then it is returned.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>A combination of the current path and the provided <see cref="NormalizedPath"/>, unless
        /// the provided <see cref="NormalizedPath"/> is absolute in which case it is returned.</returns>
        public NormalizedPath Combine(in NormalizedPath path)
        {
            ThrowIfNull();
            path.ThrowIfNull(nameof(path));
            return Combine(this, path);
        }

        /// <summary>
        /// Implements the / operator to combine paths.
        /// </summary>
        /// <param name="path1">The first path to combine.</param>
        /// <param name="path2">The second path to combine.</param>
        /// <returns>The combined paths.</returns>
        public static NormalizedPath operator /(in NormalizedPath path1, in NormalizedPath path2) => Combine(path1, path2);

        // Helper properties and methods

        public NormalizedPath ThrowIfNull(string paramName) =>
            IsNull ? throw new ArgumentNullException(paramName) : this;

        private NormalizedPath ThrowIfNull() =>
            IsNull ? throw new NullReferenceException() : this;

        /// <summary>
        /// Gets the root of this path,
        /// <see cref="Empty"/> if this is a relative path,
        /// or <see cref="Null"/> if there is no root.
        /// </summary>
        /// <value>
        /// The root of this path.
        /// </value>
        public NormalizedPath Root
        {
            get
            {
                ThrowIfNull();

                if (IsAbsolute)
                {
                    string directory = Path.GetPathRoot(FullPath);
                    if (string.IsNullOrWhiteSpace(directory))
                    {
                        return Null;
                    }
                    return new NormalizedPath(directory);
                }
                return Empty;
            }
        }

        /// <summary>
        /// Gets the name of the directory or file.
        /// </summary>
        /// <value>The directory or file name.</value>
        /// <remarks>
        /// If this is passed a file path, it will return the file name.
        /// This is by-and-large equivalent to how DirectoryInfo handles this scenario.
        /// If we wanted to return the *actual* directory name, we'd need to pull in IFileSystem,
        /// and do various checks to make sure things exists.
        /// </remarks>
        public string Name
        {
            get
            {
                ThrowIfNull();

                return Segments.Length == 0 ? FullPath : Segments.Last().ToString();
            }
        }

        /// <summary>
        /// Gets the parent path or <see cref="Empty"/> if this is a relative path with no parent.
        /// </summary>
        /// <value>
        /// The parent path.
        /// </value>
        public NormalizedPath Parent
        {
            get
            {
                ThrowIfNull();
                return _data.Parent.Value;
            }
        }

        /// <summary>
        /// Gets current path relative to it's root. If this is already a relative
        /// path or there is no root path, this just returns the current path.
        /// </summary>
        /// <value>
        /// The current path relative to it's root.
        /// </value>
        public NormalizedPath RootRelative
        {
            get
            {
                ThrowIfNull();

                if (!IsAbsolute)
                {
                    return this;
                }
                NormalizedPath root = Root;
                if (root.IsNull)
                {
                    return root;
                }
                return root.FullPath == Dot
                    ? this
                    : new NormalizedPath(FullPath[root.FullPath.Length..], PathKind.Relative);
            }
        }

        /// <summary>
        /// Combines the current path with the file name of a provided path.
        /// </summary>
        /// <param name="path">The file name to append.</param>
        /// <returns>A combination of the current path and the file name of the provided path.</returns>
        public NormalizedPath GetFilePath(in NormalizedPath path)
        {
            ThrowIfNull();
            path.ThrowIfNull(nameof(path));

            return Combine(path.FileName);
        }

        /// <summary>
        /// Get the relative path to another path.
        /// </summary>
        /// <param name="target">The target path.</param>
        /// <returns>A <see cref="NormalizedPath"/>.</returns>
        public NormalizedPath GetRelativePath(in NormalizedPath target) => RelativePathResolver.Resolve(this, target);

        /// <summary>
        /// Checks if this path contains the specified path as a direct child.
        /// </summary>
        /// <param name="path">The path to check.</param>
        /// <returns><c>true</c> if the path contains this path as a child, <c>false</c> otherwise.</returns>
        public bool ContainsChild(in NormalizedPath path) => (Equals(path.Parent) && !Equals(path)) || Equals(Current);

        /// <summary>
        /// Checks if this path is the specified path or if this path contains the specified path as a direct child.
        /// </summary>
        /// <param name="path">The path to check.</param>
        /// <returns><c>true</c> if the path is this path or contains this path as a child, <c>false</c> otherwise.</returns>
        public bool ContainsChildOrSelf(in NormalizedPath path) => Equals(path) || ContainsChild(path);

        /// <summary>
        /// Checks if this path is a sibling of the specified path.
        /// </summary>
        /// <param name="path">The path to check.</param>
        /// <returns><c>true</c> if this path is a sibling of the specified path, <c>false</c> otherwise.</returns>
        public bool IsSibling(in NormalizedPath path) => (!Equals(path) && Parent.ContainsChild(path)) || Equals(Current);

        /// <summary>
        /// Checks if this path is the specified path or if this path is a sibling of the specified path.
        /// </summary>
        /// <param name="path">The path to check.</param>
        /// <returns><c>true</c> if the path is this path or is a sibling of the specified path, <c>false</c> otherwise.</returns>
        public bool IsSiblingOrSelf(in NormalizedPath path) => Equals(path) || IsSibling(path);

        /// <summary>
        /// Checks if this directory contains the specified directory as a descendant.
        /// </summary>
        /// <param name="path">The directory path to check.</param>
        /// <returns><c>true</c> if the directory contains this directory as a descendant, <c>false</c> otherwise.</returns>
        public bool ContainsDescendant(in NormalizedPath path)
        {
            if (IsNull || path.IsNull)
            {
                return false;
            }
            if (IsNullOrEmpty || Equals(Current))
            {
                return true;
            }
            NormalizedPath parent = path.Parent;
            return !parent.IsNullOrEmpty && parent.Segments.StartsWith(Segments);
        }

        /// <summary>
        /// Checks if this path is the specified path or if this directory contains the specified directory as a descendant.
        /// </summary>
        /// <param name="path">The directory path to check.</param>
        /// <returns><c>true</c> if the directory is this directory or contains this directory as a descendant, <c>false</c> otherwise.</returns>
        public bool ContainsDescendantOrSelf(in NormalizedPath path) => Equals(path) || ContainsDescendant(path);

        /// <summary>
        /// Gets a value indicating whether this path has a file extension.
        /// </summary>
        /// <value>
        /// <c>true</c> if this file path has a file extension; otherwise, <c>false</c>.
        /// </value>
        public bool HasExtension
        {
            get
            {
                ThrowIfNull();
                return Path.HasExtension(FullPath);
            }
        }

        /// <summary>
        /// Gets the filename.
        /// </summary>
        /// <value>The file name or <see cref="Empty"/> if the path contains no file name.</value>
        public NormalizedPath FileName
        {
            get
            {
                ThrowIfNull();
                if (_data.Segments.Length == 0)
                {
                    return Empty;
                }
                return new NormalizedPath(new PathData(_data.Segments[^1].ToString(), false));
            }
        }

        /// <summary>
        /// Gets the filename without it's extension.
        /// </summary>
        /// <value>The filename without it's extension or <see cref="Empty"/> if the path contains no file name.</value>
        public NormalizedPath FileNameWithoutExtension
        {
            get
            {
                ThrowIfNull();
                if (_data.Segments.Length == 0)
                {
                    return Empty;
                }

                // Find the last dot
                ReadOnlyMemory<char> fileName = _data.Segments[^1];
                int dot = -1;
                for (int c = 0; c < fileName.Length; c++)
                {
                    if (fileName.Span[c] == '.')
                    {
                        dot = c;
                    }
                }

                // Trim the last dot
                if (dot >= 0)
                {
                    fileName = fileName.Slice(0, dot);
                }

                return new NormalizedPath(new PathData(fileName.ToString(), false));
            }
        }

        /// <summary>
        /// Gets the file extension (including the preceding ".").
        /// </summary>
        /// <value>The file extension (including the preceding ".") or <see cref="Empty"/> if the path contains no extension.</value>
        public string Extension
        {
            get
            {
                ThrowIfNull();
                return Path.GetExtension(FullPath);
            }
        }

        /// <summary>
        /// Gets the media type of the path.
        /// </summary>
        /// <remarks>
        /// A registered IANA media type will be used if available.
        /// Unregistered media type names may be returned if a registered type is unavailable.
        /// If the media type is unknown this may be null or empty.
        /// </remarks>
        public string MediaType
        {
            get
            {
                ThrowIfNull();
                return MediaTypes.Get(Extension);
            }
        }

        /// <summary>
        /// Changes the file extension of the path.
        /// </summary>
        /// <remarks>
        /// Passing in <see cref="string.Empty"/> will result in an
        /// empty extension while retaining the <c>.</c>. Passing in
        /// null will result in removing the extension.
        /// </remarks>
        /// <param name="extension">The new extension.</param>
        /// <returns>A new path with the new extension.</returns>
        public NormalizedPath ChangeExtension(string extension)
        {
            ThrowIfNull();
            if (IsNullOrEmpty)
            {
                return extension.StartsWith('.')
                    ? new NormalizedPath(extension)
                    : new NormalizedPath("." + extension);
            }
            return new NormalizedPath(Path.ChangeExtension(FullPath, extension));
        }

        /// <summary>
        /// Changes the file name of the path by combining the specified path with the <see cref="Parent"/>.
        /// </summary>
        /// <param name="path">The path to combine with the <see cref="Parent"/>.</param>
        /// <returns>A new path with the specified path replacing the current file name.</returns>
        public NormalizedPath ChangeFileName(in NormalizedPath path)
        {
            ThrowIfNull();
            return Parent.Combine(path);
        }

        /// <summary>
        /// Appends a file extension to the path.
        /// </summary>
        /// <param name="extension">The extension.</param>
        /// <returns>A new <see cref="NormalizedPath"/> with an appended extension.</returns>
        public NormalizedPath AppendExtension(string extension)
        {
            extension.ThrowIfNull(nameof(extension));
            ThrowIfNull();

            if (!extension.StartsWith(Dot, StringComparison.OrdinalIgnoreCase))
            {
                extension = string.Concat(Dot, extension);
            }
            return new NormalizedPath(string.Concat(FullPath, extension));
        }

        /// <summary>
        /// Inserts a suffix into the file name before the extension.
        /// </summary>
        /// <param name="suffix">The suffix to insert.</param>
        /// <returns>A new <see cref="NormalizedPath"/> with the specified suffix.</returns>
        public NormalizedPath InsertSuffix(string suffix)
        {
            suffix.ThrowIfNull(nameof(suffix));
            ThrowIfNull();

            int extensionIndex = FullPath.LastIndexOf(Dot);
            return extensionIndex == -1
                ? new NormalizedPath(string.Concat(FullPath, suffix))
                : new NormalizedPath(string.Concat(FullPath.Substring(0, extensionIndex), suffix, FullPath.Substring(extensionIndex)));
        }

        /// <summary>
        /// Inserts a prefix into the file name.
        /// </summary>
        /// <param name="prefix">The prefix to insert.</param>
        /// <returns>A new <see cref="NormalizedPath"/> with the specified prefix.</returns>
        public NormalizedPath InsertPrefix(string prefix)
        {
            prefix.ThrowIfNull(nameof(prefix));
            ThrowIfNull();

            int nameIndex = FullPath.LastIndexOf(Slash);
            if (nameIndex == -1)
            {
                return new NormalizedPath(string.Concat(prefix, FullPath));
            }
            return new NormalizedPath(string.Concat(FullPath.Substring(0, nameIndex + 1), prefix, FullPath.Substring(nameIndex + 1)));
        }

        /// <summary>
        /// Gets a path to this file relative to it's containing input directory in the current file system.
        /// If no input directories contain this file, then a null path is returned.
        /// </summary>
        /// <returns>A path to this file relative to it's containing input directory in the current file system.</returns>
        public NormalizedPath GetRelativeInputPath() => IExecutionContext.Current.FileSystem.GetRelativeInputPath(this);

        /// <summary>
        /// Gets a path to this file relative to the output directory in the current file system.
        /// If this path is not relative to the output directory, then a null path is returned.
        /// </summary>
        /// <returns>A path to this file relative to the output directory in the current file system.</returns>
        public NormalizedPath GetRelativeOutputPath() => IExecutionContext.Current.FileSystem.GetRelativeOutputPath(this);

        private static readonly ReadOnlyMemory<char> IndexFileName = "index.".AsMemory();

        /// <summary>
        /// Gets a normalized title derived from the file path.
        /// </summary>
        /// <returns>A normalized title or null if the path is null.</returns>
        public string GetTitle()
        {
            if (IsNull)
            {
                return null;
            }

            // Get the filename, unless an index file, then get containing directory
            ReadOnlyMemory<char> titleMemory = Segments[^1];
            if (titleMemory.StartsWith(IndexFileName) && Segments.Length > 1)
            {
                titleMemory = Segments[^2];
            }

            // Strip the extension(s)
            int extensionIndex = titleMemory.Span.LastIndexOf('.');
            if (extensionIndex > 0)
            {
                titleMemory = titleMemory.Slice(0, extensionIndex);
            }

            // Decode URL escapes
            string title = WebUtility.UrlDecode(titleMemory.ToString());

            // Replace special characters with spaces
            title = title.Replace('-', ' ').Replace('_', ' ');

            // Join adjacent spaces
            while (title.IndexOf("  ", StringComparison.Ordinal) > 0)
            {
                title = title.Replace("  ", " ");
            }

            // Capitalize
            return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(title);
        }

        public static string ReplaceInvalidFileNameChars(string fileName, char newChar = '-')
        {
            Span<char> chars = fileName.ToSpan();
            return ReplaceInvalidFileNameChars(chars, newChar) ? new string(chars) : fileName;
        }

        public static bool ReplaceInvalidFileNameChars(in Span<char> fileName, char newChar = '-') =>
            fileName.Replace(Path.GetInvalidFileNameChars(), newChar);

        public static string ReplaceInvalidPathChars(string path, char newChar = '-')
        {
            Span<char> chars = path.ToSpan();
            return ReplaceInvalidPathChars(chars, newChar) ? new string(chars) : path;
        }

        public static bool ReplaceInvalidPathChars(in Span<char> path, char newChar = '-') =>
            path.Replace(Path.GetInvalidPathChars(), newChar);

        public const string OptimizeFileNameReservedChars = "_~:/\\?#[]@!$&'()*+;=};,";

        public static string OptimizeFileName(
            string fileName,
            string reservedChars = OptimizeFileNameReservedChars,
            bool trimDot = true,
            bool collapseSpaces = true,
            bool spacesToDashes = true,
            bool toLower = true)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return string.Empty;
            }

            // Trim whitespace
            fileName = fileName.Trim();

            // Strip reserved chars
            char[] buffer = new char[fileName.Length];
            int index = 0;
            foreach (char c in fileName)
            {
                if (reservedChars?.Contains(c) != true)
                {
                    buffer[index++] = c;
                }
            }
            fileName = new string(buffer, 0, index);

            // Trim dot (special case, only reserved if at beginning or end)
            if (trimDot)
            {
                fileName = fileName.Trim('.');
            }

            // Remove multiple spaces
            if (collapseSpaces)
            {
                fileName = Regex.Replace(fileName, @"\s+", " ");
            }

            // Turn spaces into dashes
            if (spacesToDashes)
            {
                fileName = fileName.Replace(" ", "-");
            }

            // Remove multiple dashes
            fileName = Regex.Replace(fileName, @"\-+", "-");

            // Convert to lower-case
            if (toLower)
            {
                fileName = fileName.ToLowerInvariant();
            }

            return fileName;
        }

        public NormalizedPath OptimizeFileName(
            string reservedChars = OptimizeFileNameReservedChars,
            bool trimDot = true,
            bool collapseSpaces = true,
            bool spacesToDashes = true,
            bool toLower = true) =>
            ChangeFileName(OptimizeFileName(FileName.FullPath, reservedChars, trimDot, collapseSpaces, spacesToDashes, toLower));
    }
}
