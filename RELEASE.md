# 1.0.0-alpha-2

- Raw application arguments as well as application input are now surfaced through a new `IReadOnlyApplicationState` object in the `IExecutionContext`, taking the place of the `ApplicationInput` property.
- Adds a bunch of `Config.FromSettings()` methods that get values from a `IReadOnlySettings`
- "Pipeline" is now trimmed from the end of type names when types are added as a pipeline to a pipeline collection.

# 1.0.0-alpha-1

- Statiq Framework is comprehensive "reboot" of Wyam.