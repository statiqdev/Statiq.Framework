using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using NUnit.Framework;
using Shouldly;
using Statiq.Common.Documents;
using Statiq.Common.Execution;
using Statiq.Common.Modules;
using Statiq.Core.Documents;
using Statiq.Core.Execution;
using Statiq.Testing;
using Statiq.Testing.Documents;
using Statiq.Testing.Execution;

namespace Statiq.Core.Tests.Documents
{
    [TestFixture]
    public class DocumentCollectionFixture : BaseFixture
    {
        public class EnumeratorTests : DocumentCollectionFixture
        {
            [Test]
            public void ProcessPhaseGetsDocumentsFromTransientDependencies()
            {
                // Given
                TestDocument a1 = new TestDocument("a1");
                TestDocument b1 = new TestDocument("b1");
                TestDocument b2 = new TestDocument("b2");
                TestDocument c1 = new TestDocument("c1");
                TestDocument d1 = new TestDocument("d1");
                TestDocument d2 = new TestDocument("d2");
                ConcurrentDictionary<string, ImmutableArray<IDocument>> documents =
                    new ConcurrentDictionary<string, ImmutableArray<IDocument>>(StringComparer.OrdinalIgnoreCase);
                IPipelineCollection pipelines = new PipelineCollection();
                PipelinePhase phaseA =
                    GetPipelineAndPhase("A", Phase.Process, pipelines, documents, new[] { a1 });
                PipelinePhase phaseB =
                    GetPipelineAndPhase("B", Phase.Process, pipelines, documents, new[] { b1, b2 }, phaseA);
                PipelinePhase phaseC =
                    GetPipelineAndPhase("C", Phase.Process, pipelines, documents, new[] { c1 }, phaseB);
                PipelinePhase phaseD =
                    GetPipelineAndPhase("D", Phase.Process, pipelines, documents, new[] { d1, d2 });
                DocumentCollection documentCollection = new DocumentCollection(documents, phaseC, pipelines);

                // When
                IDocument[] result = documentCollection.ToArray();

                // Then
                result.ShouldBe(new[] { a1, b1, b2 }, true);
            }

            [Test]
            public void TransformPhaseGetsAllDocuments()
            {
                // Given
                TestDocument a1 = new TestDocument("a1");
                TestDocument b1 = new TestDocument("b1");
                TestDocument b2 = new TestDocument("b2");
                TestDocument c1 = new TestDocument("c1");
                TestDocument d1 = new TestDocument("d1");
                TestDocument d2 = new TestDocument("d2");
                ConcurrentDictionary<string, ImmutableArray<IDocument>> documents =
                    new ConcurrentDictionary<string, ImmutableArray<IDocument>>(StringComparer.OrdinalIgnoreCase);
                IPipelineCollection pipelines = new PipelineCollection();
                PipelinePhase phaseA =
                    GetPipelineAndPhase("A", Phase.Transform, pipelines, documents, new[] { a1 });
                PipelinePhase phaseB =
                    GetPipelineAndPhase("B", Phase.Transform, pipelines, documents, new[] { b1, b2 }, phaseA);
                PipelinePhase phaseC =
                    GetPipelineAndPhase("C", Phase.Transform, pipelines, documents, new[] { c1 }, phaseB);
                PipelinePhase phaseD =
                    GetPipelineAndPhase("D", Phase.Transform, pipelines, documents, new[] { d1, d2 });
                DocumentCollection documentCollection = new DocumentCollection(documents, phaseC, pipelines);

                // When
                IDocument[] result = documentCollection.ToArray();

                // Then
                result.ShouldBe(new[] { a1, b1, b2, c1, d1, d2 }, true);
            }

