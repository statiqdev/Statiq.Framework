using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Wyam.Common.Util;

namespace Wyam.Common.IO
{
    /// <summary>
    /// Provides properties and instance methods for working with paths.
    /// </summary>
    public abstract class NormalizedPath : IComparable<NormalizedPath>, IComparable, IEquatable<NormalizedPath>
    {
        // Initially based on code from Cake (http://cakebuild.net/)

        public const string FileProviderDelimiter = "|";
        public const string Dot = ".";
        public const string DotDot = "..";
        public const string Slash = "/";

        private const string WhitespaceChars = "\r\n\t";

        /// <summary>
        /// The default file provider.
        /// </summary>
        public static readonly Uri DefaultFileProvider = new Uri("file:///", UriKind.Absolute);

        /// <summary>
        /// Initializes a new instance of the <see cref="NormalizedPath" /> class.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="pathKind">Specifies whether the path is relative, absolute, or indeterminate.</param>
        protected NormalizedPath(string path, PathKind pathKind)
            : this(GetFileProviderAndPath(null, path), false, pathKind)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NormalizedPath" /> class
        /// with the given provider.
        /// </summary>
        /// <param name="fileProvider">The provider for this path.</param>
        /// <param name="path">The path.</param>
        /// <param name="pathKind">Specifies whether the path is relative, absolute, or indeterminate.</param>
        protected NormalizedPath(string fileProvider, string path, PathKind pathKind)
            : this(GetFileProviderUri(fileProvider), path, pathKind)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NormalizedPath" /> class
        /// with the given provider.
        /// </summary>
        /// <param name="fileProvider">The provider for this path.</param>
        /// <param name="path">The path.</param>
        /// <param name="pathKind">Specifies whether the path is relative, absolute, or indeterminate.</param>
        protected NormalizedPath(Uri fileProvider, string path, PathKind pathKind)
            : this(Tuple.Create(fileProvider, path), true, pathKind)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NormalizedPath" /> class. The new path
        /// will be absolute if the specified URI is absolute, otherwise it will be relative.
        /// </summary>
        /// <param name="path">The path as a URI.</param>
        protected NormalizedPath(Uri path)
            : this(GetFileProviderAndPath(path, null), false, path.IsAbsoluteUri ? PathKind.Absolute : PathKind.Relative)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NormalizedPath" /> class.
        /// </summary>
        /// <param name="providerAndPath">The provider and path as a Tuple so it can
        /// be passed from both of the other constructors.</param>
        /// <param name="fullySpecified">If set to <c>true</c> indicates that this constructor was
        /// called from one where the provider and path were fully specified (as opposed to being inferred).</param>
        /// <param name="pathKind">Specifies whether the path is relative, absolute, or indeterminate.</param>
        private NormalizedPath(Tuple<Uri, string> providerAndPath, bool fullySpecified, PathKind pathKind)
        {
            _ = providerAndPath ?? throw new ArgumentNullException(nameof(providerAndPath));
            _ = providerAndPath.Item2 ?? throw new ArgumentNullException(nameof(providerAndPath.Item2));

            ReadOnlySpan<char> fullPath = GetFullPath(providerAndPath.Item2);
            IsAbsolute = GetIsAbsolute(pathKind, fullPath);
            FileProvider = GetFileProvider(providerAndPath.Item1, IsAbsolute, fullySpecified);
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

        private static Uri GetFileProvider(Uri fileProvider, bool isAbsolute, bool fullySpecified)
        {
            if (!isAbsolute && fileProvider != null)
            {
                throw new ArgumentException("Can not specify provider for relative paths");
            }
            if (fileProvider == null && isAbsolute && !fullySpecified)
            {
                return DefaultFileProvider;
            }
            else if (fileProvider?.IsAbsoluteUri == false)
            {
                throw new ArgumentException("The provider URI must always be absolute");
            }

            return fileProvider;
        }

        // Internal for testing
        // Splits the path on /, collapses it, and then pools the segements
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
                        return (Slash, new ReadOnlyMemory<char>[] { Slash.AsMemory() });
                }

                string pathString = path.ToString();
                return (pathString, new ReadOnlyMemory<char>[] { pathString.AsMemory() });
            }

