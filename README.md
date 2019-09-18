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

### The Bootstrapper

The easiest way to get started with Statiq Framework is to use the `Bootstrapper` from the [Statiq.App](https://www.nuget.org/packages/Statiq.App) package. This class helps create an engine and has fluent methods to configure it, add modules and pipelines, and process command-line arguments.

In general, a Statiq Framework application looks something like the following:

```csharp
public class Program
{
  private static async Task<int> Main(string[] args) =>
    await Bootstrapper
      .CreateDefault(args)
      .BuildPipeline(
        "Pages",
        builder => builder
          .WithInputReadFiles("**/*.md")
          .WithProcessModules(new RenderMarkdown())
          .WithOutputWriteFiles(".html"))
      .RunAsync();
}
```

More exhaustive code samples and examples will be provided soon.

### Defining Pipelines and Adding Modules

Pipelines can be defined in several different ways depending on your requirements and style preferences.

TODO: Clean up this section, loose thoughts below

- Through the builder
- By creating a `Pipeline` class
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
- Use `Config<T>`:
  - If your module needs to accept user-configurable values, use `Config<T>`.
- Avoid document-to-document references (especially to/from children):
  - Try to avoid creating documents that reference other documents, especially in the top-level output documents (parent documents that reference children may be okay in some cases). If a document references another document and a following module clones the referenced document, the reference will still point to the old document and not the new clone.
- Preserve input ordering:
  - Many modules output documents in a specific order and following modules should preserve that order whenever possible. The base module classes do this by default, but any explicit parallel operations should preserve ordering as well (I.e., by calling `.AsParallel().AsOrdered()`).
- Only reference `Statiq.Common`:
  - If a module is in a separate assembly from your application you shouldn't need a reference to `Statiq.Core`, and if you find that you do please open an issue so the appropriate functionality can be moved to `Statiq.Common`.

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

Events can be helpful when you need to implement cross-cutting behavior at runtime. Statiq Framework has a global event mechanism makes it easy to subscribe and handle events.

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