            [Test]
            public void OnlyReturnsDistinctDocuments()
            {
                // Given
                TestDocument a1 = new TestDocument("a1");
                TestDocument b1 = new TestDocument("b1");
                TestDocument b2 = new TestDocument("b2");
                TestDocument c1 = new TestDocument("c1");
                TestDocument d1 = new TestDocument("d1");
                TestDocument d2 = new TestDocument("d2");
                ConcurrentDictionary<string, ImmutableArray<IDocument>> documents =
                    new ConcurrentDictionary<string, ImmutableArray<IDocument>>(StringComparer.OrdinalIgnoreCase);
                IPipelineCollection pipelines = new PipelineCollection();
                PipelinePhase phaseA =
                    GetPipelineAndPhase("A", Phase.Transform, pipelines, documents, new[] { a1 });
                PipelinePhase phaseB =
                    GetPipelineAndPhase("B", Phase.Transform, pipelines, documents, new[] { b1, b2, a1 }, phaseA);
                PipelinePhase phaseC =
                    GetPipelineAndPhase("C", Phase.Transform, pipelines, documents, new[] { c1 }, phaseB);
                PipelinePhase phaseD =
                    GetPipelineAndPhase("D", Phase.Transform, pipelines, documents, new[] { d1, d2 });
                DocumentCollection documentCollection = new DocumentCollection(documents, phaseC, pipelines);

                // When
                IDocument[] result = documentCollection.ToArray();

                // Then
                result.ShouldBe(new[] { a1, b1, b2, c1, d1, d2 }, true);
            }

            [Test]
            public void ThrowsIfCurrentPipelineIsOsolated()
            {
                // Given
                ConcurrentDictionary<string, ImmutableArray<IDocument>> documents =
                    new ConcurrentDictionary<string, ImmutableArray<IDocument>>(StringComparer.OrdinalIgnoreCase);
                IPipelineCollection pipelines = new PipelineCollection();
                PipelinePhase phase = GetPipelineAndPhase("A", Phase.Process, pipelines, documents, Array.Empty<IDocument>());
                phase.Pipeline.Isolated = true;
                DocumentCollection documentCollection = new DocumentCollection(documents, phase, pipelines);

                // When, Then
                Should.Throw<InvalidOperationException>(() => documentCollection.ToArray());
            }

            [Test]
            public void ThrowsIfInputPhase()
            {
                // Given
                ConcurrentDictionary<string, ImmutableArray<IDocument>> documents =
                    new ConcurrentDictionary<string, ImmutableArray<IDocument>>(StringComparer.OrdinalIgnoreCase);
                IPipelineCollection pipelines = new PipelineCollection();
                PipelinePhase phase = GetPipelineAndPhase("A", Phase.Input, pipelines, documents, Array.Empty<IDocument>());
                phase.Pipeline.Isolated = true;
                DocumentCollection documentCollection = new DocumentCollection(documents, phase, pipelines);

                // When, Then
                Should.Throw<InvalidOperationException>(() => documentCollection.ToArray());
            }

            [Test]
            public void ThrowsIfOutputPhase()
            {
                // Given
                ConcurrentDictionary<string, ImmutableArray<IDocument>> documents =
                    new ConcurrentDictionary<string, ImmutableArray<IDocument>>(StringComparer.OrdinalIgnoreCase);
                IPipelineCollection pipelines = new PipelineCollection();
                PipelinePhase phase = GetPipelineAndPhase("A", Phase.Output, pipelines, documents, Array.Empty<IDocument>());
                phase.Pipeline.Isolated = true;
                DocumentCollection documentCollection = new DocumentCollection(documents, phase, pipelines);

                // When, Then
                Should.Throw<InvalidOperationException>(() => documentCollection.ToArray());
            }
        }