            // Special case if path is just a windows drive
            // (it will always have a trailing slash because that got added earlier)
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                && path.Length > 2
                && (path[path.Length - 1] == '/' && path[path.Length - 2] == ':'))
            {
                string pathString = path.ToString();
                return (pathString, new ReadOnlyMemory<char>[] { pathString.AsMemory() });
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

        // Internal for testing
        internal static Uri GetFileProviderUri(string provider)
        {
            if (string.IsNullOrEmpty(provider))
            {
                return null;
            }

            // The use of IsWellFormedOriginalString() weeds out cases where a file system path was implicitly converted to a URI
            if (!Uri.TryCreate(provider, UriKind.Absolute, out Uri uri) || !uri.IsWellFormedOriginalString())
            {
                // Couldn't create the provider as a URI, try it as just a scheme
                if (Uri.CheckSchemeName(provider))
                {
                    uri = new Uri($"{provider}:", UriKind.Absolute);
                }
                else
                {
                    throw new ArgumentException("The provider URI is not valid");
                }
            }
            return uri;
        }

        /// <summary>
        /// Gets the provider and path from a path string. Implemented as a static
        /// so it can be used in a constructor chain. Internal for testing.
        /// </summary>
        /// <param name="uriPath">The URI-based path.</param>
        /// <param name="stringPath">The string-based path.</param>
        /// <returns>The provider (item 1) and path (item 2).</returns>
        internal static Tuple<Uri, string> GetFileProviderAndPath(Uri uriPath, string stringPath)
        {
            if (uriPath != null && stringPath != null)
            {
                throw new ArgumentException($"{nameof(uriPath)} and {nameof(stringPath)} can not both have values");
            }

            // If we got a relative URI, then just use that as the path
            if (uriPath?.IsAbsoluteUri == false)
            {
                return new Tuple<Uri, string>(null, uriPath.ToString());
            }

            // Did we get a delimiter?
            string path = uriPath?.ToString() ?? stringPath ?? throw new ArgumentNullException(nameof(path));
            int delimiterIndex = path.IndexOf(FileProviderDelimiter, StringComparison.Ordinal);
            if (delimiterIndex != -1)
            {
                // Path contains a provider delimiter, try to parse the provider
                return Tuple.Create(
                    GetFileProviderUri(path.Substring(0, delimiterIndex)),
                    path.Length == delimiterIndex + 1 ? string.Empty : path.Substring(delimiterIndex + 1));
            }

            // See if the path is a URI and attempt to split it into left (provider) and right (path) parts
            // The use of IsWellFormedOriginalString() weeds out cases where a file system path was implicitly converted to a URI
            Uri fileProvider = uriPath;
            if (fileProvider != null || (Uri.TryCreate(stringPath, UriKind.Absolute, out fileProvider) && fileProvider.IsWellFormedOriginalString()))
            {
                // No delimiter, but the path itself is a URI
                // However, if there is no "right part" go back to treating the whole thing as a path
                string rightPart = GetRightPart(fileProvider);
                if (!string.IsNullOrEmpty(rightPart))
                {
                    return Tuple.Create(
                        new Uri(GetLeftPart(fileProvider), UriKind.Absolute),
                        rightPart);
                }
            }

            return Tuple.Create(uriPath, stringPath);
        }

        private static string GetLeftPart(Uri uri) =>
            uri.GetComponents(UriComponents.AbsoluteUri & ~UriComponents.PathAndQuery & ~UriComponents.Fragment, UriFormat.Unescaped);

        private static string GetRightPart(Uri uri) =>
            uri.GetComponents(UriComponents.PathAndQuery | UriComponents.Fragment, UriFormat.Unescaped);

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
        /// This does not include directory seperator characters
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
        /// Gets the file provider for this path. If this is a relative path,
        /// the file provider will always be <c>null</c>. If this is an absolute
        /// path and the file provider is <c>null</c> it indicates the path
        /// is not intended for use with an actual file provider.
        /// </summary>
        /// <value>
        /// The file provider for this path.
        /// </value>
        public Uri FileProvider { get; }

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
                return new DirectoryPath(FileProvider, directory);
            }
        }

        /// <summary>
        /// Returns a <see cref="string" /> that represents this path.
        /// </summary>
        /// <returns>
        /// A <see cref="string" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            if (IsRelative || FileProvider == null)
            {
                return FullPath;
            }
            string rightPart = GetRightPart(FileProvider);
            if (string.IsNullOrEmpty(rightPart) || rightPart == "/")
            {
                // Remove the proceeding slash from FullPath if the provider already has one
                return FileProvider + (rightPart == "/" && FullPath.StartsWith("/") ? FullPath.Substring(1) : FullPath);
            }
            return FileProvider + FileProviderDelimiter + FullPath;
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            HashCode hash = default;
            hash.Add(FileProvider?.GetHashCode() ?? 0);
            hash.Add(FullPath);
            return hash.ToHashCode();
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            NormalizedPath other = obj as NormalizedPath;

            // Special case for string, attempt to create like-typed path from the value
            string path = obj as string;
            if (other == null && path != null)
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

            return other != null && ((IEquatable<NormalizedPath>)this).Equals(other);
        }

        bool IEquatable<NormalizedPath>.Equals(NormalizedPath other) =>
            other != null
            && FileProvider?.ToString() == other.FileProvider?.ToString()
            && FullPath.Equals(other.FullPath, StringComparison.Ordinal);

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

            int providerCompare = string.Compare(FileProvider?.ToString(), other.FileProvider?.ToString(), StringComparison.Ordinal);
            return providerCompare == 0
                ? string.Compare(FullPath, other.FullPath, StringComparison.Ordinal)
                : providerCompare;
        }
    }
}
