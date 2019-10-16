using System;

namespace Statiq.App
{
    [Flags]
    public enum DefaultsToAdd
    {
        None = 0,
        BootstrapperConfigurators = 1 << 0,
        Logging = 1 << 1,
        Settings = 1 << 2,
        EnvironmentVariables = 1 << 3,
        Configuration = 1 << 4,
        Commands = 1 << 5,
        Shortcodes = 1 << 6,
        Namespaces = 1 << 7,
        Pipelines = 1 << 8,
        All =
            BootstrapperConfigurators
            | Logging
            | Settings
            | EnvironmentVariables
            | Configuration
            | Commands
            | Shortcodes
            | Namespaces
            | Pipelines
    }
}
