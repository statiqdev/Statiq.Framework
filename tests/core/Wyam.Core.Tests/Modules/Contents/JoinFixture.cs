using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Shouldly;
using Wyam.Common;
using Wyam.Common.Documents;
using Wyam.Common.Execution;
using Wyam.Core.Modules.Contents;
using Wyam.Testing;
using Wyam.Testing.Documents;
using Wyam.Testing.Execution;

namespace Wyam.Core.Tests.Modules.Contents
{
    [TestFixture]
    [NonParallelizable]
    public class JoinFixture : BaseFixture
    {
        [Test]
        public async Task JoinTwoDocumentsJoinWithNoDelimiter()
        {
            // Given
            TestDocument first = new TestDocument("Test");
            TestDocument second = new TestDocument("Test2");
            Join join = new Join();

            // When
            TestDocument result = await ExecuteAsync(new[] { first, second }, join).SingleAsync();

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
            Join join = new Join();

            // When
            TestDocument result = await ExecuteAsync(new[] { first, second, third }, join).SingleAsync();

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
            Join join = new Join();

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
            Join join = new Join();

            // When
            TestDocument result = await ExecuteAsync(new[] { first, second, third }, join).SingleAsync();

            // Then
            result.Content.ShouldBe("TestTest3");
        }

        [Test]
        public async Task JoinnullPassedInAsDocumentList()
        {
            // Given
            Join join = new Join();

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
            Join join = new Join(",");

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
            Join join = new Join("Test");

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
            Join join = new Join(JoinedMetadata.FirstDocument);

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
            Join join = new Join();

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
            Join join = new Join(JoinedMetadata.LastDocument);

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
            Join join = new Join(JoinedMetadata.AllWithFirstDuplicates);

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
            Join join = new Join(JoinedMetadata.AllWithLastDuplicates);

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
            Join join = new Join();

            // When
            TestDocument result = await ExecuteAsync(join).SingleAsync();

            // Then
            result.Content.ShouldBeEmpty();
        }

        [Test]
        public async Task EmptyListWithDelimitorDoesNotError()
        {
            // Given
            Join join = new Join(",");

            // When
            TestDocument result = await ExecuteAsync(join).SingleAsync();

            // Then
            result.Content.ShouldBeEmpty();
        }
    }
}
