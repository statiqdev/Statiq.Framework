using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Statiq.Common.Documents;
using Statiq.Common.IO;
using Statiq.Common.Meta;
using Statiq.Testing;
using Statiq.Testing.Documents;
using Statiq.Testing.Execution;

namespace Statiq.Common.Tests.Meta
{
    [TestFixture]
    [Parallelizable(ParallelScope.Self | ParallelScope.Children)]
    public class MetadataFixture : BaseFixture
    {
        public class IndexerTests : MetadataFixture
        {
            [Test]
            public void MissingKeyThrowsKeyNotFoundException()
            {
                // Given
                TestEngine engine = new TestEngine();
                MetadataItems initialMetadata = new MetadataItems();
                Metadata metadata = new Metadata(engine, initialMetadata);

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
                TestEngine engine = new TestEngine();
                MetadataItems initialMetadata = new MetadataItems();
                Metadata metadata = new Metadata(engine, initialMetadata);

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
                TestEngine engine = new TestEngine();
                MetadataItems initialMetadata = new MetadataItems
                {
                    { "A", new SimpleMetadataValue { Value = "a" } }
                };
                Metadata metadata = new Metadata(engine, initialMetadata);

                // When
                object value = metadata["A"];

                // Then
                Assert.AreEqual("a", value);
            }

            [Test]
            public void ReturnsCorrectResultForKeysWithDifferentCase()
            {
                // Given
                TestEngine engine = new TestEngine();
                MetadataItems initialMetadata = new MetadataItems
                {
                    { "A", new SimpleMetadataValue { Value = "a" } }
                };
                Metadata metadata = new Metadata(engine, initialMetadata);

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
                TestEngine engine = new TestEngine();
                MetadataItems initialMetadata = new MetadataItems { { "A", "a" } };
                Metadata metadata = new Metadata(engine, initialMetadata);

                // When
                bool contains = metadata.ContainsKey("A");

                // Then
                Assert.IsTrue(contains);
            }

            [Test]
            public void ReturnsFalseForInvalidValue()
            {
                // Given
                TestEngine engine = new TestEngine();
                MetadataItems initialMetadata = new MetadataItems { { "A", "a" } };
                Metadata metadata = new Metadata(engine, initialMetadata);

                // When
                bool contains = metadata.ContainsKey("B");

                // Then
                Assert.IsFalse(contains);
            }

            [Test]
            public void ReturnsTrueForSameKeysWithDifferentCase()
            {
                // Given
                TestEngine engine = new TestEngine();
                MetadataItems initialMetadata = new MetadataItems { { "A", "a" } };
                Metadata metadata = new Metadata(engine, initialMetadata);

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
                TestEngine engine = new TestEngine();
                MetadataItems initialMetadata = new MetadataItems { { "A", "a" } };
                Metadata metadata = new Metadata(engine, initialMetadata);

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
                TestEngine engine = new TestEngine();
                MetadataItems initialMetadata = new MetadataItems { { "A", "a" } };
                Metadata metadata = new Metadata(engine, initialMetadata);

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
                TestEngine engine = new TestEngine();
                MetadataItems initialMetadata = new MetadataItems
                {
                    { "A", new SimpleMetadataValue { Value = "a" } }
                };
                Metadata metadata = new Metadata(engine, initialMetadata);

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
                TestEngine engine = new TestEngine();
                MetadataItems initialMetadata = new MetadataItems();
                Metadata metadata = new Metadata(engine, initialMetadata);

                // When
                metadata = new Metadata(engine, metadata, new[] { new KeyValuePair<string, object>("A", "a") });

                // Then
                Assert.AreEqual("a", metadata["A"]);
            }

            [Test]
            public void ContainsPreviousValues()
            {
                // Given
                TestEngine engine = new TestEngine();
                MetadataItems initialMetadata = new MetadataItems { { "A", "a" } };
                Metadata metadata = new Metadata(engine, initialMetadata);

                // When
                Metadata clone = new Metadata(engine, metadata, new Dictionary<string, object> { { "B", "b" } });

                // Then
                Assert.AreEqual("a", clone["A"]);
            }