        public class ExceptPipelineTests : DocumentCollectionFixture
        {
            [Test]
            public void ExcludeCurrentPipelineDuringProcess()
            {
                // Given
                TestDocument a1 = new TestDocument("a1");
                TestDocument b1 = new TestDocument("b1");
                TestDocument b2 = new TestDocument("b2");
                TestDocument c1 = new TestDocument("c1");
                TestDocument d1 = new TestDocument("d1");
                TestDocument d2 = new TestDocument("d2");
                ConcurrentDictionary<string, ImmutableArray<IDocument>> documents =
                    new ConcurrentDictionary<string, ImmutableArray<IDocument>>(StringComparer.OrdinalIgnoreCase);
                IPipelineCollection pipelines = new PipelineCollection();
                PipelinePhase phaseA =
                    GetPipelineAndPhase("A", Phase.Process, pipelines, documents, new[] { a1 });
                PipelinePhase phaseB =
                    GetPipelineAndPhase("B", Phase.Process, pipelines, documents, new[] { b1, b2 }, phaseA);
                PipelinePhase phaseC =
                    GetPipelineAndPhase("C", Phase.Process, pipelines, documents, new[] { c1 }, phaseB);
                PipelinePhase phaseD =
                    GetPipelineAndPhase("D", Phase.Process, pipelines, documents, new[] { d1, d2 });
                DocumentCollection documentCollection = new DocumentCollection(documents, phaseC, pipelines);

                // When
                IDocument[] result = documentCollection.ExceptPipeline("C").ToArray();

                // Then
                result.ShouldBe(new[] { a1, b1, b2 }, true);
            }

            [Test]
            public void ExcludeCurrentPipelineDuringTransform()
            {
                // Given
                TestDocument a1 = new TestDocument("a1");
                TestDocument b1 = new TestDocument("b1");
                TestDocument b2 = new TestDocument("b2");
                TestDocument c1 = new TestDocument("c1");
                TestDocument d1 = new TestDocument("d1");
                TestDocument d2 = new TestDocument("d2");
                ConcurrentDictionary<string, ImmutableArray<IDocument>> documents =
                    new ConcurrentDictionary<string, ImmutableArray<IDocument>>(StringComparer.OrdinalIgnoreCase);
                IPipelineCollection pipelines = new PipelineCollection();
                PipelinePhase phaseA =
                    GetPipelineAndPhase("A", Phase.Transform, pipelines, documents, new[] { a1 });
                PipelinePhase phaseB =
                    GetPipelineAndPhase("B", Phase.Transform, pipelines, documents, new[] { b1, b2 }, phaseA);
                PipelinePhase phaseC =
                    GetPipelineAndPhase("C", Phase.Transform, pipelines, documents, new[] { c1 }, phaseB);
                PipelinePhase phaseD =
                    GetPipelineAndPhase("D", Phase.Transform, pipelines, documents, new[] { d1, d2 });
                DocumentCollection documentCollection = new DocumentCollection(documents, phaseC, pipelines);

                // When
                IDocument[] result = documentCollection.ExceptPipeline("C").ToArray();

                // Then
                result.ShouldBe(new[] { a1, b1, b2, d1, d2 }, true);
            }
        }

        public class FromPipelineTests : DocumentCollectionFixture
        {
            [Test]
            public void ThrowsForNullPipeline()
            {
                // Given
                ConcurrentDictionary<string, ImmutableArray<IDocument>> documents =
                    new ConcurrentDictionary<string, ImmutableArray<IDocument>>(StringComparer.OrdinalIgnoreCase);
                IPipelineCollection pipelines = new PipelineCollection();
                PipelinePhase phase = GetPipelineAndPhase("A", Phase.Transform, pipelines, documents, Array.Empty<IDocument>());
                DocumentCollection documentCollection = new DocumentCollection(documents, phase, pipelines);

                // When, Then
                Should.Throw<ArgumentException>(() => documentCollection.FromPipeline(null));
            }

