using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using Shouldly;
using Statiq.Common;
using Statiq.Testing;

namespace Statiq.Core.Tests.Documents
{
    [TestFixture]
    public class ProcessPhaseOutputsFixture : BaseFixture
    {
        public class EnumeratorTests : ProcessPhaseOutputsFixture
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
                ConcurrentDictionary<string, PhaseResult[]> phaseResults =
                    new ConcurrentDictionary<string, PhaseResult[]>(StringComparer.OrdinalIgnoreCase);
                IPipelineCollection pipelines = new TestPipelineCollection();
                PipelinePhase phaseA =
                    GetPipelineAndPhase("A", Phase.Process, pipelines, phaseResults, new[] { a1 });
                PipelinePhase phaseB =
                    GetPipelineAndPhase("B", Phase.Process, pipelines, phaseResults, new[] { b1, b2 }, phaseA);
                PipelinePhase phaseC =
                    GetPipelineAndPhase("C", Phase.Process, pipelines, phaseResults, new[] { c1 }, phaseB);
                PipelinePhase phaseD =
                    GetPipelineAndPhase("D", Phase.Process, pipelines, phaseResults, new[] { d1, d2 });
                ProcessPhaseOutputs documentCollection = new ProcessPhaseOutputs(phaseResults, phaseC, pipelines);

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
                ConcurrentDictionary<string, PhaseResult[]> phaseResults =
                    new ConcurrentDictionary<string, PhaseResult[]>(StringComparer.OrdinalIgnoreCase);
                IPipelineCollection pipelines = new TestPipelineCollection();
                PipelinePhase phaseA =
                    GetPipelineAndPhase("A", Phase.Transform, pipelines, phaseResults, new[] { a1 });
                PipelinePhase phaseB =
                    GetPipelineAndPhase("B", Phase.Transform, pipelines, phaseResults, new[] { b1, b2 }, phaseA);
                PipelinePhase phaseC =
                    GetPipelineAndPhase("C", Phase.Transform, pipelines, phaseResults, new[] { c1 }, phaseB);
                PipelinePhase phaseD =
                    GetPipelineAndPhase("D", Phase.Transform, pipelines, phaseResults, new[] { d1, d2 });
                ProcessPhaseOutputs documentCollection = new ProcessPhaseOutputs(phaseResults, phaseC, pipelines);

                // When
                IDocument[] result = documentCollection.ToArray();

                // Then
                result.ShouldBe(new[] { a1, b1, b2, c1, d1, d2 }, true);
            }

            [Test]
            public void ThrowsIfCurrentPipelineIsOsolated()
            {
                // Given
                ConcurrentDictionary<string, PhaseResult[]> phaseResults =
                    new ConcurrentDictionary<string, PhaseResult[]>(StringComparer.OrdinalIgnoreCase);
                IPipelineCollection pipelines = new TestPipelineCollection();
                PipelinePhase phase = GetPipelineAndPhase("A", Phase.Process, pipelines, phaseResults, Array.Empty<IDocument>());
                phase.Pipeline.Isolated = true;
                ProcessPhaseOutputs documentCollection = new ProcessPhaseOutputs(phaseResults, phase, pipelines);

                // When, Then
                Should.Throw<InvalidOperationException>(() => documentCollection.ToArray());
            }

            [Test]
            public void ThrowsIfInputPhase()
            {
                // Given
                ConcurrentDictionary<string, PhaseResult[]> documents =
                    new ConcurrentDictionary<string, PhaseResult[]>(StringComparer.OrdinalIgnoreCase);
                IPipelineCollection pipelines = new TestPipelineCollection();
                PipelinePhase phase = GetPipelineAndPhase("A", Phase.Input, pipelines, documents, Array.Empty<IDocument>());
                phase.Pipeline.Isolated = true;
                ProcessPhaseOutputs documentCollection = new ProcessPhaseOutputs(documents, phase, pipelines);

                // When, Then
                Should.Throw<InvalidOperationException>(() => documentCollection.ToArray());
            }

