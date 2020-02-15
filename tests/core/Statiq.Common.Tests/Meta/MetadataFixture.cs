using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Shouldly;
using Statiq.Testing;

namespace Statiq.Common.Tests.Meta
{
    [TestFixture]
    public class MetadataFixture : BaseFixture
    {
        public class IndexerTests : MetadataFixture
        {
            [Test]
            public void MissingKeyThrowsKeyNotFoundException()
            {
                // Given
                MetadataItems initialMetadata = new MetadataItems();
                Metadata metadata = new Metadata(initialMetadata);

                // When
                TestDelegate test = () =>
                {
                    object value = metadata["A"];
                };

                // Then
                Assert.Throws<KeyNotFoundException>(test);
            }

            [Test]
            public void NullKeyThrowsKeyNotFoundException()
            {
                // Given
                MetadataItems initialMetadata = new MetadataItems();
                Metadata metadata = new Metadata(initialMetadata);

                // When
                TestDelegate test = () =>
                {
                    object value = metadata[null];
                };

                // Then
                Assert.Throws<ArgumentNullException>(test);
            }

            [Test]
            public void ReturnsCorrectResultWithMetadataValue()
            {
                // Given
                MetadataItems initialMetadata = new MetadataItems
                {
                    { "A", new SimpleMetadataValue { Value = "a" } }
                };
                Metadata metadata = new Metadata(initialMetadata);

                // When
                object value = metadata["A"];

                // Then
                Assert.AreEqual("a", value);
            }

            [Test]
            public void ReturnsCorrectResultForKeysWithDifferentCase()
            {
                // Given
                MetadataItems initialMetadata = new MetadataItems
                {
                    { "A", new SimpleMetadataValue { Value = "a" } }
                };
                Metadata metadata = new Metadata(initialMetadata);

                // When
                object value = metadata["a"];

                // Then
                Assert.AreEqual("a", value);
            }
        }

        public class ContainsKeyTests : MetadataFixture
        {
            [Test]
            public void ReturnsTrueForValidValue()
            {
                // Given
                MetadataItems initialMetadata = new MetadataItems { { "A", "a" } };
                Metadata metadata = new Metadata(initialMetadata);

                // When
                bool contains = metadata.ContainsKey("A");

                // Then
                Assert.IsTrue(contains);
            }

            [Test]
            public void ReturnsFalseForInvalidValue()
            {
                // Given
                MetadataItems initialMetadata = new MetadataItems { { "A", "a" } };
                Metadata metadata = new Metadata(initialMetadata);

                // When
                bool contains = metadata.ContainsKey("B");

                // Then
                Assert.IsFalse(contains);
            }

            [Test]
            public void ReturnsTrueForSameKeysWithDifferentCase()
            {
                // Given
                MetadataItems initialMetadata = new MetadataItems { { "A", "a" } };
                Metadata metadata = new Metadata(initialMetadata);

                // When
                bool contains = metadata.ContainsKey("a");

                // Then
                Assert.IsTrue(contains);
            }
        }

        public class TryGetValueTests : MetadataFixture
        {
            [Test]
            public void ReturnsTrueForValidValue()
            {
                // Given
                MetadataItems initialMetadata = new MetadataItems { { "A", "a" } };
                Metadata metadata = new Metadata(initialMetadata);

                // When
                object value;
                bool contains = metadata.TryGetValue("A", out value);

                // Then
                Assert.IsTrue(contains);
                Assert.AreEqual("a", value);
            }

            [Test]
            public void ReturnsFalseForInvalidValue()
            {
                // Given
                MetadataItems initialMetadata = new MetadataItems { { "A", "a" } };
                Metadata metadata = new Metadata(initialMetadata);

                // When
                object value;
                bool contains = metadata.TryGetValue("B", out value);

                // Then
                Assert.IsFalse(contains);
                Assert.AreEqual(null, value);
            }

            [Test]
            public void ReturnsCorrectResultWithMetadataValue()
            {
                // Given
                MetadataItems initialMetadata = new MetadataItems
                {
                    { "A", new SimpleMetadataValue { Value = "a" } }
                };
                Metadata metadata = new Metadata(initialMetadata);

                // When
                object value;
                bool contains = metadata.TryGetValue("A", out value);

                // Then
                Assert.IsTrue(contains);
                Assert.AreEqual("a", value);
            }
        }

        public class CloneTests : MetadataFixture
        {
            [Test]
            public void CanCloneWithNewValues()
            {
                // Given
                MetadataItems initialMetadata = new MetadataItems();
                Metadata metadata = new Metadata(initialMetadata);

                // When
                metadata = new Metadata(metadata, new[] { new KeyValuePair<string, object>("A", "a") });

                // Then
                Assert.AreEqual("a", metadata["A"]);
            }

