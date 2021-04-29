using System;
using Statiq.Common;

namespace Statiq.Razor
{
    /// <summary>
    /// Used as a key for the Razor compiler cache to persist page compilations from one generation to the next.
    /// A composite of layout location, view start location, and file hash. Note that file path is not included
    /// so that documents with the same content (or not content) and with the same layout and view start can
    /// use the same cached compilation.
    /// </summary>
    internal class CompilerCacheKey : IEquatable<CompilerCacheKey>
    {
        public static CompilerCacheKey Get(RenderRequest request, int contentCacheCode)
        {
            CacheCode cacheCode = new CacheCode();
            cacheCode.Add(request.LayoutLocation);
            cacheCode.Add(request.ViewStartLocation);
            cacheCode.Add(contentCacheCode);
            return new CompilerCacheKey(request.LayoutLocation, request.ViewStartLocation, contentCacheCode, cacheCode.ToCacheCode());
        }

        // Single parameterized constructor required for JSON deserialization
        public CompilerCacheKey(string layoutLocation, string viewStartLocation, int contentCacheCode, int cacheCode)
        {
            LayoutLocation = layoutLocation;
            ViewStartLocation = viewStartLocation;
            ContentCacheCode = contentCacheCode;
            CacheCode = cacheCode;
        }

        public string LayoutLocation { get; }

        public string ViewStartLocation { get; }

        public int ContentCacheCode { get; }

        public int CacheCode { get; }

        public override int GetHashCode() => CacheCode;

        public override bool Equals(object obj) => Equals(obj as CompilerCacheKey);

        public bool Equals(CompilerCacheKey other)
        {
            if (other is null || other.CacheCode != CacheCode)
            {
                return false;
            }
            return LayoutLocation == other.LayoutLocation
                && ViewStartLocation == other.ViewStartLocation
                && ContentCacheCode == other.ContentCacheCode;
        }
    }
}