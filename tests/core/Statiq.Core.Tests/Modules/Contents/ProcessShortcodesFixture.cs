using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Shouldly;
using Statiq.Common;
using Statiq.Common.Documents;
using Statiq.Common.Execution;
using Statiq.Common.Meta;
using Statiq.Common.Shortcodes;
using Statiq.Testing;
using Statiq.Testing.Documents;
using Statiq.Testing.Execution;

namespace Statiq.Core.Tests.Modules.Contents
{
    [TestFixture]
    public class ProcessShortcodesFixture : BaseFixture
    {
        public class ExecuteTests : ProcessShortcodesFixture
        {
            [Test]
            public async Task ProcessesShortcode()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                context.Shortcodes.Add<TestShortcode>("Bar");
                TestDocument document = new TestDocument("123<?# Bar /?>456");
                Core.Modules.Contents.ProcessShortcodes module = new Core.Modules.Contents.ProcessShortcodes();

                // When
                TestDocument result = await ExecuteAsync(document, context, module).SingleAsync();

                // Then
                result.Content.ShouldBe("123Foo456");
            }

            [Test]
            public async Task ProcessesNestedShortcodeInResult()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                context.Shortcodes.Add<TestShortcode>("Nested");
                context.Shortcodes.Add<NestedShortcode>("Bar");
                TestDocument document = new TestDocument("123<?# Bar /?>456");
                Core.Modules.Contents.ProcessShortcodes module = new Core.Modules.Contents.ProcessShortcodes();

                // When
                TestDocument result = await ExecuteAsync(document, context, module).SingleAsync();

                // Then
                result.Content.ShouldBe("123ABCFooXYZ456");
            }

            [Test]
            public async Task ProcessesNestedShortcode()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                context.Shortcodes.Add<RawShortcode>("Foo");
                context.Shortcodes.Add<TestShortcode>("Bar");
                TestDocument document = new TestDocument("123<?# Foo ?>ABC<?# Bar /?>XYZ<?#/ Foo ?>456");
                Core.Modules.Contents.ProcessShortcodes module = new Core.Modules.Contents.ProcessShortcodes();

                // When
                TestDocument result = await ExecuteAsync(document, context, module).SingleAsync();