            [Test]
            public void ThrowsForEmptyPipeline()
            {
                // Given
                ConcurrentDictionary<string, ImmutableArray<IDocument>> documents =
                    new ConcurrentDictionary<string, ImmutableArray<IDocument>>(StringComparer.OrdinalIgnoreCase);
                IPipelineCollection pipelines = new PipelineCollection();
                PipelinePhase phase = GetPipelineAndPhase("A", Phase.Transform, pipelines, documents, Array.Empty<IDocument>());
                DocumentCollection documentCollection = new DocumentCollection(documents, phase, pipelines);

                // When, Then
                Should.Throw<ArgumentException>(() => documentCollection.FromPipeline(string.Empty));
            }

            [Test]
            public void ThrowsForCurrentPipelineDuringProcess()
            {
                // Given
                ConcurrentDictionary<string, ImmutableArray<IDocument>> documents =
                    new ConcurrentDictionary<string, ImmutableArray<IDocument>>(StringComparer.OrdinalIgnoreCase);
                IPipelineCollection pipelines = new PipelineCollection();
                PipelinePhase phase = GetPipelineAndPhase("A", Phase.Process, pipelines, documents, Array.Empty<IDocument>());
                DocumentCollection documentCollection = new DocumentCollection(documents, phase, pipelines);

                // When, Then
                Should.Throw<InvalidOperationException>(() => documentCollection.FromPipeline("A"));
            }

            [Test]
            public void DoesNotThrowForCurrentPipelineDuringTransform()
            {
                // Given
                ConcurrentDictionary<string, ImmutableArray<IDocument>> documents =
                    new ConcurrentDictionary<string, ImmutableArray<IDocument>>(StringComparer.OrdinalIgnoreCase);
                IPipelineCollection pipelines = new PipelineCollection();
                PipelinePhase phase = GetPipelineAndPhase("A", Phase.Transform, pipelines, documents, Array.Empty<IDocument>());
                DocumentCollection documentCollection = new DocumentCollection(documents, phase, pipelines);

                // When, Then
                Should.NotThrow(() => documentCollection.FromPipeline("A"));
            }

            [Test]
            public void ThrowsForNonExistingPipeline()
            {
                // Given
                TestDocument a1 = new TestDocument("a1");
                TestDocument b1 = new TestDocument("b1");
                TestDocument b2 = new TestDocument("b2");
                TestDocument c1 = new TestDocument("c1");
                TestDocument d1 = new TestDocument("d1");
                TestDocument d2 = new TestDocument("d2");
                ConcurrentDictionary<string, ImmutableArray<IDocument>> documents =
                    new ConcurrentDictionary<string, ImmutableArray<IDocument>>(StringComparer.OrdinalIgnoreCase);
                IPipelineCollection pipelines = new PipelineCollection();
                PipelinePhase phaseA =
                    GetPipelineAndPhase("A", Phase.Process, pipelines, documents, new[] { a1 });
                PipelinePhase phaseB =
                    GetPipelineAndPhase("B", Phase.Process, pipelines, documents, new[] { b1, b2 }, phaseA);
                PipelinePhase phaseC =
                    GetPipelineAndPhase("C", Phase.Process, pipelines, documents, new[] { c1 }, phaseB);
                PipelinePhase phaseD =
                    GetPipelineAndPhase("D", Phase.Process, pipelines, documents, new[] { d1, d2 });
                DocumentCollection documentCollection = new DocumentCollection(documents, phaseC, pipelines);

                // When, Then
                Should.Throw<KeyNotFoundException>(() => documentCollection.FromPipeline("E"));
            }

