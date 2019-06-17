using System;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Shouldly;
using Statiq.Common.Documents;
using Statiq.Common.Execution;
using Statiq.Common.Meta;
using Statiq.Core.Modules.Metadata;
using Statiq.Testing;
using Statiq.Testing.Documents;
using Statiq.Testing.Execution;

namespace Statiq.Core.Tests.Modules.Metadata
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
                TestDocument document = new TestDocument(
                    new MetadataItems
                    {
                        { "Title", "Foo" }
                    });
                ValidateMeta<string> validateMeta = new ValidateMeta<string>("Title");

                // When, Then
                // Convert this to Should.NotThrowAsync when https://github.com/shouldly/shouldly/pull/430 is merged
                Should.NotThrow(() => ExecuteAsync(document, validateMeta).Result);
            }

            [Test]
            public async Task AbsenceOfKeyThrows()
            {
                // Given
                TestDocument document = new TestDocument();
                ValidateMeta<string> validateMeta = new ValidateMeta<string>("Title");

                // When, Then
                await Should.ThrowAsync<AggregateException>(async () => await ExecuteAsync(document, validateMeta));
            }

            [Test]
            public async Task FailedAssertionThrows()
            {
                // Given
                TestDocument document = new TestDocument(
                    new MetadataItems
                    {
                        { "Title", "Foo" }
                    });
                ValidateMeta<string> validateMeta = new ValidateMeta<string>("Title").WithAssertion(x => x == "Baz");

                // When, Then
                await Should.ThrowAsync<AggregateException>(async () => await ExecuteAsync(document, validateMeta));
            }

            [Test]
            public void PassedAssertionDoesNotThrow()
            {
                // Given
                TestDocument document = new TestDocument(
                    new MetadataItems
                    {
                        { "Title", "Foo" }
                    });
                ValidateMeta<string> validateMeta = new ValidateMeta<string>("Title").WithAssertion(x => x == "Foo");

                // When, Then
                // Convert this to Should.NotThrowAsync when https://github.com/shouldly/shouldly/pull/430 is merged
                Should.NotThrow(() => ExecuteAsync(document, validateMeta).Result);
            }
        }
    }
}
