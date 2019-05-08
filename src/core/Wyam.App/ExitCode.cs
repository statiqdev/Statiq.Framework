namespace Wyam.App
{
    public enum ExitCode
    {
        Normal = 0,
        UnhandledError = 1,
        CommandLineError = 2,
        ConfigurationError = 3,
        ExecutionError = 4
    }
}
