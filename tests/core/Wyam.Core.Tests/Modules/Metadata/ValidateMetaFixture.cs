using System;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Shouldly;
using Wyam.Common.Documents;
using Wyam.Common.Execution;
using Wyam.Common.Meta;
using Wyam.Common.Util;
using Wyam.Core.Modules.Metadata;
using Wyam.Testing;
using Wyam.Testing.Documents;
using Wyam.Testing.Execution;

namespace Wyam.Core.Tests.Modules.Metadata
{
    [TestFixture]
    [NonParallelizable]
    public class ValidateMetaFixture : BaseFixture
    {
        public class ExecuteTests : ValidateMetaFixture
        {
            [Test]
            public void ExistenceOfKeyDoesNotThrow()
            {
                // Given
                IDocument document = new TestDocument(
                    new MetadataItems
                    {
                        { "Title", "Foo" }
                    });
                IExecutionContext context = new TestExecutionContext();
                ValidateMeta<string> validateMeta = new ValidateMeta<string>("Title");

                // When, Then
                // Convert this to Should.NotThrowAsync when https://github.com/shouldly/shouldly/pull/430 is merged
                Should.NotThrow(() => validateMeta.ExecuteAsync(new[] { document }, context).Result.ToList());  // Make sure to materialize the result list
            }

            [Test]
            public async Task AbsenceOfKeyThrows()
            {
                // Given
                IDocument document = new TestDocument();
                IExecutionContext context = new TestExecutionContext();
                ValidateMeta<string> validateMeta = new ValidateMeta<string>("Title");

                // When, Then
                await Should.ThrowAsync<AggregateException>(async () => await validateMeta.ExecuteAsync(new[] { document }, context).ToListAsync());  // Make sure to materialize the result list
            }

            [Test]
            public async Task FailedAssertionThrows()
            {
                // Given
                IDocument document = new TestDocument(
                    new MetadataItems
                    {
                        { "Title", "Foo" }
                    });
                IExecutionContext context = new TestExecutionContext();
                ValidateMeta<string> validateMeta = new ValidateMeta<string>("Title").WithAssertion(x => x == "Baz");

                // When, Then
                await Should.ThrowAsync<AggregateException>(async () => await validateMeta.ExecuteAsync(new[] { document }, context).ToListAsync());  // Make sure to materialize the result list
            }

            [Test]
            public void PassedAssertionDoesNotThrow()
            {
                // Given
                IDocument document = new TestDocument(
                    new MetadataItems
                    {
                        { "Title", "Foo" }
                    });
                IExecutionContext context = new TestExecutionContext();
                ValidateMeta<string> validateMeta = new ValidateMeta<string>("Title").WithAssertion(x => x == "Foo");

                // When, Then
                // Convert this to Should.NotThrowAsync when https://github.com/shouldly/shouldly/pull/430 is merged
                Should.NotThrow(() => validateMeta.ExecuteAsync(new[] { document }, context).Result.ToList());  // Make sure to materialize the result list
            }
        }
    }
}
