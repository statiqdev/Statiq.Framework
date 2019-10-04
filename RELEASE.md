# 1.0.0-alpha.7

- Adds support for deployment pipelines (`IPipeline.Deployment`) which run their output phase only after other output phases (#57).
- Fixes a bug when specifying a setting on the CLI and the bootstrapper.
- Adds `StartProcess.WithErrorExitCode()` to define a custom function for determining if the process existed in error.

# 1.0.0-alpha.6

- Adds back a `ExecuteModules` module that works like the old `Branch` module used to by dropping any output documents from the child modules.
- Tweaks to the placeholder factory in `CreateTree`.
- Fix for the JavaScript engine getting reset on execution.
- No longer strips "Pipeline" from the end of pipeline classes for the pipeline name since `nameof` is often used to refer to pipelines.

# 1.0.0-alpha.5

- Adds `StartProcess.KeepContent()` to prevent replacing document content with process output.
- Adds `StartProcess.OnlyOnce()` to only execute the process once.
- Renamed `PipelineTrigger` to `ExecutionPolicy`.
- Adds type-based methods for adding pipelines.
- All `IPipeline` implementations from the entry assembly are added by the bootstrapper by default.
- All `ICommand` implementations from the entry assembly are added by the bootstrapper by default.
- Adds ability to specify which defaults to add to the bootstrapper.
- Made `EngineManager` public so it can be used by custom commands.
- Adds a new `SimpleBuildCommand` base command to make creating new pipeline-specific build commands easier.
- Adds `AddPipelines()` and `AddCommands()` methods to add pipelines and commands from the entry or a given assembly.
- Adds `MultiConfigModule`, `ParallelMultiConfigModule`, `SyncMultiConfigModule`, and `ParallelSyncMultiConfigModule` base classes for modules that use multiple `Config<T>` values.
- Adds `ExecutionPipeline` base pipeline for use when a custom pipeline that runs code for each phase is needed.
- Adds new `ZipDirectory` module.
- Renames the `Statiq.AmazonWebServices` extension library to `Statiq.Aws`.
- Adds `Statiq.Azure` extension library.
- Adds new `DeployAppService` module.

# 1.0.0-alpha.4

- Updated to .NET Core 3.0 final.
- Isolated pipelines can now be dependencies of other pipelines (but output documents still can't be accessed).
- Renames delegate-based `IBootstrapper.AddSettings()` overload to `IBoostrapper.ConfigureSettings()`.
- Renames `IBootstrapper.AddServices()` to `IBoostrapper.ConfigureServices()`.
- Adds `IBootstrapper.ConfigureEngine()`.
- Adds a `StartProcess` module to start a process and create a document from it's output or run it in the background.
- The bootstrapper now adds environment variables to the settings by default with ALL_CAPS keys.
- Any setting with an ALL_CAPS key is masked during debug output on startup.
- Ongoing console logging improvements.
- Added trigger conditions to pipelines to include always running or manually running.
- Added a `-p`/`--pipeline` CLI argument to indicate which pipelines to execute.

# 1.0.0-alpha.3

- New execution summary table logged after execution.
- New console logger with better output.
- More refactoring of base `Module` before/after methods.

# 1.0.0-alpha.2

- Renamed the GitHub project/repo to "Statiq.Framework" to match forthcoming "Statiq.Web" and to distinguish between primary code repos (prefixed by "Statiq.") and themes, etc. Also note the upcoming Statiq app will be known as Statiq Web from now on (as opposed to Statiq Framework).
- The engine now returns a `IPipelineOutputs` with the result documents from each pipeline.
- Adds global events `BeforeModuleExecution` and `AfterModuleExecution` with ability to override outputs.
- Adds a new global event mechanism via `IEventCollection`, `IReadOnlyEventCollection`, `IEngine.Events`, and `IExecutionContext.Events`.
- Refactored the base module classes to include a before/after execution method, made the execution methods `protected`, and renamed the execution methods for clarity.
- Added property setters with null checks to `Pipeline` so it works better as a base class and you can define the phase modules directly as properties.
- Cleaned up `ModuleList` methods to remove overload ambiguity between `params IModule[]` and `IEnumerable<IModule>`.
- Added an implicit operator from `IModule[]` to `ModuleList`.
- Raw application arguments as well as application input are now surfaced through a new `IReadOnlyApplicationState` object in the `IExecutionContext`, taking the place of the `ApplicationInput` property.
- Adds a bunch of `Config.FromSettings()` methods that get values from a `IReadOnlySettings`.
- "Pipeline" is now trimmed from the end of type names when types are added as a pipeline to a pipeline collection.

# 1.0.0-alpha.1

- Statiq Framework is comprehensive "reboot" of Wyam.