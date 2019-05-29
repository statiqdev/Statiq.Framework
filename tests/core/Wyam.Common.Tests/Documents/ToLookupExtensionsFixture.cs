using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Wyam.Common.Documents;
using Wyam.Testing;
using Wyam.Testing.Documents;

namespace Wyam.Common.Tests.Documents
{
    [TestFixture]
    [Parallelizable(ParallelScope.Self | ParallelScope.Children)]
    public class ToLookupExtensionsFixture : BaseFixture
    {
        public class ToLookupManyTests : ToLookupExtensionsFixture
        {
            [Test]
            public void ReturnsCorrectLookupOfInt()
            {
                // Given
                IDocument a = new TestDocument("a")
                {
                    { "Numbers", new[] { 1, 2, 3 } }
                };
                IDocument b = new TestDocument("b")
                {
                    { "Numbers", new[] { 2, 3, 4 } }
                };
                IDocument c = new TestDocument("c")
                {
                    { "Numbers", new[] { 3 } }
                };
                IDocument d = new TestDocument("d")
                {
                    { "Numbers", new[] { 4 } }
                };
                List<IDocument> documents = new List<IDocument>() { a, b, c, d };

                // When
                ILookup<int, IDocument> lookup = documents.ToLookupMany<int>("Numbers");

                // Then
                Assert.AreEqual(4, lookup.Count);
                CollectionAssert.AreEquivalent(new[] { a }, lookup[1]);
                CollectionAssert.AreEquivalent(new[] { a, b }, lookup[2]);
                CollectionAssert.AreEquivalent(new[] { a, b, c }, lookup[3]);
                CollectionAssert.AreEquivalent(new[] { b, d }, lookup[4]);
            }

            [Test]
            public void ReturnsCorrectLookupOfString()
            {
                // Given
                IDocument a = new TestDocument("a")
                {
                    { "Numbers", new[] { "1", "2", "3" } }
                };
                IDocument b = new TestDocument("b")
                {
                    { "Numbers", new[] { "2", "3", "4" } }
                };
                IDocument c = new TestDocument("c")
                {
                    { "Numbers", new[] { "3" } }
                };
                IDocument d = new TestDocument("d")
                {
                    { "Numbers", new[] { "4" } }
                };
                List<IDocument> documents = new List<IDocument>() { a, b, c, d };

                // When
                ILookup<string, IDocument> lookup = documents.ToLookupMany<string>("Numbers");

                // Then
                Assert.AreEqual(4, lookup.Count);
                CollectionAssert.AreEquivalent(new[] { a }, lookup["1"]);
                CollectionAssert.AreEquivalent(new[] { a, b }, lookup["2"]);
                CollectionAssert.AreEquivalent(new[] { a, b, c }, lookup["3"]);
                CollectionAssert.AreEquivalent(new[] { b, d }, lookup["4"]);
            }

            [Test]
            public void ReturnsCorrectLookupWithValues()
            {
                // Given
                IDocument a = new TestDocument("a")
                {
                    { "Numbers", new[] { 1, 2, 3 } },
                    { "Colors", "Red" }
                };
                IDocument b = new TestDocument("b")
                {
                    { "Numbers", new[] { 2, 3, 4 } },
                    { "Colors", "Red" }
                };
                IDocument c = new TestDocument("c")
                {
                    { "Numbers", new[] { 3 } },
                    { "Colors", "Green" }
                };
                IDocument d = new TestDocument("d")
                {
                    { "Numbers", new[] { 4 } },
                    { "Colors", "Green" }
                };
                List<IDocument> documents = new List<IDocument>() { a, b, c, d };

                // When
                ILookup<int, string> lookup = documents.ToLookupMany<int, string>("Numbers", "Colors");

                // Then
                Assert.AreEqual(4, lookup.Count);
                CollectionAssert.AreEquivalent(new[] { "Red" }, lookup[1]);
                CollectionAssert.AreEquivalent(new[] { "Red", "Red" }, lookup[2]);
                CollectionAssert.AreEquivalent(new[] { "Red", "Red", "Green" }, lookup[3]);
                CollectionAssert.AreEquivalent(new[] { "Red", "Green" }, lookup[4]);
            }
        }

        public class ToLookupManyToManyTests : ToLookupExtensionsFixture
        {
            [Test]
            public void ReturnsCorrectLookupWithValues()
            {
                // Given
                IDocument a = new TestDocument("a")
                {
                    { "Numbers", new[] { 1, 2, 3 } },
                    { "Colors", new[] { "Red" } }
                };
                IDocument b = new TestDocument("b")
                {
                    { "Numbers", new[] { 2, 3, 4 } },
                    { "Colors", new[] { "Red", "Blue" } }
                };
                IDocument c = new TestDocument("c")
                {
                    { "Numbers", new[] { 3 } },
                    { "Colors", new[] { "Green" } }
                };
                IDocument d = new TestDocument("d")
                {
                    { "Numbers", new[] { 4 } },
                    { "Colors", new[] { "Green", "Blue" } }
                };
                List<IDocument> documents = new List<IDocument>() { a, b, c, d };

                // When
                ILookup<int, string> lookup = documents.ToLookupManyToMany<int, string>("Numbers", "Colors");

                // Then
                Assert.AreEqual(4, lookup.Count);
                CollectionAssert.AreEquivalent(new[] { "Red" }, lookup[1]);
                CollectionAssert.AreEquivalent(new[] { "Red", "Red", "Blue" }, lookup[2]);
                CollectionAssert.AreEquivalent(new[] { "Red", "Red", "Blue", "Green" }, lookup[3]);
                CollectionAssert.AreEquivalent(new[] { "Red", "Blue", "Green", "Blue" }, lookup[4]);
            }
        }
    }
}
