using System;
using System.Linq;
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
        private readonly RenderRequest _request;
        private readonly int _fileHash;
        private readonly int _cacheCode;

        public CompilerCacheKey(RenderRequest request, int fileHash)
        {
            _request = request;
            _fileHash = fileHash;

            // Precalculate the cache code since we know we'll need it
            CacheCode cacheCode = default;
            cacheCode.Add(_request.LayoutLocation);
            cacheCode.Add(_request.ViewStartLocation);
            cacheCode.Add(fileHash);
            _cacheCode = cacheCode.ToCacheCode();
        }

        public override int GetHashCode() => _cacheCode;

        public override bool Equals(object obj) => Equals(obj as CompilerCacheKey);

        public bool Equals(CompilerCacheKey other)
        {
            if (other is null || other._cacheCode != _cacheCode)
            {
                return false;
            }
            return _request.LayoutLocation == other._request.LayoutLocation
                && _request.ViewStartLocation == other._request.ViewStartLocation
                && _fileHash == other._fileHash;
        }
    }
}