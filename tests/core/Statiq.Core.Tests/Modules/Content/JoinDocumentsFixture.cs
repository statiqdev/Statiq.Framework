using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using System.Linq;
using NUnit.Framework;
using Shouldly;
using Statiq.Common;
using Statiq.Testing;

namespace Statiq.Core.Tests.Modules.Contents
{
    [TestFixture]
    public class JoinDocumentsFixture : BaseFixture
    {
        [Test]
        public async Task JoinTwoDocumentsJoinWithNoDelimiter()
        {
            // Given
            TestDocument first = new TestDocument("Test");
            TestDocument second = new TestDocument("Test2");
            JoinDocuments join = new JoinDocuments();

            // When
            ImmutableArray<TestDocument> results = await ExecuteAsync(new[] { first, second }, join);

            // Then
            results.Single().Content.ShouldBe("TestTest2");
        }

        [Test]
        public async Task JoinThreeDocumentsJoinWithNoDelimiter()
        {
            // Given
            TestDocument first = new TestDocument("Test");
            TestDocument second = new TestDocument("Test2");
            TestDocument third = new TestDocument("Test3");
            JoinDocuments join = new JoinDocuments();

            // When
            ImmutableArray<TestDocument> results = await ExecuteAsync(new[] { first, second, third }, join);

            // Then
            results.Single().Content.ShouldBe("TestTest2Test3");
        }

        [Test]
        public async Task JoinThreeDocumentsJoinWithNoDelimiter_firstnull()
        {
            // Given
            TestDocument first = null;
            TestDocument second = new TestDocument("Test2");
            TestDocument third = new TestDocument("Test3");
            JoinDocuments join = new JoinDocuments();

            // When
            ImmutableArray<TestDocument> results = await ExecuteAsync(new[] { first, second, third }, join);

            // Then
            results.Single().Content.ShouldBe("Test2Test3");
        }

        [Test]
        public async Task JoinThreeDocumentsJoinWithNoDelimiter_secondnull()
        {
            // Given
            TestDocument first = new TestDocument("Test");
            TestDocument second = null;
            TestDocument third = new TestDocument("Test3");
            JoinDocuments join = new JoinDocuments();

            // When
            ImmutableArray<TestDocument> results = await ExecuteAsync(new[] { first, second, third }, join);

            // Then
            results.Single().Content.ShouldBe("TestTest3");
        }

        [Test]
        public async Task JoinnullPassedInAsDocumentList()
        {
            // Given
            JoinDocuments join = new JoinDocuments();

            // When
            ImmutableArray<TestDocument> results = await ExecuteAsync(join);

            // Then
            results.Single().Content.ShouldBeEmpty();
        }

        [Test]
        public async Task JoinTwoDocumentsJoinWithCommaDelimiter()
        {
            // Given
            TestDocument first = new TestDocument("Test");
            TestDocument second = new TestDocument("Test2");
            JoinDocuments join = new JoinDocuments(",");

            // When
            ImmutableArray<TestDocument> results = await ExecuteAsync(new[] { first, second }, join);

            // Then
            results.Single().Content.ShouldBe("Test,Test2");
        }

        [Test]
        public async Task JoinTwoDocumentsJoinWithDelimiterInText()
        {
            // Given
            TestDocument first = new TestDocument("Test");
            TestDocument second = new TestDocument("Test2");
            JoinDocuments join = new JoinDocuments("Test");

            // When
            ImmutableArray<TestDocument> results = await ExecuteAsync(new[] { first, second }, join);

            // Then
            results.Single().Content.ShouldBe("TestTestTest2");
        }

        [Test]
        public async Task JoinTwoDocumentsWithKeepFirstMetaDataReturnKeepsFirstMetaData()
        {
            // Given
            TestDocument first = new TestDocument("Test")
            {
                { "one", "two" }
            };
            TestDocument second = new TestDocument("Test2")
            {
                { "three", "four" }
            };
            JoinDocuments join = new JoinDocuments(JoinedMetadata.FirstDocument);

            // When
            ImmutableArray<TestDocument> results = await ExecuteAsync(new[] { first, second }, join);

            // Then
            TestDocument result = results.Single();
            result.Keys.ShouldContain("one");
            result.Keys.ShouldNotContain("three");
        }

        [Test]
        public async Task JoinTwoDocumentsWithMetaDataReturnDefaultMetaData()
        {
            // Given
            TestDocument first = new TestDocument("Test")
            {
                { "one", "two" }
            };
            TestDocument second = new TestDocument("Test2")
            {
                { "three", "four" }
            };
            JoinDocuments join = new JoinDocuments();

            // When
            ImmutableArray<TestDocument> results = await ExecuteAsync(new[] { first, second }, join);

            // Then
            TestDocument result = results.Single();
            result.Keys.ShouldNotContain("one");
            result.Keys.ShouldNotContain("three");
        }

        [Test]
        public async Task JoinTwoDocumentsWithKeepLastMetaDataReturnKeepsLastMetaData()
        {
            // Given
            TestDocument first = new TestDocument("Test")
            {
                { "one", "two" }
            };
            TestDocument second = new TestDocument("Test2")
            {
                { "three", "four" }
            };
            JoinDocuments join = new JoinDocuments(JoinedMetadata.LastDocument);

            // When
            ImmutableArray<TestDocument> results = await ExecuteAsync(new[] { first, second }, join);

            // Then
            TestDocument result = results.Single();
            result.Keys.ShouldNotContain("one");
            result.Keys.ShouldContain("three");
        }

        [Test]
        public async Task JoinTwoDocumentsWithAllKeepFirstMetaData()
        {
            // Given
            TestDocument first = new TestDocument("Test")
            {
                { "one", "two" }
            };
            TestDocument second = new TestDocument("Test2")
            {
                { "one", "seven" },
                { "three", "four" }
            };
            JoinDocuments join = new JoinDocuments(JoinedMetadata.AllWithFirstDuplicates);

            // When
            ImmutableArray<TestDocument> results = await ExecuteAsync(new[] { first, second }, join);

            // Then
            TestDocument result = results.Single();
            result.Values.ShouldContain("two");
            result.Values.ShouldNotContain("seven");
            result.Keys.ShouldContain("three");
        }

        [Test]
        public async Task JoinTwoDocumentsWithAllKeepLastMetaData()
        {
            // Given
            TestDocument first = new TestDocument("Test")
            {
                { "one", "two" }
            };
            TestDocument second = new TestDocument("Test2")
            {
                { "one", "seven" },
                { "three", "four" }
            };
            JoinDocuments join = new JoinDocuments(JoinedMetadata.AllWithLastDuplicates);

            // When
            ImmutableArray<TestDocument> results = await ExecuteAsync(new[] { first, second }, join);

            // Then
            TestDocument result = results.Single();
            result.Values.ShouldNotContain("two");
            result.Values.ShouldContain("seven");
            result.Keys.ShouldContain("three");
        }

        [Test]
        public async Task EmptyListDoesNotError()
        {
            // Given
            JoinDocuments join = new JoinDocuments();

            // When
            ImmutableArray<TestDocument> results = await ExecuteAsync(join);

            // Then
            results.Single().Content.ShouldBeEmpty();
        }

        [Test]
        public async Task EmptyListWithDelimitorDoesNotError()
        {
            // Given
            JoinDocuments join = new JoinDocuments(",");

            // When
            ImmutableArray<TestDocument> results = await ExecuteAsync(join);

            // Then
            results.Single().Content.ShouldBeEmpty();
        }
    }
}
