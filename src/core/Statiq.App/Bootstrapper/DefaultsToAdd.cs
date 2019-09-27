using System;

namespace Statiq.App
{
    [Flags]
    public enum DefaultsToAdd
    {
        None = 0,
        Logging = 1 << 0,
        Settings = 1 << 1,
        EnvironmentVariables = 1 << 2,
        Configurators = 1 << 3,
        Commands = 1 << 4,
        Shortcodes = 1 << 5,
        Namespaces = 1 << 6,
        Pipelines = 1 << 7,
        All = Logging | Settings | EnvironmentVariables | Configurators | Commands | Shortcodes | Namespaces | Pipelines
    }
}