            [Test]
            public void ClonedMetadataDoesNotContainNewValues()
            {
                // Given
                TestEngine engine = new TestEngine();
                MetadataItems initialMetadata = new MetadataItems { { "A", "a" } };
                Metadata metadata = new Metadata(engine, initialMetadata);

                // When
                Metadata clone = new Metadata(engine, metadata, new Dictionary<string, object> { { "B", "b" } });

                // Then
                Assert.IsFalse(metadata.ContainsKey("B"));
            }

            [Test]
            public void ContainsNewValues()
            {
                // Given
                TestEngine engine = new TestEngine();
                MetadataItems initialMetadata = new MetadataItems { { "A", "a" } };
                Metadata metadata = new Metadata(engine, initialMetadata);

                // When
                Metadata clone = new Metadata(engine, metadata, new Dictionary<string, object> { { "B", "b" } });

                // Then
                Assert.AreEqual("b", clone["B"]);
            }

            [Test]
            public void ReplacesValue()
            {
                // Given
                TestEngine engine = new TestEngine();
                MetadataItems initialMetadata = new MetadataItems { { "A", "a" } };
                Metadata metadata = new Metadata(engine, initialMetadata);

                // When
                Metadata clone = new Metadata(engine, metadata, new Dictionary<string, object> { { "A", "b" } });

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
                TestEngine engine = new TestEngine();
                MetadataItems initialMetadata = new MetadataItems { { "A", "a" } };
                Metadata metadata = new Metadata(engine, initialMetadata);

                // When
                object value = metadata.Get("A");

                // Then
                Assert.AreEqual("a", value);
            }

            [Test]
            public void ReturnsCorrectResultWithDerivedMetadataValue()
            {
                // Given
                TestEngine engine = new TestEngine();
                MetadataItems initialMetadata = new MetadataItems
                {
                    { "A", new DerivedMetadataValue { Key = "X" } },
                    { "X", "x" }
                };
                Metadata metadata = new Metadata(engine, initialMetadata);

                // When
                object value = metadata.Get("A");

                // Then
                Assert.AreEqual("x", value);
            }

            [Test]
            public void MetadataValueCalledForEachRequest()
            {
                // Given
                TestEngine engine = new TestEngine();
                SimpleMetadataValue metadataValue = new SimpleMetadataValue { Value = "a" };
                MetadataItems initialMetadata = new MetadataItems { { "A", metadataValue } };
                Metadata metadata = new Metadata(engine, initialMetadata);

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
                TestEngine engine = new TestEngine();
                MetadataItems initialMetadata = new MetadataItems { { "A", new List<int> { 1, 2, 3 } } };
                Metadata metadata = new Metadata(engine, initialMetadata);

                // When
                IReadOnlyList<int> result = metadata.List<int>("A");

                // Then
                Assert.IsNotNull(result);
                CollectionAssert.AreEqual(result, new[] { 1, 2, 3 });
            }

            [Test]
            public void ReturnsCorrectResultForArray()
            {
                // Given
                TestEngine engine = new TestEngine();
                MetadataItems initialMetadata = new MetadataItems { { "A", new[] { 1, 2, 3 } } };
                Metadata metadata = new Metadata(engine, initialMetadata);

                // When
                IReadOnlyList<int> result = metadata.List<int>("A");

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
                TestEngine engine = new TestEngine();
                MetadataItems initialMetadata = new MetadataItems();
                Metadata metadata = new Metadata(engine, initialMetadata);

                // When
                IReadOnlyList<IDocument> result = metadata.DocumentList("A");

                // Then
                Assert.IsNull(result);
            }

            [Test]
            public void ReturnsListForList()
            {
                // Given
                TestEngine engine = new TestEngine();
                IDocument a = new TestDocument();
                IDocument b = new TestDocument();
                IDocument c = new TestDocument();
                MetadataItems initialMetadata = new MetadataItems { { "A", new List<IDocument> { a, b, c } } };
                Metadata metadata = new Metadata(engine, initialMetadata);

                // When
                IReadOnlyList<IDocument> result = metadata.DocumentList("A");

                // Then
                Assert.IsNotNull(result);
                CollectionAssert.AreEqual(new[] { a, b, c }, result);
            }
        }

