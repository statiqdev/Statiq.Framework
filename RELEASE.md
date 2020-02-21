# 1.0.0-alpha.26

- Added some `Span<char>.Replace()` extension methods.
- Added `NormalizedPath.ReplaceInvalidFileNameChars()` and `NormalizedPath.ReplaceInvalidPathChars()` static methods.
- Added `IMetadata.GetRawEnumerable()` and an `IMetadata.GetRawEnumerable()` extension method to make enumerating raw key-value pairs easier.
- Added a new `ExecuteBranch` module that can execute multiple branches of modules.
- Added config overrides to the modules that operate on document sets.
- Fixes a bug with `MirrorResources` and files that contain "index" in their name.
- Combines `FilePath` and `DirectoryPath` into a single consolidated `NormalizedPath (#79).
- Fixes a race condition in `Process.WaitForExit(int)` calls (thanks @duncanawoods).

# 1.0.0-alpha.25

- Added support for object-based settings to the bootstrapper (as opposed to just initial string-based configuration).
- Added support for `IConfig` metadata values.
- Added `LastWriteTime` and `CreationTime` to `IFileSystemEntry`.
- Fixed a race condition in `ScriptMetadataValue`.
- Changed behavior of `SetDestination` when using a config value to make `DestinationPath`, `DestinationFileName`, and `DestinationExtension` take precedence (with an option to override).
- Added a `Context` property for the current `IExecutionContext` to the available script properties (I.e., for use in metadata value scripts via "=>" notation).

# 1.0.0-alpha.24

- Added new `ProcessHtml` module for more flexible processing of DOM nodes.
- Added new `IEnumerable<IDocument>.Flatten()` extension to flatten document trees.
- Added new `IEnumerable<IDocument>.FilterSources()` and `IEnumerable<IDocument>.FilterDestinations()` extension methods.
- Added a new `FilterDestinations` module to filter documents by destination path.
- Added a `Config.ContainsSettings(params string[] keys)` config factory to return whether the settings contain all the specified keys.
- Refactored some methods from `IExecutionContext` into `IExecutionState` and moved implementation to `Engine`.
- Added new `EnumerateValues` module that will clone or create documents for each item in an enumeration.
- Added `Keys.ExcludeFromEvaluation` that can exclude all or some metadata values from automatic script evaluation.
- Added `IMetadata.GetNestedMetadata()` to get a nested metadata value (not called `IMetadata.GetMetadata()` to avoid conflicts with the old previous method of that name).
- Renamed `IMetadata.GetMetadata()` to `IMetadata.FilterMetadata()` which now returns a `FilteredMetadata` instance.
- Added new `FilteredMetadata` class to filter underlying metadata items by key(s).
- Refactored a bunch of default interface implementations back to extension methods (turns out default interface implementations are a little awkward to maintain).
- Moved scripting support and the `CompileScript` and `EvaluateScript` modules to `Statiq.Core`.
- Added `Microsoft.CodeAnalysis` to `Statiq.Core`.
- Metadata and configuration settings that are a string starting with "=>" are now considered "scripted" and the content to the right of the arrow will be lazily evaluated as C# code - this is a **big deal** and the use cases will become apparent over time (I have lots of big ideas around this feature).
- Removed the `InterpolateMetadata` module in favor of more robust built-in scripted metadata.
- Removed the `Statiq.CodeAnalysis.IDocumentExtensions.Interpolate()` extension method in favor of more robust built-in scripted metadata.
- Changed `CancellationTokenSource` uses inside the engine to `CancellationToken` since the engine does not itself cancel execution.
- Surfaced the `CancellationToken` for a given execution through the `IExecutionState`.
- Added a check to ensure the engine is only performing one execution at a time (the outer execution loop is not concurrently safe).
- Major refactoring involving engine and execution context interfaces, added a new `IExecutionState` interface that essentially represents a run-time engine.
- Added a new `DocumentIdComparer` to compare documents by ID (#69, thanks @mholo65).
- Removed `IParallelModule.WithSequentialExecution()` and standardized on `.WithParallelExecution(false)` instead to make default behavior of running in parallel clearer (I.e., you have to turn it off).
- Added additional configuration methods to `CacheDocuments` providing more control over when to invalidate cached documents (#78).
- Added `IReadOnlyPipeline` for runtime access to pipeline data without changing modules.
- Added `IExecutionContext.Pipeline` to get the currently executing pipeline from the execution context.
- Removed `IBootstrapper` and refactored to the one true `Bootstrapper`.
- Added a `BootstrapperFactory` available via `Bootstrapper.Factory` to create bootstrappers (this will make specialized creation extensions easier to discover).

# 1.0.0-alpha.23

- Changing target for all projects to .NET Core 3.1 LTS.
- New `Eval` shortcode (#37, #68, thanks @ProH4Ck).
- Fixes the `CacheDocuments` module and excludes `IDocument.Id` from hash calculation (#74, thanks @mholo65).

# 1.0.0-alpha.22

- Fixes a bug with `MirrorResources` and relative links (#72, thanks @dafergu2).
- The `PhaseOutputs` collection now returns output documents from the most recent available phase or an empty result set if no phases were defined.
- Adds .wasm as a supported media type (required for WebAssembly streaming).

# 1.0.0-alpha.21

- Fixes a bug with parallel modules when they return a null enumerable.
- Adds `AnalyzeCSharp.WithCompilationAssemblyName()` to set the name of the module compilation (#71).
- Adds a metadata key "Compilation" to `AnalyzeCSharp` output documents to get the Roslyn `Compilation` from the module (#71).
- Adds `IConfig.EnsureNonNull()` and `IConfig.EnsureNonDocument()` extensions to simplify config parameter checks.
- Refactors many of the configuration methods in `AnalyzeCSharp` to take configs instead of atomic values.
- Ensures namespace documents from `AnalyzeCSharp` contain the "ContainingAssembly" metadata (#70).
- Build script support for non-Windows platforms via a new `build.sh` (#65, thanks @khalidabuhakmeh).
- Adds a `Config<TValue>.MakeEnumerable()` extension to transform a config into an enumerable value.
- Adds a common `MultiConfigModuleBase` for `MultiConfigModule` and `ParallelMultiConfigModule`.
- Adds `CombineConfig` helper methods to `MultiConfigModuleBase` to help with combining config values during configuration.
- Refactors several of the `StartProcess` configuration methods to take configs.

# 1.0.0-alpha.20

- Split `DefaultFeatures.Commands` into `DefaultFeatures.BuildCommands`, `DefaultFeatures.HostingCommands`, and `DefaultFeatures.CustomCommands` for finer control.
- Renamed `DefaultsToAdd` to `DefaultFeatures`.
- Adds a new `ThrowException` module that can be used to throw exceptions based on a config value.
- Renames `BuildCommand` to `PipelinesCommand`.
- Refactors default commands by renaming `build` to `pipelines` and accepting pipelines to execute as an argument (moving the root path to an option).
- Added `IBootstrapper.AddCommands<TParent>()` to add all nested class commands of a parent type.
- Added `IBootstrapper.AddPipelines<TParent>()` to add all nested class pipelines of a parent type.
- Added `Bootstrapper.CreateDefaultWithout()` and `IBootstrapper.AddDefaultsWithout()` to create a default bootstrapper without specific components.
- Renamed `IBootstrapper.AddBuildCommand()` methods to `IBootstrapper.AddPipelineCommand()`.

# 1.0.0-alpha.19

- Added `StartProcess.WithArgument()` methods to add arguments to the module using a fluent interface.
- Added `Config<TValue>.CombineWith()` extensions for combining two configs.
- Added `Config<TValue>.Transform()` extensions for transforming from one value to another.
- Made `IExecutionContext` (re)implement `IMetadata` through `Settings`.
- Added `ToString()` overloads to `IFileSystemEntry` (can't believe those weren't already there).
- Added a `Statiq.Netlify` extension with a `DeployNetlifySite` module.
- `IDocument` now implements `IContentProviderFactory`.
- Added some additional `IContentProvider` overloads to the `DeployAppService` module.

# 1.0.0-alpha.18

- Updated several low-risk package versions.
- Adds option names to all command option values (#64, thanks @patriksvensson).
- New `serve` command for serving arbitrary folders with a local preview server (#55, #60, thanks @duracellko).
- Fix for regression when there are dependencies on pipelines that aren't executing.

# 1.0.0-alpha.17

- Small -- prefix in console/logs to help indicate when a pipeline phase is finished.
- Set longer timeouts for App Service uploads.
- Added support for uploading a zip file to Azure App Service (useful for artifact-based release processes).

# 1.0.0-alpha.16

- Fixes a bug in dependency ordering where deployment pipeline output phases were being run too early.

# 1.0.0-alpha.15

- Added `SetMediaType` module to set the media type without changing the content.
- Changed the semantics of `IDocument.ContentProvider` so that it's never null.
- Added `IContentProvider.Length` to get the content length without needing to get the stream.
- Renamed `AddMetadata` to `SetMetadata` to better match other module naming conventions and to reflect the metadata key being set might already exist.
- Renamed `ReplaceContent` to `SetContent` to better match other module naming conventions.
- Removed the pre/post render flag from `ProcessShortcodes` and uses a default delimiter of `<?# ... ?>` (the forthcoming site generator will need to define the alternate delimiter for pre-transform shortcodes directly).
- Added overloads of methods that create content providers to set the media type.
- Updated all built-in modules to set the media type whenever appropriate.
- Added `IContentProvider.MediaType` to surface the media type of content.
- Added a `MediaTypes` static class that contains an exhaustive set of media type (MIME) mappings by file extension.
- Added a `DeploySearchIndex` module that deploys an Azure search index from input document metadata.
- Added a `deploy` command that executes deployment pipelines.
- Added a new `ExecutionPolicy.Normal` policy and changed `ExecutionPolicy.Default` to specify different behavior depending on if the pipeline is a deployment pipeline.

