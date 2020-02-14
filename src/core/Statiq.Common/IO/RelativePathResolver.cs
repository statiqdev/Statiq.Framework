using System;
using System.Collections.Generic;

namespace Statiq.Common
{
    internal static class RelativePathResolver
    {
        public static NewNormalizedPath Resolve(NewNormalizedPath source, NewNormalizedPath target)
        {
            if (source.IsNull)
            {
                throw new ArgumentNullException(nameof(source));
            }
            if (target.IsNull)
            {
                throw new ArgumentNullException(nameof(target));
            }

            // Make sure they're both either relative or absolute
            if (source.IsAbsolute != target.IsAbsolute)
            {
                throw new ArgumentException("Paths must both be relative or both be absolute");
            }

            // Check if they're the same path
            if (source.FullPath == target.FullPath)
            {
                return NewNormalizedPath.EmptyPath;
            }

            // Special case if source is just root
            if (source.IsAbsolute && source.Segments.Length == 0)
            {
                return new NewNormalizedPath(string.Join(NewNormalizedPath.Slash, target.Segments));
            }

            // Check if they share the same root
            if (target.Segments.Length == 0 || !source.Segments[0].SequenceEqual(target.Segments[0]))
            {
                return target;
            }

            int minimumSegmentsLength = Math.Min(source.Segments.Length, target.Segments.Length);

            int lastCommonRoot = -1;

            // Find common root
            for (int x = 0; x < minimumSegmentsLength; x++)
            {
                if (!source.Segments[x].SequenceEqual(target.Segments[x]))
                {
                    break;
                }

                lastCommonRoot = x;
            }

            if (lastCommonRoot == -1)
            {
                return target;
            }

            // Add relative folders in from path
            List<ReadOnlyMemory<char>> relativeSegments = new List<ReadOnlyMemory<char>>();
            for (int x = lastCommonRoot + 1; x < source.Segments.Length; x++)
            {
                if (source.Segments[x].Length > 0)
                {
                    relativeSegments.Add(NormalizedPath.DotDot.AsMemory());
                }
            }

            // Add to folders to path
            for (int x = lastCommonRoot + 1; x < target.Segments.Length; x++)
            {
                relativeSegments.Add(target.Segments[x]);
            }

            // Create relative path
            return new NewNormalizedPath(string.Join("/", relativeSegments));
        }

        public static FilePath Resolve(DirectoryPath source, FilePath target)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            return Resolve(source, target.Directory).GetFilePath(target.FileName);
        }

        public static DirectoryPath Resolve(DirectoryPath source, DirectoryPath target)
        {
            _ = source ?? throw new ArgumentNullException(nameof(source));
            _ = target ?? throw new ArgumentNullException(nameof(target));

            // Make sure they're both either relative or absolute
            if (source.IsAbsolute != target.IsAbsolute)
            {
                throw new ArgumentException("Paths must both be relative or both be absolute");
            }

            // Check if they're the same path
            if (source.FullPath == target.FullPath)
            {
                return new DirectoryPath(".");
            }

            // Special case if source is just root
            if (source.IsAbsolute && source.Segments.Length == 0)
            {
                return new DirectoryPath(string.Join("/", target.Segments));
            }

            // Check if they share the same root
            if (target.Segments.Length == 0 || !source.Segments[0].SequenceEqual(target.Segments[0]))
            {
                return target;
            }

            int minimumSegmentsLength = Math.Min(source.Segments.Length, target.Segments.Length);

            int lastCommonRoot = -1;

            // Find common root
            for (int x = 0; x < minimumSegmentsLength; x++)
            {
                if (!source.Segments[x].SequenceEqual(target.Segments[x]))
                {
                    break;
                }

                lastCommonRoot = x;
            }

            if (lastCommonRoot == -1)
            {
                return target;
            }

            // Add relative folders in from path
            List<ReadOnlyMemory<char>> relativeSegments = new List<ReadOnlyMemory<char>>();
            for (int x = lastCommonRoot + 1; x < source.Segments.Length; x++)
            {
                if (source.Segments[x].Length > 0)
                {
                    relativeSegments.Add(NormalizedPath.DotDot.AsMemory());
                }
            }

            // Add to folders to path
            for (int x = lastCommonRoot + 1; x < target.Segments.Length; x++)
            {
                relativeSegments.Add(target.Segments[x]);
            }

            // Create relative path
            return new DirectoryPath(string.Join("/", relativeSegments));
        }
    }
}
