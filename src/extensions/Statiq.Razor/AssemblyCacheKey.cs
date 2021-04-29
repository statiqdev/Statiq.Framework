using System;

namespace Statiq.Razor
{
    /// <summary>
    /// A key for generated assemblies which is a JSON serializable combination
    /// of <see cref="CompilationParameters"/> and <see cref="CompilerCacheKey"/>.
    /// </summary>
    internal class AssemblyCacheKey : IEquatable<AssemblyCacheKey>
    {
        public static AssemblyCacheKey Get(CompilationParameters compilationParameters, CompilerCacheKey compilerCacheKey)
        {
            Common.CacheCode cacheCode = new Common.CacheCode();
            cacheCode.Add(compilationParameters.CacheCode);
            cacheCode.Add(compilerCacheKey.CacheCode);
            return new AssemblyCacheKey(compilationParameters, compilerCacheKey, cacheCode.ToCacheCode());
        }

        // Single parameterized constructor required for JSON deserialization
        public AssemblyCacheKey(CompilationParameters compilationParameters, CompilerCacheKey compilerCacheKey, int cacheCode)
        {
            CompilationParameters = compilationParameters;
            CompilerCacheKey = compilerCacheKey;
            CacheCode = cacheCode;
        }

        public CompilationParameters CompilationParameters { get; }

        public CompilerCacheKey CompilerCacheKey { get; }

        public int CacheCode { get; }

        public override int GetHashCode() => CacheCode;

        public override bool Equals(object obj) => Equals(obj as AssemblyCacheKey);

        public bool Equals(AssemblyCacheKey other)
        {
            if (other is null || other.CacheCode != CacheCode)
            {
                return false;
            }
            return CompilationParameters.Equals(other.CompilationParameters)
                && CompilerCacheKey.Equals(other.CompilerCacheKey);
        }
    }
}