# 1.0.0-alpha.14

- The string overload of `SetDestination` now takes either an extension or a path distinguished by a preceding dot.
- Updated Statiq.Razor to 3.0 libraries.
- Updated Statiq.Hosting to 3.0 libraries.

# 1.0.0-alpha.13

- Fixes `ExecuteIf` to work when there are no input documents and the config doesn't require one.
- Fixes bug calculating command name for generic command types.
- Refactors `CreateDocuments` and adds additional config-based overloads.

# 1.0.0-alpha.12

- Changed `IBoostrapper.ConfigureSettings()` to use a new `IConfigurationSettings` object that exposes the settings and the underlying `IConfiguration`
- Renamed the execution-time `IConfigurationSettings` to `IReadOnlyConfigurationSettings` and introduced a new mutable `IConfigurationSettings` to use in the bootstrapper.
- Several bug fixes related to settings and configuration.

# 1.0.0-alpha.11

- Changed `IBootstrapper.AddSettings()` calls to run after other configuration.
- Adds a single pattern overload to `ReadFiles`.
- Some refactoring of the base `Pipeline` class (most importantly to remove the `Dependencies` setter in favor of adding to the existing hash set).
- Adds `IReadOnlyApplicationState.IsCommand()` to determine the current command.

# 1.0.0-alpha.10