            [Test]
            public void GetsDocumentsDuringTransformPhase()
            {
                // Given
                TestDocument a1 = new TestDocument("a1");
                TestDocument b1 = new TestDocument("b1");
                TestDocument b2 = new TestDocument("b2");
                TestDocument c1 = new TestDocument("c1");
                TestDocument d1 = new TestDocument("d1");
                TestDocument d2 = new TestDocument("d2");
                ConcurrentDictionary<string, ImmutableArray<IDocument>> documents =
                    new ConcurrentDictionary<string, ImmutableArray<IDocument>>(StringComparer.OrdinalIgnoreCase);
                IPipelineCollection pipelines = new PipelineCollection();
                PipelinePhase phaseA =
                    GetPipelineAndPhase("A", Phase.Transform, pipelines, documents, new[] { a1 });
                PipelinePhase phaseB =
                    GetPipelineAndPhase("B", Phase.Transform, pipelines, documents, new[] { b1, b2 }, phaseA);
                PipelinePhase phaseC =
                    GetPipelineAndPhase("C", Phase.Transform, pipelines, documents, new[] { c1 }, phaseB);
                PipelinePhase phaseD =
                    GetPipelineAndPhase("D", Phase.Transform, pipelines, documents, new[] { d1, d2 });
                DocumentCollection documentCollection = new DocumentCollection(documents, phaseC, pipelines);

                // When
                IDocument[] result = documentCollection.FromPipeline("D").ToArray();

                // Then
                result.ShouldBe(new[] { d1, d2 });
            }

            [Test]
            public void IsCaseInsensitive()
            {
                // Given
                TestDocument a1 = new TestDocument("a1");
                TestDocument b1 = new TestDocument("b1");
                TestDocument b2 = new TestDocument("b2");
                TestDocument c1 = new TestDocument("c1");
                TestDocument d1 = new TestDocument("d1");
                TestDocument d2 = new TestDocument("d2");
                ConcurrentDictionary<string, ImmutableArray<IDocument>> documents =
                    new ConcurrentDictionary<string, ImmutableArray<IDocument>>(StringComparer.OrdinalIgnoreCase);
                IPipelineCollection pipelines = new PipelineCollection();
                PipelinePhase phaseA =
                    GetPipelineAndPhase("A", Phase.Transform, pipelines, documents, new[] { a1 });
                PipelinePhase phaseB =
                    GetPipelineAndPhase("B", Phase.Transform, pipelines, documents, new[] { b1, b2 }, phaseA);
                PipelinePhase phaseC =
                    GetPipelineAndPhase("C", Phase.Transform, pipelines, documents, new[] { c1 }, phaseB);
                PipelinePhase phaseD =
                    GetPipelineAndPhase("D", Phase.Transform, pipelines, documents, new[] { d1, d2 });
                DocumentCollection documentCollection = new DocumentCollection(documents, phaseC, pipelines);

                // When
                IDocument[] result = documentCollection.FromPipeline("d").ToArray();

                // Then
                result.ShouldBe(new[] { d1, d2 });
            }

            [Test]
            public void ThrowsForNonDependentPipelineDuringProcessPhase()
            {
                // Given
                TestDocument a1 = new TestDocument("a1");
                TestDocument b1 = new TestDocument("b1");
                TestDocument b2 = new TestDocument("b2");
                TestDocument c1 = new TestDocument("c1");
                TestDocument d1 = new TestDocument("d1");
                TestDocument d2 = new TestDocument("d2");
                ConcurrentDictionary<string, ImmutableArray<IDocument>> documents =
                    new ConcurrentDictionary<string, ImmutableArray<IDocument>>(StringComparer.OrdinalIgnoreCase);
                IPipelineCollection pipelines = new PipelineCollection();
                PipelinePhase phaseA =
                    GetPipelineAndPhase("A", Phase.Process, pipelines, documents, new[] { a1 });
                PipelinePhase phaseB =
                    GetPipelineAndPhase("B", Phase.Process, pipelines, documents, new[] { b1, b2 }, phaseA);
                PipelinePhase phaseC =
                    GetPipelineAndPhase("C", Phase.Process, pipelines, documents, new[] { c1 }, phaseB);
                PipelinePhase phaseD =
                    GetPipelineAndPhase("D", Phase.Process, pipelines, documents, new[] { d1, d2 });
                DocumentCollection documentCollection = new DocumentCollection(documents, phaseC, pipelines);

                // When, Then
                Should.Throw<InvalidOperationException>(() => documentCollection.FromPipeline("D"));
            }

