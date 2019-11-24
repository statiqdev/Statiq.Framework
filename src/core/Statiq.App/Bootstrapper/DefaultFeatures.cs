using System;

namespace Statiq.App
{
    [Flags]
    public enum DefaultFeatures
    {
        None = 0,
        BootstrapperConfigurators = 1 << 0,
        Logging = 1 << 1,
        Settings = 1 << 2,
        EnvironmentVariables = 1 << 3,
        ConfigurationFiles = 1 << 4,
        BuildCommands = 1 << 5,
        HostingCommands = 1 << 6,
        CustomCommands = 1 << 7,
        Shortcodes = 1 << 8,
        Namespaces = 1 << 9,
        Pipelines = 1 << 10,
        All =
            BootstrapperConfigurators
            | Logging
            | Settings
            | EnvironmentVariables
            | ConfigurationFiles
            | BuildCommands
            | HostingCommands
            | CustomCommands
            | Shortcodes
            | Namespaces
            | Pipelines
    }
}