        public class StringTests : MetadataFixture
        {
            [TestCase("/a/b/c.txt", "/a/b/c.txt")]
            [TestCase("a/b/c.txt", "a/b/c.txt")]
            public void ReturnsCorrectStringForFilePath(string path, string expected)
            {
                // Given
                TestEngine engine = new TestEngine();
                MetadataItems initialMetadata = new MetadataItems();
                Metadata metadata = new Metadata(engine, initialMetadata);

                // When
                metadata = new Metadata(engine, metadata, new[] { new KeyValuePair<string, object>("A", new FilePath(path)) });
                object result = metadata.String("A");

                // Then
                Assert.IsInstanceOf<string>(result);
                Assert.AreEqual(expected, result);
            }

            [TestCase("/a/b/c", "/a/b/c")]
            [TestCase("a/b/c", "a/b/c")]
            public void ReturnsCorrectStringForDirectoryPath(string path, string expected)
            {
                // Given
                TestEngine engine = new TestEngine();
                MetadataItems initialMetadata = new MetadataItems();
                Metadata metadata = new Metadata(engine, initialMetadata);

                // When
                metadata = new Metadata(engine, metadata, new[] { new KeyValuePair<string, object>("A", new DirectoryPath(path)) });
                object result = metadata.String("A");

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
                TestEngine engine = new TestEngine();
                MetadataItems initialMetadata = new MetadataItems();
                Metadata metadata = new Metadata(engine, initialMetadata);

                // When
                metadata = new Metadata(engine, metadata, new[] { new KeyValuePair<string, object>("A", new FilePath(path)) });
                object result = metadata.FilePath("A");

                // Then
                Assert.IsInstanceOf<FilePath>(result);
                Assert.AreEqual(expected, ((FilePath)result).FullPath);
            }

            [TestCase("/a/b/c.txt", "/a/b/c.txt")]
            [TestCase("a/b/c.txt", "a/b/c.txt")]
            [TestCase(null, null)]
            public void ReturnsCorrectFilePathForString(string path, string expected)
            {
                // Given
                TestEngine engine = new TestEngine();
                MetadataItems initialMetadata = new MetadataItems();
                Metadata metadata = new Metadata(engine, initialMetadata);

                // When
                metadata = new Metadata(engine, metadata, new[] { new KeyValuePair<string, object>("A", path) });
                object result = metadata.FilePath("A");

                // Then
                if (expected == null)
                {
                    Assert.IsNull(result);
                }
                else
                {
                    Assert.IsInstanceOf<FilePath>(result);
                    Assert.AreEqual(expected, ((FilePath)result).FullPath);
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
                TestEngine engine = new TestEngine();
                MetadataItems initialMetadata = new MetadataItems();
                Metadata metadata = new Metadata(engine, initialMetadata);

                // When
                metadata = new Metadata(engine, metadata, new[] { new KeyValuePair<string, object>("A", new DirectoryPath(path)) });
                object result = metadata.DirectoryPath("A");

                // Then
                Assert.IsInstanceOf<DirectoryPath>(result);
                Assert.AreEqual(expected, ((DirectoryPath)result).FullPath);
            }

            [TestCase("/a/b/c", "/a/b/c")]
            [TestCase("a/b/c", "a/b/c")]
            [TestCase(null, null)]
            public void ReturnsCorrectDirectoryPathForString(string path, string expected)
            {
                // Given
                TestEngine engine = new TestEngine();
                MetadataItems initialMetadata = new MetadataItems();
                Metadata metadata = new Metadata(engine, initialMetadata);

                // When
                metadata = new Metadata(engine, metadata, new[] { new KeyValuePair<string, object>("A", path) });
                object result = metadata.DirectoryPath("A");

                // Then
                if (expected == null)
                {
                    Assert.IsNull(result);
                }
                else
                {
                    Assert.IsInstanceOf<DirectoryPath>(result);
                    Assert.AreEqual(expected, ((DirectoryPath)result).FullPath);
                }
            }
        }

        public class EnumeratorTests : MetadataFixture
        {
            [Test]
            public void EnumeratingMetadataValuesReturnsCorrectResults()
            {
                // Given
                TestEngine engine = new TestEngine();
                MetadataItems initialMetadata = new MetadataItems
                {
                    { "A", new SimpleMetadataValue { Value = "a" } },
                    { "B", new SimpleMetadataValue { Value = "b" } },
                    { "C", new SimpleMetadataValue { Value = "c" } }
                };
                Metadata metadata = new Metadata(engine, initialMetadata);

                // When
                object[] values = metadata.Select(x => x.Value).ToArray();

                // Then
                CollectionAssert.AreEquivalent(new[] { "a", "b", "c" }, values);
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