            [Test]
            public void ContainsPreviousValues()
            {
                // Given
                MetadataItems initialMetadata = new MetadataItems { { "A", "a" } };
                Metadata metadata = new Metadata(initialMetadata);

                // When
                Metadata clone = new Metadata(metadata, new Dictionary<string, object> { { "B", "b" } });

                // Then
                Assert.AreEqual("a", clone["A"]);
            }

            [Test]
            public void ClonedMetadataDoesNotContainNewValues()
            {
                // Given
                MetadataItems initialMetadata = new MetadataItems { { "A", "a" } };
                Metadata metadata = new Metadata(initialMetadata);

                // When
                Metadata clone = new Metadata(metadata, new Dictionary<string, object> { { "B", "b" } });

                // Then
                Assert.IsFalse(metadata.ContainsKey("B"));
            }

            [Test]
            public void ContainsNewValues()
            {
                // Given
                MetadataItems initialMetadata = new MetadataItems { { "A", "a" } };
                Metadata metadata = new Metadata(initialMetadata);

                // When
                Metadata clone = new Metadata(metadata, new Dictionary<string, object> { { "B", "b" } });

                // Then
                Assert.AreEqual("b", clone["B"]);
            }

            [Test]
            public void ReplacesValue()
            {
                // Given
                MetadataItems initialMetadata = new MetadataItems { { "A", "a" } };
                Metadata metadata = new Metadata(initialMetadata);

                // When
                Metadata clone = new Metadata(metadata, new Dictionary<string, object> { { "A", "b" } });

                // Then
                Assert.AreEqual("a", metadata["A"]);
                Assert.AreEqual("b", clone["A"]);
            }
        }

        public class GetTests : MetadataFixture
        {
            [Test]
            public void GetWithMetadataValueReturnsCorrectResult()
            {
                // Given
                MetadataItems initialMetadata = new MetadataItems { { "A", "a" } };
                IMetadata metadata = new Metadata(initialMetadata);

                // When
                object value = metadata.Get("A");

                // Then
                Assert.AreEqual("a", value);
            }

            [Test]
            public void ReturnsCorrectResultWithDerivedMetadataValue()
            {
                // Given
                MetadataItems initialMetadata = new MetadataItems
                {
                    { "A", new DerivedMetadataValue { Key = "X" } },
                    { "X", "x" }
                };
                IMetadata metadata = new Metadata(initialMetadata);

                // When
                object value = metadata.Get("A");

                // Then
                Assert.AreEqual("x", value);
            }

            [Test]
            public void MetadataValueCalledForEachRequest()
            {
                // Given
                SimpleMetadataValue metadataValue = new SimpleMetadataValue { Value = "a" };
                MetadataItems initialMetadata = new MetadataItems { { "A", metadataValue } };
                IMetadata metadata = new Metadata(initialMetadata);

                // When
                object value = metadata.Get("A");
                value = metadata.Get("A");
                value = metadata.Get("A");

                // Then
                Assert.AreEqual("a", value);
                Assert.AreEqual(3, metadataValue.Calls);
            }
        }

        public class ListTests : MetadataFixture
        {
            [Test]
            public void ReturnsCorrectResultForList()
            {
                // Given
                MetadataItems initialMetadata = new MetadataItems { { "A", new List<int> { 1, 2, 3 } } };
                IMetadata metadata = new Metadata(initialMetadata);

                // When
                IReadOnlyList<int> result = metadata.GetList<int>("A");

                // Then
                Assert.IsNotNull(result);
                CollectionAssert.AreEqual(result, new[] { 1, 2, 3 });
            }

            [Test]
            public void ReturnsCorrectResultForConvertedStringList()
            {
                // Given
                MetadataItems initialMetadata = new MetadataItems { { "A", new List<string> { "1", "2", "3" } } };
                IMetadata metadata = new Metadata(initialMetadata);

                // When
                IReadOnlyList<int> result = metadata.GetList<int>("A");

                // Then
                Assert.IsNotNull(result);
                CollectionAssert.AreEqual(result, new[] { 1, 2, 3 });
            }

            [Test]
            public void ReturnsCorrectResultForConvertedIntList()
            {
                // Given
                MetadataItems initialMetadata = new MetadataItems { { "A", new List<int> { 1, 2, 3 } } };
                IMetadata metadata = new Metadata(initialMetadata);

                // When
                IReadOnlyList<string> result = metadata.GetList<string>("A");

                // Then
                Assert.IsNotNull(result);
                CollectionAssert.AreEqual(result, new[] { "1", "2", "3" });
            }

