using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;

namespace Statiq.Common
{
    /// <summary>
    /// Provides properties and instance methods for working with paths.
    /// </summary>
    public struct NormalizedPath : IDisplayable, IComparable<NormalizedPath>, IComparable, IEquatable<NormalizedPath>
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

        public static readonly NormalizedPath Null;

        public static readonly NormalizedPath Current = new NormalizedPath(Dot);

        public static readonly NormalizedPath Up = new NormalizedPath(DotDot);

        public static readonly NormalizedPath AbsoluteRoot = new NormalizedPath(Slash);

        public static readonly NormalizedPath Empty = new NormalizedPath(string.Empty);

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
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            // Try known paths first
            if (path == string.Empty)
            {
                if (pathKind == PathKind.Absolute)
                {
                    throw new ArgumentException("An empty path cannot be absolute");
                }
                FullPath = string.Empty;
                Segments = new ReadOnlyMemory<char>[] { ReadOnlyMemory<char>.Empty };
                IsAbsolute = false;
            }
            else if (path == Slash || path == "\\")
            {
                if (pathKind == PathKind.Relative)
                {
                    throw new ArgumentException("An absolute root path cannot be relative");
                }
                FullPath = Slash;
                Segments = new ReadOnlyMemory<char>[] { Slash.AsMemory() };
                IsAbsolute = true;
            }
            else if (path == Dot || path == DotDot)
            {
                if (pathKind == PathKind.Absolute)
                {
                    throw new ArgumentException("A dotted relative path cannot be absolute");
                }
                FullPath = path;
                Segments = new ReadOnlyMemory<char>[] { path.AsMemory() };
                IsAbsolute = false;
            }
            else
            {
                ReadOnlySpan<char> fullPath = GetFullPath(path);
                IsAbsolute = GetIsAbsolute(pathKind, fullPath);
                (FullPath, Segments) = GetFullPathAndSegments(fullPath);
            }
        }

        private static ReadOnlySpan<char> GetFullPath(string path)
        {
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
        /// Indicates if this is a null path.
        /// </summary>
        public bool IsNull => FullPath == null;

        /// <summary>
        /// Indicates if this is an empty path.
        /// </summary>
        public bool IsEmpty => FullPath?.Length == 0;

        /// <summary>
        /// Indicates if this is a null or empty path.
        /// </summary>
        public bool IsNullOrEmpty => IsNull || IsEmpty;

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

        public bool Equals(NormalizedPath other, StringComparison comparisonType)
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

        public static bool operator ==(NormalizedPath a, NormalizedPath b) => a.Equals(b);

        public static bool operator !=(NormalizedPath a, NormalizedPath b) => !a.Equals(b);

        public static bool operator ==(NormalizedPath a, object b) => a.Equals(b);

        public static bool operator !=(NormalizedPath a, object b) => !a.Equals(b);

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
        public static implicit operator NormalizedPath(string path) => path == null ? NormalizedPath.Null : new NormalizedPath(path);

        /// <summary>
        /// Performs an explicit conversion from <see cref="NormalizedPath"/> to <see cref="string"/>.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>The result as a string of the conversion.</returns>
        public static implicit operator string(NormalizedPath path) => path.ToString();

        /// <summary>
        /// Combines two paths into a new path.
        /// </summary>
        /// <param name="path1">The first path to combine.</param>
        /// <param name="path2">The second path to combine.</param>
        /// <returns>The combined paths.</returns>
        public static NormalizedPath Combine(NormalizedPath path1, NormalizedPath path2)
        {
            path1.ThrowIfNull(nameof(path1));
            path2.ThrowIfNull(nameof(path2));

            if (path1.IsEmpty && path2.IsEmpty)
            {
                return Empty;
            }

            // Return the right path if absolute
            if (path2.IsAbsolute)
            {
                return path2;
            }

            return new NormalizedPath(System.IO.Path.Combine(path1.FullPath, path2.FullPath));
        }

        public static NormalizedPath Combine(NormalizedPath path1, NormalizedPath path2, NormalizedPath path3) =>
            Combine(Combine(path1, path2), path3);

        public static NormalizedPath Combine(NormalizedPath path1, NormalizedPath path2, NormalizedPath path3, NormalizedPath path4) =>
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
        public NormalizedPath Combine(NormalizedPath path)
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
        public static NormalizedPath operator /(NormalizedPath path1, NormalizedPath path2) => Combine(path1, path2);

        // Helper properties and methods

        public void ThrowIfNull(string paramName)
        {
            if (IsNull)
            {
                throw new ArgumentNullException(paramName);
            }
        }

        private void ThrowIfNull()
        {
            if (IsNull)
            {
                throw new NullReferenceException();
            }
        }

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
                    string directory = System.IO.Path.GetPathRoot(FullPath);
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

                string directory = System.IO.Path.GetDirectoryName(FullPath);
                if (string.IsNullOrWhiteSpace(directory))
                {
                    return IsAbsolute ? Null : Empty;
                }
                return new NormalizedPath(directory);
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
                    : new NormalizedPath(FullPath.Substring(root.FullPath.Length), PathKind.Relative);
            }
        }

        /// <summary>
        /// Combines the current path with the file name of a provided path.
        /// </summary>
        /// <param name="path">The file name to append.</param>
        /// <returns>A combination of the current path and the file name of the provided path.</returns>
        public NormalizedPath GetFilePath(NormalizedPath path)
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
        public NormalizedPath GetRelativePath(NormalizedPath target) => RelativePathResolver.Resolve(this, target);

        /// <summary>
        /// Checks if this path contains the specified path as a direct child.
        /// </summary>
        /// <param name="path">The path to check.</param>
        /// <returns><c>true</c> if the path is or contains this path, <c>false</c> otherwise.</returns>
        public bool ContainsChild(NormalizedPath path)
        {
            ThrowIfNull();
            path.ThrowIfNull(nameof(path));

            return Equals(path.Parent);
        }

        /// <summary>
        /// Checks if this directory contains the specified directory as a descendant.
        /// </summary>
        /// <param name="path">The directory path to check.</param>
        /// <returns><c>true</c> if the directory is or contains this directory, <c>false</c> otherwise.</returns>
        public bool ContainsDescendant(NormalizedPath path)
        {
            ThrowIfNull();
            path.ThrowIfNull(nameof(path));

            path = path.Parent;
            while (!path.IsNullOrEmpty)
            {
                if (Equals(path))
                {
                    return true;
                }
                path = path.Parent;
            }
            return false;
        }

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
                return System.IO.Path.HasExtension(FullPath);
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
                return new NormalizedPath(System.IO.Path.GetFileName(FullPath), PathKind.Relative);
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
                return new NormalizedPath(System.IO.Path.GetFileNameWithoutExtension(FullPath), PathKind.Relative);
            }
        }

        /// <summary>
        /// Gets the file extension.
        /// </summary>
        /// <value>The file extension (including the preceding ".") or <see cref="Empty"/> if the path contains no extension.</value>
        public string Extension
        {
            get
            {
                ThrowIfNull();
                return System.IO.Path.GetExtension(FullPath);
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
        /// <param name="extension">The new extension.</param>
        /// <returns>A new path with the new extension.</returns>
        public NormalizedPath ChangeExtension(string extension)
        {
            ThrowIfNull();
            if (IsEmpty)
            {
                return extension.StartsWith('.')
                    ? new NormalizedPath(extension)
                    : new NormalizedPath("." + extension);
            }
            return new NormalizedPath(System.IO.Path.ChangeExtension(FullPath, extension));
        }

        /// <summary>
        /// Changes the file name of the path by combining the specified path with the <see cref="Parent"/>.
        /// </summary>
        /// <param name="path">The path to combine with the <see cref="Parent"/>.</param>
        /// <returns>A new path with the specified path replacing the current file name.</returns>
        public NormalizedPath ChangeFileName(NormalizedPath path)
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
            _ = extension ?? throw new ArgumentNullException(nameof(extension));
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
            _ = suffix ?? throw new ArgumentNullException(nameof(suffix));
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
            _ = prefix ?? throw new ArgumentNullException(nameof(prefix));
            ThrowIfNull();

            int nameIndex = FullPath.LastIndexOf(Slash);
            if (nameIndex == -1)
            {
                return new NormalizedPath(string.Concat(prefix, FullPath));
            }
            return new NormalizedPath(string.Concat(FullPath.Substring(0, nameIndex + 1), prefix, FullPath.Substring(nameIndex + 1)));
        }

        /// <summary>
        /// Gets path to this file relative to it's containing input directory in the current file system.
        /// If no input directories contain this file, then the file name is returned.
        /// </summary>
        /// <param name="context">The current execution context.</param>
        /// <returns>A path to this file relative to it's containing input directory in the current file system.</returns>
        public NormalizedPath GetRelativeInputPath(IExecutionContext context)
        {
            ThrowIfNull();

            if (IsRelative)
            {
                return this;
            }

            NormalizedPath containingPath = context.FileSystem.GetContainingInputPathForAbsolutePath(this);
            return containingPath.IsNull ? FileName : containingPath.GetRelativePath(this);
        }

        /// <summary>
        /// Gets path to this file relative to the output directory in the current file system.
        /// If this path is not relative to the output directory, then the file name is returned.
        /// </summary>
        /// <param name="context">The current execution context.</param>
        /// <returns>A path to this file relative to it's containing input directory in the current file system.</returns>
        public NormalizedPath GetRelativeOutputPath(IExecutionContext context)
        {
            ThrowIfNull();

            if (IsRelative)
            {
                return this;
            }

            return Parent.Segments.StartsWith(context.FileSystem.OutputPath.Segments)
                ? context.FileSystem.OutputPath.GetRelativePath(this)
                : FileName;
        }

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
            int extensionIndex = titleMemory.Span.IndexOf('.');
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
    }
}
