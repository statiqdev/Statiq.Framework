using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Shouldly;
using Statiq.Common.Configuration;
using Statiq.Common.Meta;
using Statiq.Core.Modules.Control;
using Statiq.Testing;
using Statiq.Testing.Documents;

namespace Statiq.Core.Tests.Modules.Control
{
    [TestFixture]
    public class FilterDocumentsFixture : BaseFixture
    {
        public class ExecuteTests : FilterDocumentsFixture
        {
            [Test]
            public async Task AppliesNoFilteringWithoutPredicate()
            {
                // Given
                TestDocument a = new TestDocument()
                {
                    { "Foo", "a" }
                };
                TestDocument b = new TestDocument()
                {
                    { "Foo", "b" }
                };
                TestDocument c = new TestDocument();
                FilterDocuments filter = new FilterDocuments();

                // When
                IReadOnlyList<TestDocument> results = await ExecuteAsync(new[] { a, b, c }, filter);

                // Then
                results.Count.ShouldBe(3);
            }

            [Test]
            public async Task FiltersWithSingePredicate()
            {
                // Given
                TestDocument a = new TestDocument()
                {
                    { "Foo", "a" }
                };
                TestDocument b = new TestDocument()
                {
                    { "Foo", "b" }
                };
                TestDocument c = new TestDocument();
                FilterDocuments filter = new FilterDocuments(Config.FromDocument(x => x.String("Foo") == "b"));

                // When
                IReadOnlyList<TestDocument> results = await ExecuteAsync(new[] { a, b, c }, filter);

                // Then
                results.Count.ShouldBe(1);
            }

            [Test]
            public async Task FiltersWithMultiplePredicates()
            {
                // Given
                TestDocument a = new TestDocument()
                {
                    { "Foo", "a" }
                };
                TestDocument b = new TestDocument()
                {
                    { "Foo", "b" }
                };
                TestDocument c = new TestDocument();
                FilterDocuments filter = new FilterDocuments(
                    Config.FromDocument(x => x.String("Foo") == "b"))
                    .Or(Config.FromDocument(x => x.String("Foo") == "a"));

                // When
                IReadOnlyList<TestDocument> results = await ExecuteAsync(new[] { a, b, c }, filter);

                // Then
                results.Count.ShouldBe(2);
            }

            [Test]
            public async Task FiltersWithFalsePredicate()
            {
                // Given
                TestDocument a = new TestDocument()
                {
                    { "Foo", "a" }
                };
                TestDocument b = new TestDocument()
                {
                    { "Foo", "b" }
                };
                TestDocument c = new TestDocument();
                FilterDocuments filter = new FilterDocuments(
                    Config.FromDocument(x => x.String("Foo") == "c"))
                    .Or(Config.FromDocument(x => x.String("Foo") == "a"));

                // When
                IReadOnlyList<TestDocument> results = await ExecuteAsync(new[] { a, b, c }, filter);

                // Then
                results.Count.ShouldBe(1);
            }

            [Test]
            public async Task FiltersWithSingePredicateNoMatches()
            {
                // Given
                TestDocument a = new TestDocument()
                {
                    { "Foo", "a" }
                };
                TestDocument b = new TestDocument()
                {
                    { "Foo", "b" }
                };
                TestDocument c = new TestDocument();
                FilterDocuments filter = new FilterDocuments(Config.FromDocument(x => x.String("Foo") == "c"));

                // When
                IReadOnlyList<TestDocument> results = await ExecuteAsync(new[] { a, b, c }, filter);

                // Then
                results.ShouldBeEmpty();
            }
        }
    }
}