            [Test]
            public void ReturnsCorrectResultForArray()
            {
                // Given
                MetadataItems initialMetadata = new MetadataItems { { "A", new[] { 1, 2, 3 } } };
                IMetadata metadata = new Metadata(initialMetadata);

                // When
                IReadOnlyList<int> result = metadata.GetList<int>("A");

                // Then
                Assert.IsNotNull(result);
                CollectionAssert.AreEqual(result, new[] { 1, 2, 3 });
            }
        }

        public class DocumentListTests : MetadataFixture
        {
            [Test]
            public void ReturnsNullWhenKeyNotFound()
            {
                // Given
                MetadataItems initialMetadata = new MetadataItems();
                IMetadata metadata = new Metadata(initialMetadata);

                // When
                IReadOnlyList<IDocument> result = metadata.GetDocumentList("A");

                // Then
                Assert.IsNull(result);
            }

            [Test]
            public void ReturnsListForList()
            {
                // Given
                IDocument a = new TestDocument();
                IDocument b = new TestDocument();
                IDocument c = new TestDocument();
                MetadataItems initialMetadata = new MetadataItems { { "A", new List<IDocument> { a, b, c } } };
                IMetadata metadata = new Metadata(initialMetadata);

                // When
                IReadOnlyList<IDocument> result = metadata.GetDocumentList("A");

                // Then
                Assert.IsNotNull(result);
                CollectionAssert.AreEqual(new[] { a, b, c }, result);
            }

            [Test]
            public void ReturnsEmptyListForListOfInt()
            {
                // Given
                MetadataItems initialMetadata = new MetadataItems { { "A", new List<int> { 1, 2, 3 } } };
                IMetadata metadata = new Metadata(initialMetadata);

                // When
                IReadOnlyList<IDocument> result = metadata.GetDocumentList("A");

                // Then
                Assert.IsNotNull(result);
                CollectionAssert.IsEmpty(result);
            }

            [Test]
            public void ReturnsEmptyListForSingleInt()
            {
                // Given
                MetadataItems initialMetadata = new MetadataItems { { "A", 1 } };
                IMetadata metadata = new Metadata(initialMetadata);

                // When
                IReadOnlyList<IDocument> result = metadata.GetDocumentList("A");

                // Then
                Assert.IsNotNull(result);
                CollectionAssert.IsEmpty(result);
            }
        }

        public class StringTests : MetadataFixture
        {
            [TestCase("/a/b/c.txt", "/a/b/c.txt")]
            [TestCase("a/b/c.txt", "a/b/c.txt")]
            public void ReturnsCorrectStringForFilePath(string path, string expected)
            {
                // Given
                MetadataItems initialMetadata = new MetadataItems();
                IMetadata metadata = new Metadata(initialMetadata);

                // When
                metadata = new Metadata(metadata, new[] { new KeyValuePair<string, object>("A", new NormalizedPath(path)) });
                object result = metadata.GetString("A");

                // Then
                Assert.IsInstanceOf<string>(result);
                Assert.AreEqual(expected, result);
            }

            [TestCase("/a/b/c", "/a/b/c")]
            [TestCase("a/b/c", "a/b/c")]
            public void ReturnsCorrectStringForDirectoryPath(string path, string expected)
            {
                // Given
                MetadataItems initialMetadata = new MetadataItems();
                IMetadata metadata = new Metadata(initialMetadata);

                // When
                metadata = new Metadata(metadata, new[] { new KeyValuePair<string, object>("A", new NormalizedPath(path)) });
                object result = metadata.GetString("A");

                // Then
                Assert.IsInstanceOf<string>(result);
                Assert.AreEqual(expected, result);
            }
        }

        public class FilePathTests : MetadataFixture
        {
            [TestCase("/a/b/c.txt", "/a/b/c.txt")]
            [TestCase("a/b/c.txt", "a/b/c.txt")]
            public void ReturnsCorrectFilePathForFilePath(string path, string expected)
            {
                // Given
                MetadataItems initialMetadata = new MetadataItems();
                IMetadata metadata = new Metadata(initialMetadata);

                // When
                metadata = new Metadata(metadata, new[] { new KeyValuePair<string, object>("A", new NormalizedPath(path)) });
                object result = metadata.GetFilePath("A");

                // Then
                Assert.IsInstanceOf<NormalizedPath>(result);
                Assert.AreEqual(expected, ((NormalizedPath)result).FullPath);
            }

