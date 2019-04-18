using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Wyam.Common.Documents;
using Wyam.Common.Execution;
using Wyam.Common.IO;
using Wyam.Common.Meta;
using Wyam.Common.Util;
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
            IDocument first = new TestDocument("Test");
            IDocument second = new TestDocument("Test2");

            IExecutionContext context = new TestExecutionContext();
            Join join = new Join();

            // When
            List<IDocument> results = await join.ExecuteAsync(new[] { first, second }, context).ToListAsync();

            // Then
            Assert.AreEqual("TestTest2", results.Single().Content);
        }

        [Test]
        public async Task JoinThreeDocumentsJoinWithNoDelimiter()
        {
            // Given
            IDocument first = new TestDocument("Test");
            IDocument second = new TestDocument("Test2");
            IDocument third = new TestDocument("Test3");

            IExecutionContext context = new TestExecutionContext();
            Join join = new Join();

            // When
            List<IDocument> results = await join.ExecuteAsync(new[] { first, second, third }, context).ToListAsync();

            // Then
            Assert.AreEqual("TestTest2Test3", results.Single().Content);
        }

        [Test]
        public async Task JoinThreeDocumentsJoinWithNoDelimiter_firstnull()
        {
            // Given
            IDocument first = null;
            IDocument second = new TestDocument("Test2");
            IDocument third = new TestDocument("Test3");

            IExecutionContext context = new TestExecutionContext();
            Join join = new Join();

            // When
            List<IDocument> results = await join.ExecuteAsync(new[] { first, second, third }, context).ToListAsync();

            // Then
            Assert.AreEqual("Test2Test3", results.Single().Content);
        }

        [Test]
        public async Task JoinThreeDocumentsJoinWithNoDelimiter_secondnull()
        {
            // Given
            IDocument first = new TestDocument("Test");
            IDocument second = null;
            IDocument third = new TestDocument("Test3");

            IExecutionContext context = new TestExecutionContext();
            Join join = new Join();

            // When
            List<IDocument> results = await join.ExecuteAsync(new[] { first, second, third }, context).ToListAsync();

            // Then
            Assert.AreEqual("TestTest3", results.Single().Content);
        }

        [Test]
        public async Task JoinnullPassedInAsDocumentList()
        {
            // Given
            IExecutionContext context = new TestExecutionContext();
            Join join = new Join();

            // When
            List<IDocument> results = await join.ExecuteAsync(null, context).ToListAsync();

            // Then
            Assert.AreEqual(null, results.Single().Content);
        }

        [Test]
        public async Task JoinTwoDocumentsJoinWithCommaDelimiter()
        {
            // Given
            IDocument first = new TestDocument("Test");
            IDocument second = new TestDocument("Test2");

            IExecutionContext context = new TestExecutionContext();
            Join join = new Join(",");

            // When
            List<IDocument> results = await join.ExecuteAsync(new[] { first, second }, context).ToListAsync();

            // Then
            Assert.AreEqual("Test,Test2", results.Single().Content);
        }

        [Test]
        public async Task JoinTwoDocumentsJoinWithDelimiterInText()
        {
            // Given
            IDocument first = new TestDocument("Test");
            IDocument second = new TestDocument("Test2");

            IExecutionContext context = new TestExecutionContext();
            Join join = new Join("Test");

            // When
            List<IDocument> results = await join.ExecuteAsync(new[] { first, second }, context).ToListAsync();

            // Then
            Assert.AreEqual("TestTestTest2", results.Single().Content);
        }

        [Test]
        public async Task JoinTwoDocumentsWithKeepFirstMetaDataReturnKeepsFirstMetaData()
        {
            // Given
            IDocument first = new TestDocument("Test", new List<KeyValuePair<string, object>>() { new KeyValuePair<string, object>("one", "two") });
            IDocument second = new TestDocument("Test2", new List<KeyValuePair<string, object>>() { new KeyValuePair<string, object>("three", "four") });

            IExecutionContext context = new TestExecutionContext();
            Join join = new Join(JoinedMetadata.FirstDocument);

            // When
            List<IDocument> results = await join.ExecuteAsync(new[] { first, second }, context).ToListAsync();

            // Then
            Assert.True(results.Single().Keys.Contains("one"));
            Assert.False(results.Single().Keys.Contains("three"));
        }

        [Test]
        public async Task JoinTwoDocumentsWithMetaDataReturnDefaultMetaData()
        {
            // Given
            IDocument first = new TestDocument("Test", new List<KeyValuePair<string, object>>() { new KeyValuePair<string, object>("one", "two") });
            IDocument second = new TestDocument("Test2", new List<KeyValuePair<string, object>>() { new KeyValuePair<string, object>("three", "four") });

            IExecutionContext context = new TestExecutionContext();
            Join join = new Join();

            // When
            List<IDocument> results = await join.ExecuteAsync(new[] { first, second }, context).ToListAsync();

            // Then
            Assert.False(results.Single().Keys.Contains("one"));
            Assert.False(results.Single().Keys.Contains("three"));
        }

        [Test]
        public async Task JoinTwoDocumentsWithKeepLastMetaDataReturnKeepsLastMetaData()
        {
            // Given
            IDocument first = new TestDocument("Test", new List<KeyValuePair<string, object>>() { new KeyValuePair<string, object>("one", "two") });
            IDocument second = new TestDocument("Test2", new List<KeyValuePair<string, object>>() { new KeyValuePair<string, object>("three", "four") });

            IExecutionContext context = new TestExecutionContext();
            Join join = new Join(JoinedMetadata.LastDocument);

            // When
            List<IDocument> results = await join.ExecuteAsync(new[] { first, second }, context).ToListAsync();

            // Then
            Assert.True(results.Single().Keys.Contains("three"));
            Assert.False(results.Single().Keys.Contains("one"));
        }

        [Test]
        public async Task JoinTwoDocumentsWithAllKeepFirstMetaData()
        {
            // Given
            IDocument first = new TestDocument("Test", new List<KeyValuePair<string, object>>() { new KeyValuePair<string, object>("one", "two") });
            IDocument second = new TestDocument("Test2", new List<KeyValuePair<string, object>>() { new KeyValuePair<string, object>("one", "seven"),  new KeyValuePair<string, object>("three", "four") });

            IExecutionContext context = new TestExecutionContext();
            Join join = new Join(JoinedMetadata.AllWithFirstDuplicates);

            // When
            List<IDocument> results = await join.ExecuteAsync(new[] { first, second }, context).ToListAsync();

            // Then
            Assert.True(results.Single().Values.Contains("two"));
            Assert.False(results.Single().Values.Contains("seven"));

            Assert.True(results.Single().Keys.Contains("three"));
        }

        [Test]
        public async Task JoinTwoDocumentsWithAllKeepLastMetaData()
        {
            // Given
            IDocument first = new TestDocument("Test", new List<KeyValuePair<string, object>>() { new KeyValuePair<string, object>("one", "two") });
            IDocument second = new TestDocument("Test2", new List<KeyValuePair<string, object>>() { new KeyValuePair<string, object>("one", "seven"), new KeyValuePair<string, object>("three", "four") });

            IExecutionContext context = new TestExecutionContext();
            Join join = new Join(JoinedMetadata.AllWithLastDuplicates);

            // When
            List<IDocument> results = await join.ExecuteAsync(new[] { first, second }, context).ToListAsync();

            // Then
            Assert.False(results.Single().Values.Contains("two"));
            Assert.True(results.Single().Values.Contains("seven"));

            Assert.True(results.Single().Keys.Contains("three"));
        }

        [Test]
        public async Task EmptyListDoesNotError()
        {
            // Given
            IExecutionContext context = new TestExecutionContext();
            Join join = new Join();

            // When
            List<IDocument> results = await join.ExecuteAsync(new IDocument[0], context).ToListAsync();

            // Then
            Assert.AreEqual(null, results.Single().Content);
        }

        [Test]
        public async Task EmptyListWithDelimitorDoesNotError()
        {
            // Given
            IExecutionContext context = new TestExecutionContext();
            Join join = new Join(",");

            // When
            List<IDocument> results = await join.ExecuteAsync(new IDocument[0], context).ToListAsync();

            // Then
            Assert.AreEqual(null, results.Single().Content);
        }
    }
}