            [Test]
            public void ThrowsIfOutputPhase()
            {
                // Given
                ConcurrentDictionary<string, PhaseResult[]> phaseResults =
                    new ConcurrentDictionary<string, PhaseResult[]>(StringComparer.OrdinalIgnoreCase);
                IPipelineCollection pipelines = new TestPipelineCollection();
                PipelinePhase phase = GetPipelineAndPhase("A", Phase.Output, pipelines, phaseResults, Array.Empty<IDocument>());
                phase.Pipeline.Isolated = true;
                ProcessPhaseOutputs documentCollection = new ProcessPhaseOutputs(phaseResults, phase, pipelines);

                // When, Then
                Should.Throw<InvalidOperationException>(() => documentCollection.ToArray());
            }
        }

        public class ExceptPipelineTests : ProcessPhaseOutputsFixture
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
                ConcurrentDictionary<string, PhaseResult[]> phaseResults =
                    new ConcurrentDictionary<string, PhaseResult[]>(StringComparer.OrdinalIgnoreCase);
                IPipelineCollection pipelines = new TestPipelineCollection();
                PipelinePhase phaseA =
                    GetPipelineAndPhase("A", Phase.Process, pipelines, phaseResults, new[] { a1 });
                PipelinePhase phaseB =
                    GetPipelineAndPhase("B", Phase.Process, pipelines, phaseResults, new[] { b1, b2 }, phaseA);
                PipelinePhase phaseC =
                    GetPipelineAndPhase("C", Phase.Process, pipelines, phaseResults, new[] { c1 }, phaseB);
                PipelinePhase phaseD =
                    GetPipelineAndPhase("D", Phase.Process, pipelines, phaseResults, new[] { d1, d2 });
                ProcessPhaseOutputs documentCollection = new ProcessPhaseOutputs(phaseResults, phaseC, pipelines);

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
                ConcurrentDictionary<string, PhaseResult[]> phaseResults =
                    new ConcurrentDictionary<string, PhaseResult[]>(StringComparer.OrdinalIgnoreCase);
                IPipelineCollection pipelines = new TestPipelineCollection();
                PipelinePhase phaseA =
                    GetPipelineAndPhase("A", Phase.Transform, pipelines, phaseResults, new[] { a1 });
                PipelinePhase phaseB =
                    GetPipelineAndPhase("B", Phase.Transform, pipelines, phaseResults, new[] { b1, b2 }, phaseA);
                PipelinePhase phaseC =
                    GetPipelineAndPhase("C", Phase.Transform, pipelines, phaseResults, new[] { c1 }, phaseB);
                PipelinePhase phaseD =
                    GetPipelineAndPhase("D", Phase.Transform, pipelines, phaseResults, new[] { d1, d2 });
                ProcessPhaseOutputs documentCollection = new ProcessPhaseOutputs(phaseResults, phaseC, pipelines);

                // When
                IDocument[] result = documentCollection.ExceptPipeline("C").ToArray();

                // Then
                result.ShouldBe(new[] { a1, b1, b2, d1, d2 }, true);
            }
        }

        public class FromPipelineTests : ProcessPhaseOutputsFixture
        {
            [Test]
            public void ThrowsForNullPipeline()
            {
                // Given
                ConcurrentDictionary<string, PhaseResult[]> phaseResults =
                    new ConcurrentDictionary<string, PhaseResult[]>(StringComparer.OrdinalIgnoreCase);
                IPipelineCollection pipelines = new TestPipelineCollection();
                PipelinePhase phase = GetPipelineAndPhase("A", Phase.Transform, pipelines, phaseResults, Array.Empty<IDocument>());
                ProcessPhaseOutputs documentCollection = new ProcessPhaseOutputs(phaseResults, phase, pipelines);

                // When, Then
                Should.Throw<ArgumentException>(() => documentCollection.FromPipeline(null));
            }

            [Test]
            public void ThrowsForEmptyPipeline()
            {
                // Given
                ConcurrentDictionary<string, PhaseResult[]> phaseResults =
                    new ConcurrentDictionary<string, PhaseResult[]>(StringComparer.OrdinalIgnoreCase);
                IPipelineCollection pipelines = new TestPipelineCollection();
                PipelinePhase phase = GetPipelineAndPhase("A", Phase.Transform, pipelines, phaseResults, Array.Empty<IDocument>());
                ProcessPhaseOutputs documentCollection = new ProcessPhaseOutputs(phaseResults, phase, pipelines);

                // When, Then
                Should.Throw<ArgumentException>(() => documentCollection.FromPipeline(string.Empty));
            }

            [Test]
            public void ThrowsForCurrentPipelineDuringProcess()
            {
                // Given
                ConcurrentDictionary<string, PhaseResult[]> phaseResults =
                    new ConcurrentDictionary<string, PhaseResult[]>(StringComparer.OrdinalIgnoreCase);
                IPipelineCollection pipelines = new TestPipelineCollection();
                PipelinePhase phase = GetPipelineAndPhase("A", Phase.Process, pipelines, phaseResults, Array.Empty<IDocument>());
                ProcessPhaseOutputs documentCollection = new ProcessPhaseOutputs(phaseResults, phase, pipelines);

                // When, Then
                Should.Throw<InvalidOperationException>(() => documentCollection.FromPipeline("A"));
            }

            [Test]
            public void DoesNotThrowForCurrentPipelineDuringTransform()
            {
                // Given
                ConcurrentDictionary<string, PhaseResult[]> phaseResults =
                    new ConcurrentDictionary<string, PhaseResult[]>(StringComparer.OrdinalIgnoreCase);
                IPipelineCollection pipelines = new TestPipelineCollection();
                PipelinePhase phase = GetPipelineAndPhase("A", Phase.Transform, pipelines, phaseResults, Array.Empty<IDocument>());
                ProcessPhaseOutputs documentCollection = new ProcessPhaseOutputs(phaseResults, phase, pipelines);

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
                ConcurrentDictionary<string, PhaseResult[]> phaseResults =
                    new ConcurrentDictionary<string, PhaseResult[]>(StringComparer.OrdinalIgnoreCase);
                IPipelineCollection pipelines = new TestPipelineCollection();
                PipelinePhase phaseA =
                    GetPipelineAndPhase("A", Phase.Process, pipelines, phaseResults, new[] { a1 });
                PipelinePhase phaseB =
                    GetPipelineAndPhase("B", Phase.Process, pipelines, phaseResults, new[] { b1, b2 }, phaseA);
                PipelinePhase phaseC =
                    GetPipelineAndPhase("C", Phase.Process, pipelines, phaseResults, new[] { c1 }, phaseB);
                PipelinePhase phaseD =
                    GetPipelineAndPhase("D", Phase.Process, pipelines, phaseResults, new[] { d1, d2 });
                ProcessPhaseOutputs documentCollection = new ProcessPhaseOutputs(phaseResults, phaseC, pipelines);

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
                ConcurrentDictionary<string, PhaseResult[]> phaseResults =
                    new ConcurrentDictionary<string, PhaseResult[]>(StringComparer.OrdinalIgnoreCase);
                IPipelineCollection pipelines = new TestPipelineCollection();
                PipelinePhase phaseA =
                    GetPipelineAndPhase("A", Phase.Transform, pipelines, phaseResults, new[] { a1 });
                PipelinePhase phaseB =
                    GetPipelineAndPhase("B", Phase.Transform, pipelines, phaseResults, new[] { b1, b2 }, phaseA);
                PipelinePhase phaseC =
                    GetPipelineAndPhase("C", Phase.Transform, pipelines, phaseResults, new[] { c1 }, phaseB);
                PipelinePhase phaseD =
                    GetPipelineAndPhase("D", Phase.Transform, pipelines, phaseResults, new[] { d1, d2 });
                ProcessPhaseOutputs documentCollection = new ProcessPhaseOutputs(phaseResults, phaseC, pipelines);

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
                ConcurrentDictionary<string, PhaseResult[]> phaseResults =
                    new ConcurrentDictionary<string, PhaseResult[]>(StringComparer.OrdinalIgnoreCase);
                IPipelineCollection pipelines = new TestPipelineCollection();
                PipelinePhase phaseA =
                    GetPipelineAndPhase("A", Phase.Transform, pipelines, phaseResults, new[] { a1 });
                PipelinePhase phaseB =
                    GetPipelineAndPhase("B", Phase.Transform, pipelines, phaseResults, new[] { b1, b2 }, phaseA);
                PipelinePhase phaseC =
                    GetPipelineAndPhase("C", Phase.Transform, pipelines, phaseResults, new[] { c1 }, phaseB);
                PipelinePhase phaseD =
                    GetPipelineAndPhase("D", Phase.Transform, pipelines, phaseResults, new[] { d1, d2 });
                ProcessPhaseOutputs documentCollection = new ProcessPhaseOutputs(phaseResults, phaseC, pipelines);

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
                ConcurrentDictionary<string, PhaseResult[]> phaseResults =
                    new ConcurrentDictionary<string, PhaseResult[]>(StringComparer.OrdinalIgnoreCase);
                IPipelineCollection pipelines = new TestPipelineCollection();
                PipelinePhase phaseA =
                    GetPipelineAndPhase("A", Phase.Process, pipelines, phaseResults, new[] { a1 });
                PipelinePhase phaseB =
                    GetPipelineAndPhase("B", Phase.Process, pipelines, phaseResults, new[] { b1, b2 }, phaseA);
                PipelinePhase phaseC =
                    GetPipelineAndPhase("C", Phase.Process, pipelines, phaseResults, new[] { c1 }, phaseB);
                PipelinePhase phaseD =
                    GetPipelineAndPhase("D", Phase.Process, pipelines, phaseResults, new[] { d1, d2 });
                ProcessPhaseOutputs documentCollection = new ProcessPhaseOutputs(phaseResults, phaseC, pipelines);

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
                ConcurrentDictionary<string, PhaseResult[]> phaseResults =
                    new ConcurrentDictionary<string, PhaseResult[]>(StringComparer.OrdinalIgnoreCase);
                IPipelineCollection pipelines = new TestPipelineCollection();
                PipelinePhase phaseA =
                    GetPipelineAndPhase("A", Phase.Process, pipelines, phaseResults, new[] { a1 });
                PipelinePhase phaseB =
                    GetPipelineAndPhase("B", Phase.Process, pipelines, phaseResults, new[] { b1, b2 }, phaseA);
                PipelinePhase phaseC =
                    GetPipelineAndPhase("C", Phase.Process, pipelines, phaseResults, new[] { c1 }, phaseB);
                PipelinePhase phaseD =
                    GetPipelineAndPhase("D", Phase.Process, pipelines, phaseResults, new[] { d1, d2 });
                ProcessPhaseOutputs documentCollection = new ProcessPhaseOutputs(phaseResults, phaseC, pipelines);

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
                ConcurrentDictionary<string, PhaseResult[]> phaseResults =
                    new ConcurrentDictionary<string, PhaseResult[]>(StringComparer.OrdinalIgnoreCase);
                IPipelineCollection pipelines = new TestPipelineCollection();
                PipelinePhase phaseA =
                    GetPipelineAndPhase("A", Phase.Process, pipelines, phaseResults, new[] { a1 });
                PipelinePhase phaseB =
                    GetPipelineAndPhase("B", Phase.Process, pipelines, phaseResults, new[] { b1, b2 }, phaseA);
                PipelinePhase phaseC =
                    GetPipelineAndPhase("C", Phase.Process, pipelines, phaseResults, new[] { c1 }, phaseB);
                PipelinePhase phaseD =
                    GetPipelineAndPhase("D", Phase.Process, pipelines, phaseResults, new[] { d1, d2 });
                ProcessPhaseOutputs documentCollection = new ProcessPhaseOutputs(phaseResults, phaseC, pipelines);

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
            ConcurrentDictionary<string, PhaseResult[]> phaseResults,
            IDocument[] outputs,
            params PipelinePhase[] dependencies)
        {
            TestPipeline pipeline = new TestPipeline();
            PipelinePhase pipelinePhase = new PipelinePhase(pipeline, pipelineName, phase, Array.Empty<IModule>(), NullLogger.Instance, dependencies);
            pipelines.Add(pipelineName, pipeline);
            PhaseResult[] results = new PhaseResult[4];
            results[(int)Phase.Process] = new PhaseResult(pipelineName, Phase.Process, outputs.ToImmutableArray(), 0);
            phaseResults.TryAdd(pipelineName, results);
            return pipelinePhase;
        }
    }
}