            [TestCase("/a/b/c.txt", "/a/b/c.txt")]
            [TestCase("a/b/c.txt", "a/b/c.txt")]
            [TestCase(null, null)]
            public void ReturnsCorrectFilePathForString(string path, string expected)
            {
                // Given
                MetadataItems initialMetadata = new MetadataItems();
                IMetadata metadata = new Metadata(initialMetadata);

                // When
                metadata = new Metadata(metadata, new[] { new KeyValuePair<string, object>("A", path) });
                object result = metadata.GetFilePath("A");

                // Then
                if (expected == null)
                {
                    Assert.IsNull(result);
                }
                else
                {
                    Assert.IsInstanceOf<NormalizedPath>(result);
                    Assert.AreEqual(expected, ((NormalizedPath)result).FullPath);
                }
            }
        }

        public class DirectoryPathTests : MetadataFixture
        {
            [TestCase("/a/b/c", "/a/b/c")]
            [TestCase("a/b/c", "a/b/c")]
            public void ReturnsCorrectDirectoryPathForDirectoryPath(string path, string expected)
            {
                // Given
                MetadataItems initialMetadata = new MetadataItems();
                IMetadata metadata = new Metadata(initialMetadata);

                // When
                metadata = new Metadata(metadata, new[] { new KeyValuePair<string, object>("A", new NormalizedPath(path)) });
                object result = metadata.GetDirectoryPath("A");

                // Then
                Assert.IsInstanceOf<NormalizedPath>(result);
                Assert.AreEqual(expected, ((NormalizedPath)result).FullPath);
            }

            [TestCase("/a/b/c", "/a/b/c")]
            [TestCase("a/b/c", "a/b/c")]
            [TestCase(null, null)]
            public void ReturnsCorrectDirectoryPathForString(string path, string expected)
            {
                // Given
                MetadataItems initialMetadata = new MetadataItems();
                IMetadata metadata = new Metadata(initialMetadata);

                // When
                metadata = new Metadata(metadata, new[] { new KeyValuePair<string, object>("A", path) });
                object result = metadata.GetDirectoryPath("A");

                // Then
                if (expected == null)
                {
                    Assert.IsNull(result);
                }
                else
                {
                    Assert.IsInstanceOf<NormalizedPath>(result);
                    Assert.AreEqual(expected, ((NormalizedPath)result).FullPath);
                }
            }
        }

        public class EnumeratorTests : MetadataFixture
        {
            [Test]
            public void EnumeratingMetadataValuesReturnsCorrectResults()
            {
                // Given
                MetadataItems initialMetadata = new MetadataItems
                {
                    { "A", new SimpleMetadataValue { Value = "a" } },
                    { "B", new SimpleMetadataValue { Value = "b" } },
                    { "C", new SimpleMetadataValue { Value = "c" } }
                };
                Metadata metadata = new Metadata(initialMetadata);

                // When
                object[] values = metadata.Select(x => x.Value).ToArray();

                // Then
                CollectionAssert.AreEquivalent(new[] { "a", "b", "c" }, values);
            }
        }

        public class CountTests : MetadataFixture
        {
            [Test]
            public void GetsCorrectCount()
            {
                // Given
                MetadataItems initialMetadata = new MetadataItems
                {
                    { "A", new SimpleMetadataValue { Value = "a" } },
                    { "B", new SimpleMetadataValue { Value = "b" } },
                    { "C", new SimpleMetadataValue { Value = "c" } }
                };
                Metadata metadata = new Metadata(initialMetadata);

                // When
                int count = metadata.Count;

                // Then
                count.ShouldBe(3);
            }

            [Test]
            public void GetsCorrectCountWithPrevious()
            {
                // Given
                MetadataItems initialMetadata = new MetadataItems
                {
                    { "A", new SimpleMetadataValue { Value = "a" } },
                    { "B", new SimpleMetadataValue { Value = "b" } },
                    { "C", new SimpleMetadataValue { Value = "c" } }
                };
                MetadataItems newMetadata = new MetadataItems
                {
                    { "A", new SimpleMetadataValue { Value = "a" } },
                    { "D", new SimpleMetadataValue { Value = "b" } },
                    { "E", new SimpleMetadataValue { Value = "c" } }
                };
                Metadata previous = new Metadata(initialMetadata);
                Metadata metadata = new Metadata(previous, newMetadata);

                // When
                int count = metadata.Count;

                // Then
                count.ShouldBe(5);
            }
        }

        private class SimpleMetadataValue : IMetadataValue
        {
            public object Value { get; set; }
            public int Calls { get; set; }

            object IMetadataValue.Get(IMetadata metadata)
            {
                Calls++;
                return Value;
            }
        }

        private class DerivedMetadataValue : IMetadataValue
        {
            public string Key { get; set; }

            object IMetadataValue.Get(IMetadata metadata)
            {
                return metadata[Key];
            }
        }
    }
}
