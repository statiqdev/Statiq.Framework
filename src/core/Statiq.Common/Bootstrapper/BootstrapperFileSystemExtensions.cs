using System;

namespace Statiq.Common
{
    public static class BootstrapperFileSystemExtensions
    {
        public static TBootstrapper ConfigureFileSystem<TBootstrapper>(this TBootstrapper bootstrapper, Action<IFileSystem> action)
            where TBootstrapper : IBootstrapper
        {
            bootstrapper.ThrowIfNull(nameof(bootstrapper));
            action.ThrowIfNull(nameof(action));
            bootstrapper.ConfigureEngine(engine => action(engine.FileSystem));
            return bootstrapper;
        }

        public static TBootstrapper AddInputPath<TBootstrapper>(this TBootstrapper bootstrapper, NormalizedPath inputPath)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.ConfigureFileSystem(x => x.InputPaths.Add(inputPath));

        public static TBootstrapper AddMappedInputPath<TBootstrapper>(this TBootstrapper bootstrapper, NormalizedPath inputPath, NormalizedPath virtualPath)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.ConfigureFileSystem(x =>
            {
                x.InputPaths.Add(inputPath);
                x.InputPathMappings.Add(inputPath, virtualPath);
            });

        public static TBootstrapper AddExcludedPath<TBootstrapper>(this TBootstrapper bootstrapper, NormalizedPath excludedPath)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.ConfigureFileSystem(x => x.ExcludedPaths.Add(excludedPath));
    }
}
