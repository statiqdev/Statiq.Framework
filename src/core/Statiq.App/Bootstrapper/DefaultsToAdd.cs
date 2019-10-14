using System;

namespace Statiq.App
{
    [Flags]
    public enum DefaultsToAdd
    {
        None = 0,
        BootstrapperConfigurators = 1 << 0,
        Logging = 1 << 1,
        EnvironmentVariables = 1 << 2,
        Configuration = 1 << 3,
        Commands = 1 << 4,
        Shortcodes = 1 << 5,
        Namespaces = 1 << 6,
        Pipelines = 1 << 7,
        All = BootstrapperConfigurators | Logging | Configuration | EnvironmentVariables | Commands | Shortcodes | Namespaces | Pipelines
    }
}
