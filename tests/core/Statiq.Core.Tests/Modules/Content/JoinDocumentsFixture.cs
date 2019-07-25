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
            TestDocument result = await ExecuteAsync(new[] { first, second }, join).FromDerived<IEnumerable<TestDocument>, ImmutableArray<TestDocument>>().SingleAsync();

            // Then
            result.Content.ShouldBe("TestTest2");
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
            TestDocument result = await ExecuteAsync(new[] { first, second, third }, join).ThenAsync(x => x.Single());

            // Then
            result.Content.ShouldBe("TestTest2Test3");
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
            TestDocument result = await ExecuteAsync(new[] { first, second, third }, join).SingleAsync();

            // Then
            result.Content.ShouldBe("Test2Test3");
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
            TestDocument result = await ExecuteAsync(new[] { first, second, third }, join).SingleAsync();

            // Then
            result.Content.ShouldBe("TestTest3");
        }

        [Test]
        public async Task JoinnullPassedInAsDocumentList()
        {
            // Given
            JoinDocuments join = new JoinDocuments();

            // When
            TestDocument result = await ExecuteAsync(join).SingleAsync();

            // Then
            result.Content.ShouldBeEmpty();
        }

        [Test]
        public async Task JoinTwoDocumentsJoinWithCommaDelimiter()
        {
            // Given
            TestDocument first = new TestDocument("Test");
            TestDocument second = new TestDocument("Test2");
            JoinDocuments join = new JoinDocuments(",");

            // When
            TestDocument result = await ExecuteAsync(new[] { first, second }, join).SingleAsync();

            // Then
            result.Content.ShouldBe("Test,Test2");
        }

        [Test]
        public async Task JoinTwoDocumentsJoinWithDelimiterInText()
        {
            // Given
            TestDocument first = new TestDocument("Test");
            TestDocument second = new TestDocument("Test2");
            JoinDocuments join = new JoinDocuments("Test");

            // When
            TestDocument result = await ExecuteAsync(new[] { first, second }, join).SingleAsync();

            // Then
            result.Content.ShouldBe("TestTestTest2");
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
            TestDocument result = await ExecuteAsync(new[] { first, second }, join).SingleAsync();

            // Then
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
            TestDocument result = await ExecuteAsync(new[] { first, second }, join).SingleAsync();

            // Then
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
            TestDocument result = await ExecuteAsync(new[] { first, second }, join).SingleAsync();

            // Then
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
            TestDocument result = await ExecuteAsync(new[] { first, second }, join).SingleAsync();

            // Then
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
            TestDocument result = await ExecuteAsync(new[] { first, second }, join).SingleAsync();

            // Then
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
            TestDocument result = await ExecuteAsync(join).SingleAsync();

            // Then
            result.Content.ShouldBeEmpty();
        }

        [Test]
        public async Task EmptyListWithDelimitorDoesNotError()
        {
            // Given
            JoinDocuments join = new JoinDocuments(",");

            // When
            TestDocument result = await ExecuteAsync(join).SingleAsync();

            // Then
            result.Content.ShouldBeEmpty();
        }
    }
}