- Added extensions for CLI `IConfigurator` to allow more flexible direct configuration (such as command branches).
- Removed configuration/settings debug output on startup as it could leak secrets via environment variables or other configuration providers.
- Fix for `GenerateJson` so it executes when no input documents are provided.
- Fix for preview command when no output is generated and the output directory does not exist.
- Fix for clearing content with empty string in `ReplaceContent`.
- Renamed `EngineSettings` to `ConfigurationSettings` to reflect a broader use than just the engine.

# 1.0.0-alpha.9

- Added `IBootstrapper.AddDelegateCommand()` fluent methods to configure delegate-based commands.
- Added `IBootstrapper.AddBuildCommand()` fluent methods to configure simple commands that build specified pipelines.
- Refactored the base commands to allow consumers to derive from `EngineCommand`.
- Added a new `IEngineManager` interface to expose the engine manager to commands that derive from `EngineCommand`.
- Refactored `IEngine.Settings` and `IExecutionContext.Settings` to use a `IConfiguration` as the backing store and present it as metadata.
- Lazily creates type-based pipelines using the DI container so they can have injected services (#59).
- Adds `INamedPipeline` to allow pipeline instances to provide names.
- Changes `Module.AfterExecution()` and `Module.AfterExecutionAsync()` to pass a new `ExecutionOutputs` class instead of by ref (which doesn't work with async).
- Some tweaks to the `MirrorResources` retry policy.

# 1.0.0-alpha.8

- Adds `==` overloads to `NormalizedPath`.
- Adds a special `RenderSection()` to `StatiqRazorPage` that renders default content if the section is not defined.
- Renamed `IDocument.GetStream()` to `IDocument.GetContentStream()`.
- Renamed `IDocument.GetStringAsync()` to `IDocument.GetContentStringAsync()`.
- Renamed `IDocument.GetBytesAsync()` to `IDocument.GetContentBytesAsync()`.
- Added `IEngine.SerialExecution` and `--serial` CLI argument to run pipelines and modules in serial (#58).

# 1.0.0-alpha.7

- Adds support for deployment pipelines (`IPipeline.Deployment`) which run their output phase only after other output phases (#57).
- Fixes a bug when specifying a setting on the CLI and the bootstrapper.
- Adds `StartProcess.WithErrorExitCode()` to define a custom function for determining if the process existed in error.
- Adds new `-d`/`--defaults` and a flag to the engine to indicate if default pipelines should be run independent of specified pipelines.
- Renames `SimpleBuildCommand` to `CustomBuildCommand` and adds support for the default pipelines flag.

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