                // Then
                result.Content.ShouldBe("123ABCFooXYZ456");
            }

            [Test]
            public async Task DoesNotProcessNestedRawShortcode()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                context.Shortcodes.Add<RawShortcode>("Raw");
                context.Shortcodes.Add<TestShortcode>("Bar");
                TestDocument document = new TestDocument("123<?# Raw ?>ABC<?# Bar /?>XYZ<?#/ Raw ?>456");
                Core.Modules.Contents.ProcessShortcodes module = new Core.Modules.Contents.ProcessShortcodes();

                // When
                TestDocument result = await ExecuteAsync(document, context, module).SingleAsync();

                // Then
                result.Content.ShouldBe("123ABC<?# Bar /?>XYZ456");
            }

            [Test]
            public async Task DoesNotProcessDirectlyNestedRawShortcode()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                context.Shortcodes.Add<RawShortcode>("Raw");
                context.Shortcodes.Add<TestShortcode>("Bar");
                TestDocument document = new TestDocument("123<?# Raw ?><?# Bar /?><?#/ Raw ?>456");
                Core.Modules.Contents.ProcessShortcodes module = new Core.Modules.Contents.ProcessShortcodes();

                // When
                TestDocument result = await ExecuteAsync(document, context, module).SingleAsync();

                // Then
                result.Content.ShouldBe("123<?# Bar /?>456");
            }

            [Test]
            public async Task ShortcodeSupportsNullResult()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                context.Shortcodes.Add<TestShortcode>("S1");
                context.Shortcodes.Add<NullResultShortcode>("S2");
                TestDocument document = new TestDocument("123<?# S1 /?>456<?# S2 /?>789<?# S1 /?>");
                Core.Modules.Contents.ProcessShortcodes module = new Core.Modules.Contents.ProcessShortcodes();

                // When
                TestDocument result = await ExecuteAsync(document, context, module).SingleAsync();

                // Then
                result.Content.ShouldBe("123Foo456789Foo");
            }

            [Test]
            public async Task DisposesShortcode()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                context.Shortcodes.Add<DisposableShortcode>("Bar");
                TestDocument document = new TestDocument("123<?# Bar /?>456");
                Core.Modules.Contents.ProcessShortcodes module = new Core.Modules.Contents.ProcessShortcodes();

                // When
                await ExecuteAsync(document, context, module).SingleAsync();

                // Then
                DisposableShortcode.Disposed.ShouldBeTrue();
            }

            [Test]
            public async Task ShortcodesCanAddMetadata()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                context.Shortcodes.Add<AddsMetadataShortcode>("S1");
                context.Shortcodes.Add<AddsMetadataShortcode2>("S2");
                TestDocument document = new TestDocument("123<?# S1 /?>456<?# S2 /?>789");
                Core.Modules.Contents.ProcessShortcodes module = new Core.Modules.Contents.ProcessShortcodes();

                // When
                TestDocument result = await ExecuteAsync(document, context, module).SingleAsync();

                // Then
                result.Content.ShouldBe("123456789");
                result["A"].ShouldBe("3");
                result["B"].ShouldBe("2");
                result["C"].ShouldBe("4");
            }

            [Test]
            public async Task ShortcodesCanReadMetadata()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                context.Shortcodes.Add<ReadsMetadataShortcode>("S1");
                context.Shortcodes.Add<ReadsMetadataShortcode>("S2");
                TestDocument document = new TestDocument("123<?# S1 /?>456<?# S2 /?>789<?# S1 /?>")
                {
                    { "Foo", 10 }
                };
                Core.Modules.Contents.ProcessShortcodes module = new Core.Modules.Contents.ProcessShortcodes();

                // When
                TestDocument result = await ExecuteAsync(document, context, module).SingleAsync();

                // Then
                result.Content.ShouldBe("123456789");
                result["Foo"].ShouldBe(13);
            }

            [Test]
            public async Task ShortcodesPersistState()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                context.Shortcodes.Add<IncrementingShortcode>("S");
                TestDocument document = new TestDocument("123<?# S /?>456<?# S /?>789<?# S /?>");
                Core.Modules.Contents.ProcessShortcodes module = new Core.Modules.Contents.ProcessShortcodes();

                // When
                TestDocument result = await ExecuteAsync(document, context, module).SingleAsync();

                // Then
                result.Content.ShouldBe("123456789");
                result["Foo"].ShouldBe(22);
            }
        }

        public class TestShortcode : IShortcode
        {
            public Task<IDocument> ExecuteAsync(KeyValuePair<string, string>[] args, string content, IDocument document, IExecutionContext context) =>
                Task.FromResult<IDocument>(new TestDocument("Foo"));
        }

        public class NestedShortcode : IShortcode
        {
            public Task<IDocument> ExecuteAsync(KeyValuePair<string, string>[] args, string content, IDocument document, IExecutionContext context) =>
                Task.FromResult<IDocument>(new TestDocument("ABC<?# Nested /?>XYZ"));
        }

        public class RawShortcode : IShortcode
        {
            public async Task<IDocument> ExecuteAsync(KeyValuePair<string, string>[] args, string content, IDocument document, IExecutionContext context) =>
                new TestDocument(await context.GetContentStreamAsync(content));
        }

        public class NullResultShortcode : IShortcode
        {
            public Task<IDocument> ExecuteAsync(KeyValuePair<string, string>[] args, string content, IDocument document, IExecutionContext context) => Task.FromResult<IDocument>(null);
        }

        public class DisposableShortcode : IShortcode, IDisposable
        {
            public static bool Disposed { get; set; }

            public DisposableShortcode()
            {
                // Make sure it resets
                Disposed = false;
            }

            public async Task<IDocument> ExecuteAsync(KeyValuePair<string, string>[] args, string content, IDocument document, IExecutionContext context) =>
                new TestDocument(await context.GetContentStreamAsync("Foo"));

            public void Dispose() =>
                Disposed = true;
        }

        public class AddsMetadataShortcode : IShortcode
        {
            public Task<IDocument> ExecuteAsync(KeyValuePair<string, string>[] args, string content, IDocument document, IExecutionContext context) =>
                Task.FromResult<IDocument>(new TestDocument(new MetadataItems
                {
                    { "A", "1" },
                    { "B", "2" }
                }));
        }

        public class AddsMetadataShortcode2 : IShortcode
        {
            public Task<IDocument> ExecuteAsync(KeyValuePair<string, string>[] args, string content, IDocument document, IExecutionContext context) =>
                Task.FromResult<IDocument>(new TestDocument(new MetadataItems
                {
                    { "A", "3" },
                    { "C", "4" }
                }));
        }

        public class ReadsMetadataShortcode : IShortcode
        {
            public Task<IDocument> ExecuteAsync(KeyValuePair<string, string>[] args, string content, IDocument document, IExecutionContext context) =>
                Task.FromResult<IDocument>(new TestDocument(new MetadataItems
                {
                    { $"Foo", document.Get<int>("Foo") + 1 }
                }));
        }

        public class IncrementingShortcode : IShortcode
        {
            private int _value = 20;

            public Task<IDocument> ExecuteAsync(KeyValuePair<string, string>[] args, string content, IDocument document, IExecutionContext context) =>
                Task.FromResult<IDocument>(new TestDocument(new MetadataItems
                {
                    { $"Foo", _value++ }
                }));
        }
    }
}