            [Test]
            public void GetsDocumentsForDependentPipelineDuringProcessPhase()
            {
                // Given
                TestDocument a1 = new TestDocument("a1");
                TestDocument b1 = new TestDocument("b1");
                TestDocument b2 = new TestDocument("b2");
                TestDocument c1 = new TestDocument("c1");
                TestDocument d1 = new TestDocument("d1");
                TestDocument d2 = new TestDocument("d2");
                ConcurrentDictionary<string, ImmutableArray<IDocument>> documents =
                    new ConcurrentDictionary<string, ImmutableArray<IDocument>>(StringComparer.OrdinalIgnoreCase);
                IPipelineCollection pipelines = new PipelineCollection();
                PipelinePhase phaseA =
                    GetPipelineAndPhase("A", Phase.Process, pipelines, documents, new[] { a1 });
                PipelinePhase phaseB =
                    GetPipelineAndPhase("B", Phase.Process, pipelines, documents, new[] { b1, b2 }, phaseA);
                PipelinePhase phaseC =
                    GetPipelineAndPhase("C", Phase.Process, pipelines, documents, new[] { c1 }, phaseB);
                PipelinePhase phaseD =
                    GetPipelineAndPhase("D", Phase.Process, pipelines, documents, new[] { d1, d2 });
                DocumentCollection documentCollection = new DocumentCollection(documents, phaseC, pipelines);

                // When
                IDocument[] result = documentCollection.FromPipeline("B").ToArray();

                // Then
                result.ShouldBe(new[] { b1, b2 });
            }

            [Test]
            public void GetsDocumentsForTransientDependentPipelineDuringProcessPhase()
            {
                // Given
                TestDocument a1 = new TestDocument("a1");
                TestDocument b1 = new TestDocument("b1");
                TestDocument b2 = new TestDocument("b2");
                TestDocument c1 = new TestDocument("c1");
                TestDocument d1 = new TestDocument("d1");
                TestDocument d2 = new TestDocument("d2");
                ConcurrentDictionary<string, ImmutableArray<IDocument>> documents =
                    new ConcurrentDictionary<string, ImmutableArray<IDocument>>(StringComparer.OrdinalIgnoreCase);
                IPipelineCollection pipelines = new PipelineCollection();
                PipelinePhase phaseA =
                    GetPipelineAndPhase("A", Phase.Process, pipelines, documents, new[] { a1 });
                PipelinePhase phaseB =
                    GetPipelineAndPhase("B", Phase.Process, pipelines, documents, new[] { b1, b2 }, phaseA);
                PipelinePhase phaseC =
                    GetPipelineAndPhase("C", Phase.Process, pipelines, documents, new[] { c1 }, phaseB);
                PipelinePhase phaseD =
                    GetPipelineAndPhase("D", Phase.Process, pipelines, documents, new[] { d1, d2 });
                DocumentCollection documentCollection = new DocumentCollection(documents, phaseC, pipelines);

                // When
                IDocument[] result = documentCollection.FromPipeline("A").ToArray();

                // Then
                result.ShouldBe(new[] { a1 });
            }
        }

        private PipelinePhase GetPipelineAndPhase(
            string pipelineName,
            Phase phase,
            IPipelineCollection pipelines,
            ConcurrentDictionary<string, ImmutableArray<IDocument>> documentCollection,
            IDocument[] documents,
            params PipelinePhase[] dependencies)
        {
            TestPipeline pipeline = new TestPipeline();
            PipelinePhase pipelinePhase = new PipelinePhase(pipeline, pipelineName, phase, Array.Empty<IModule>(), dependencies);
            pipelines.Add(pipelineName, pipeline);
            documentCollection.AddOrUpdate(pipelineName, documents.ToImmutableArray(), (_, __) => documents.ToImmutableArray());
            return pipelinePhase;
        }
    }
}
