# Statiq Framework

![Logo](logo.png)

Statiq Framework is a flexible and extensible static content generation framework for .NET. This project is an evolution of [Wyam](https://wyam.io) and is planned for official release late 2019.

## Introduction

Statiq Framework is oriented around three main concepts:

- **Documents** embody data as it moves through the system. Documents can represent any kind of data and may contain content, metadata, or both. Try not to think of documents and having a one-to-one relationship with files. Some documents may eventually be output as a file while others are intended only for data passing. Documents are immutable and must be cloned to change their content and/or metadata.
- **Modules** create, manipulate, and operate on documents. Modules form the logic and processing core of a Statiq Framework application and the framework comes with many modules for performing different operations. You can also easily write your own modules to fully customize generation.
- **Pipelines** execute one or more modules in sequence. Pipelines have four phases, any of which may contain a sequence of modules to execute: input, process, transform, output. Pipelines are executed concurrently and can specify dependencies.

In many ways, Statiq Framework can be viewed as an implementation of the [model-view-controller](https://en.wikipedia.org/wiki/Model-view-controller) (MVC) pattern. This analogy can be helpful when figuring out how to apply the different components of the framework. Documents contain the data model, pipelines and modules act as controllers, and your layouts and themes are the view.

## Usage

### The Engine

The core runtime component of a Statiq Framework application is the `Engine` which contains the collections of pipelines, settings, and services. The term _execution_ is generally used to describe applying pipelines and modules to documents and the primary responsibility of the engine is to determine the pipeline dependency hierarchy and coordinate the execution of pipelines (and their modules).

You can create and use the `Engine` class directly, but using the bootstrapper (described below) is recommended.

### Getting Started

The easiest way to get started with Statiq Framework is to use the `Bootstrapper` from the [Statiq.App](https://www.nuget.org/packages/Statiq.App) package. This class helps create an engine and has fluent methods to configure it, add modules and pipelines, and process command-line arguments.

For this sample, you'll need the following NuGet Packages.

```console
> dotnet add package Statiq.App
> dotnet add package Statiq.Common
> dotnet add package Statiq.Markdown
```

```csharp
using Statiq.App;
using Statiq.Markdown;
using System.Threading.Tasks;

namespace HelloStatiq
{
    class Program
    {
        private static async Task<int> Main(string[] args) =>
            await Bootstrapper
                .Factory
                .CreateDefault(args)
                .BuildPipeline(
                    "Pages",
                    builder => builder
                        .WithInputReadFiles("**/*.md")
                        .WithProcessModules(new RenderMarkdown())
                        .WithOutputWriteFiles(".html"))
                .RunAsync();
    }
}
```

Next, create a directory named `input` in your project. Create a new `index.md` file within the newly created `input` folder.

```md
# Hello World!
## From Statiq

> This is the first page I've generated with Statiq. 
> <br> -- Statiq Generator
```

Be sure to mark the new file to be copied to the build directory. This can be accomplished by right clicking the properties of the file and choosing to copy it to the build directory at compile time. You may also modify the `.csproj` file to accomplish the same thing.

```xml
    <ItemGroup>
      <Content Include="input\index.md">
        <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
      </Content>
    </ItemGroup>
```

Now we are ready to run the project. During the build process, Statiq will show the results of each pipeline.

```console
[INFO] Statiq version 1.0.0-alpha.26+0ba395e08ff85c191f88e053e829add9e7bb6d58
[INFO] Root path:
       /Users/<user>/Projects/dotnet/HelloStatiq/bin/Debug/netcoreapp3.1
[INFO] Input path(s):
       theme
       input
[INFO] Output path:
       output
[INFO] Temp path:
       temp
[INFO] Executing 1 pipelines (Pages)
[INFO] Cleaned temp directory: temp
[INFO] Cleaned output directory: output
[INFO] -> Pages/Input » Starting Pages Input phase execution... (0 input document(s), 1 module(s))
[INFO]    Pages/Input » Finished Pages Input phase execution (1 output document(s), 114 ms)
[INFO] -> Pages/Process » Starting Pages Process phase execution... (1 input document(s), 1 module(s))
[INFO]    Pages/Process » Finished Pages Process phase execution (1 output document(s), 94 ms)
[INFO] -> Pages/Output » Starting Pages Output phase execution... (1 input document(s), 2 module(s))
[INFO]    Pages/Output » Finished Pages Output phase execution (1 output document(s), 41 ms)
[INFO] Execution summary...

Number of output documents per pipeline and phase:

 | Pipeline | Input      | Process   | Transform | Output    | Total Time | 
 |------------------------------------------------------------------------| 
 | Pages    | 1 (114 ms) | 1 (94 ms) |           | 1 (41 ms) | 249 ms     | 

Pipeline phase timeline:

 | Pipeline | Timeline (253 total ms)                                                              | 
 |-------------------------------------------------------------------------------------------------| 
 | Pages    | I--------------------------------------P-------------------------------O------------ | 


[INFO] Finished execution in 290 ms
[INFO] Cleaned temp directory: temp
```

With a resulting output file in our build directory `\output\index.html`.

```html
<h1>Hello World!</h1>
<h2>From Statiq</h2>
<blockquote>
<p>This is the first page I've generated
with Statiq.
<br> -- Statiq Generator</p>
</blockquote>
```

More exhaustive code samples and examples will be provided soon.

### About Pipelines and Phases

Pipelines define the work to be done by the engine and contain a sequence of modules that operate on documents. A pipeline has four phases, any of which can contain modules (or not). While the phases serve as a useful organizational tool, they also have a practical purpose with regard to how pipelines manage concurrency. The outputs from the last module of a particular phase are used as the inputs to the first module of the next phase. Pipeline phases follow rules for when each executes (described below) but will execute as soon as possible within those rules. In other words, the engine doesn't run one phase for all the pipelines and then wait to run the next phase for all the pipelines (except for the transform phase which does wait for all process phases to complete).

The four phases are:

- **Input** is generally used for fetching data from an outside source and creating documents from it. For example, the file system, a database, a web API, etc. The input phase is immediatly started for all pipelines concurrently and cannot access outputs from other pipelines.
- **Process** is where documents are manipulated and most of the pipeline logic should go. For simple pipelines, all modules can be placed in the process phase. Process phases for each pipeline are executed in dependency order (and concurrently when possible) and a process phase can access output documents from the process phase(s) of other _dependent pipelines_.
- **Transform** contains modules that apply templates or otherwise render the output documents from the process phase into something that should be output. The transform phase for each pipeline is only executed after all process phases have finished and therefore has access to the process phase outputs from _all pipelines_.
- **Output** is used for modules that output the finished documents somewhere (usually to disk, but could also be to a database, web service, etc.). A pipeline's output phase is executed immediatly following it's transform phase.

A pipeline can also follow one of three execution policies that defines if that pipeline is executed:

- **Default** means that the pipeline is executed unless other pipelines are explicitly specifed on the command line or when running the engine. A default pipeline will also be executed if it's the dependent of an executing pipeline. This policy is the most common and should be used for most pipelines.
- **Manual** means that the pipeline is only executed if explicity specified on the command line or when running the engine. A manual pipeline will also be executed if it's the dependent of an executing pipeline. This policy is useful for specialized pipelines that should only be executed on-demand and are not part of the normal generation process.
- **Always** means that the pipeline should always execute regardless of what pipelines are explicitly specified (if any). Pipelines with an always policy are useful for housekeeping and other tasks that should be carried out no matter what.

Finally, pipelines can also have a couple additional modifiers:

- **Isolated** pipelines are executed independant of any other pipelines and cannot have dependencies or be dependant. The outputs from isolated pipelines are also not available to other pipelines regardless of phase. This allows an isolated pipeline to execute immediatly and to begin each phase as soon as the previous one finishes without waiting on other pipelines. This is useful for pipelines that you know will not have any dependencies or be dependent, for example processing Sass files.
- **Deployment** pipelines work like normal pipelines except their output phase is only executed after all other output phases. This allows them to access the final results of other pipelines in order to do things like upload to a server. Generally a deployment pipeline will also have a manual execution policy so that deployment only happens when specified (as opposed to every execution).

### Defining Pipelines and Adding Modules

Pipelines can be defined in several different ways depending on your requirements and style preferences.

TODO: Clean up this section, loose thoughts below

- Through the builder fluent API
- By creating a `Pipeline` class, which are automatically discovered in the entry assembly
- Adding modules using a fluent API in pipeline constructor
- Adding modules using collection initialization
- Adding child modules using collection initialization

### Libraries and Extensions

Statiq Framework consists of many different packages to help you craft exactly the static generator that you need.

The core packages include:

- **[Statiq.App](https://www.nuget.org/packages/Statiq.App)**: contains the `Bootstrapper` and other functionality like command-line parsing. This package should be referenced by generator applications to get up and running quickly.
- **[Statiq.Core](https://www.nuget.org/packages/Statiq.Core)**: contains the core implementation including the `Engine`. This package should be referenced by generator applications for full control over the engine (it's installed automatically when using `Statiq.App`).
- **[Statiq.Common](https://www.nuget.org/packages/Statiq.Common)**: contains abstractions and other common code that extension libraries might need. This package should be referenced by extension libraries such as modules.
- **[Statiq.Hosting](https://www.nuget.org/packages/Statiq.Hosting)**: contains the functionality related to the embedded preview server, live reload, etc.
- **[Statiq.Testing](https://www.nuget.org/packages/Statiq.Testing)**: contains mock classes for most of the interfaces in Statiq.Common and is useful for writing tests that use Statiq objects.

The core packages contain many useful modules, but there are also lots of extension packages that provide additional modules. In general if a module requires additional libraries, it's split out into a separate package. These include [Statiq.Yaml](https://www.nuget.org/packages/Statiq.Yaml), [Statiq.Markdown](https://www.nuget.org/packages/Statiq.Markdown), and [Statiq.Razor](https://www.nuget.org/packages/Statiq.Razor). Reference these packages to gain access to modules like `ParseYaml`, `RenderMarkdown`, and `RenderRazor`.

### Module Guidelines

If the out-of-the-box modules don't satisfy your use case, it's easy to customize generation by creating new modules. Follow these guidelines and tips when doing so:

- Use the `ExecuteConfig` module:
  - You may not even need a new module. The `ExecuteConfig` module lets you specify a delegate that can return documents, content, and other types of data which will be converted to output documents as appropriate.
- Use base classes:
  - Even though implementing the `IModule` interface is the only requirement, strongly consider using one of the many base module classes like `Module` or `SyncModule`.
  - Most of the module base classes (there are many in order to satisfy different use cases) have both an `ExecuteContext` virtual method and an `ExecuteInput` virtual method. Overload the `ExecuteContext` method to have your code called once for all the inputs (available via `IExecutionContext.Inputs`). This is useful for modules that need to create new documents from scratch or that need to aggregate or operate on the input documents as a set. Overload the `ExecuteInput` method to have you code called once per document. This is useful when the module transforms or manipulates documents unrelated to each other.
  - Many existing modules are derived from `ParallelModule` and similar base module classes and implement `IParallelModule`. You can also derive from these base parallel module classes. Note the "parallel" in this context refers to processing input documents in parallel within the module, not how the module is executed in relation to other modules (see the discussion about regarding phases which is what controls when modules are run in relation to each other).
- Use `Config<T>`:
  - If your module needs to accept user-configurable values, use `Config<T>`.
  - Consider using one of the base module classes that deals with `Config<T>` like `ConfigModule` or `MultiConfigModule`.
- Avoid document-to-document references (especially to/from children):
  - Try to avoid creating documents that reference other documents, especially in the top-level output documents (parent documents that reference children may be okay in some cases). If a document references another document and a following module clones the referenced document, the reference will still point to the old document and not the new clone.
- Preserve input ordering:
  - Many modules output documents in a specific order and following modules should preserve that order whenever possible. The base module classes do this by default, but any explicit parallel operations should preserve ordering as well (I.e., by calling `.AsParallel().AsOrdered()`).
- Only reference `Statiq.Common`:
  - If a module is in a separate assembly from your application you shouldn't need a reference to `Statiq.Core`, and if you find that you do please open an issue so the appropriate functionality can be moved to `Statiq.Common`.
- Name modules using a VerbNoun convention when possible.

### Documents

If your module creates or manipulates documents, follow these guidelines and tips on document creation and working with documents:

- Call `IDocument.Clone()` on existing documents to clone with new properties.
- Call `Engine.SetDefaultDocumentType<TDocument>()` to change the default document type.
- Call `IDocumentFactory.CreateDocument()` (engine or execution context) to create a new document of the default document type.
- Call `IDocumentFactory.CreateDocument<TDocument>()` (engine or execution context) to create a new document of the specified document type.
- Call `IDocumentFactoryProvider.CloneOrCreateDocument()` (engine or execution context) to either clone _or_ create a new document of the default document type depending on if a passed-in document exists (is `null`) or not.
- Call `IDocumentFactoryProvider.CloneOrCreateDocument<TDocument>()` to either clone _or_ create a new document of the specified document type depending on if a passed-in document exists (is `null`) or not.

Statiq is very flexible with what can be considered a document. You may find that a custom document type better represents your data than creating a standard document. If you already have an existing data element (such as the result of an API call), it might also be helpful to wrap that object as a document instead of copying it's data to a default document object. Follow these guidelines and tips when working with alternate document types:

- Use base classes:
  - Implementing `IDocument` is the minimum requirement, but it's not recommended to implement this interface directly.
  - Override `Document<TDocument>` to derive a custom document type with built-in metadata support.
  - Override `IDocument.Clone()` in custom document types as needed. The default behavior is to perform a member-wise clone.
- Convert an existing object of any type into a `IDocument` using `.ToDocument()` extensions:
  - This wraps the object in an `ObjectDocument<T>`.

### Execution Context

While executing pipelines and modules, the current state and other functionality is passed in an instance of `IExecutionContext`. This object contains lots of information such as the current pipeline, phase, and module, the settings and file system, the input documents to the module, and more.

The context also implements `IDocumentFactory` so it can be used to create documents (see above), `ILogger` so it can be used for logging, and `IServiceProvider` so it can be used as a dependency injection service provider.

### Events

Events can be helpful when you need to implement cross-cutting behavior at runtime or when you need to modify the behavior of pipelines from other sources. Statiq Framework has a global event mechanism that makes it easy to subscribe to and handle events.

You can subscribe to an event in an engine through the `Events` property:

```csharp
engine.Events.Subscribe<BeforeModuleExecution>(
  evt => evt.Context.LogInformation("I'm in a module!"));
```

You can also subscribe to an event using the bootstrapper:

```csharp
await Bootstrapper
  .CreateDefault(args)
  .SubscribeEvent<BeforeModuleExecution>(
    evt => evt.Context.LogInformation("I'm in a module!"))
  // ...
  .RunAsync();
```

Events are represented by an _event object_ which doesn't have to follow any pattern or derive from any special base class. To expose your own events, create an object that will represent the event and it's data and then raise subscribers through the execution context:

```csharp
await context.Events.RaiseAsync(new MyEvent("some data"));
```

All subscribers to the `MyEvent` object will be invoked in the order in which they were subscribed.

Some of the events Statiq Framework supports are:

- `BeforeEngineExecution` - raised before the engine executes pipelines.
- `AfterEngineExecution` - raised after the engine executes pipelines.
- `BeforePipelinePhaseExecution` - raised before a pipeline phase is executed.
- `AfterPipelinePhaseExecution` - raised after a pipeline phase is executed.
- `BeforeModuleExecution` - raised before a module is executed and provides an opportunity to "short-circuit" the module and provide alternate output documents.
- `AfterModuleExecution` - raised after a module has executed and provides an opportunity to further operate on output documents or provide alternate output documents.

## Licensing

This project is licensed under the Prosperity Public License 2.0.0 **which prohibits commercial use**. A private commercial license for a single major version may be [purchased from License Zero](https://licensezero.com/ids/968702b6-a2b0-4042-9561-d1a98cc4f3fd) for each developer/user:

[![L0](https://licensezero.com/ids/968702b6-a2b0-4042-9561-d1a98cc4f3fd/badge.svg)](https://licensezero.com/ids/968702b6-a2b0-4042-9561-d1a98cc4f3fd)

You _do not_ need a license for non-commercial use. For more licensing information, please read the [licensing FAQs](LICENSING.md).
