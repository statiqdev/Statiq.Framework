using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Shouldly;
using Statiq.Common.Documents;
using Statiq.Common.Execution;
using Statiq.Common.IO;
using Statiq.Testing;
using Statiq.Testing.Documents;
using Statiq.Testing.Execution;

namespace Statiq.Xmp.Tests
{
    [TestFixture]
    public class XmpFixture : BaseFixture
    {
        public class ExecuteTests : XmpFixture
        {
            [Test]
            public async Task ReadMetadata()
            {
                // Given
                System.Globalization.CultureInfo.CurrentCulture = System.Globalization.CultureInfo.GetCultureInfo("en");
                IReadOnlyList<TestDocument> documents = GetDocumentsFromSources(Path.Combine(TestContext.CurrentContext.TestDirectory, "Samples", "Flamme.png"));
                Xmp directoryMetadata = new Xmp()
                    .WithMetadata("xmpRights:UsageTerms", "Copyright");

                // When
                IReadOnlyList<TestDocument> results = await ExecuteAsync(documents, directoryMetadata);

                // Then
                results.Single()["Copyright"]
                    .ShouldBe("This work is licensed under a <a rel=\"license\" href=\"http://creativecommons.org/licenses/by-sa/4.0/\">Creative Commons Attribution-ShareAlike 4.0 International License</a>.");
            }

            [Test]
            public async Task SkipMissingMandatory()
            {
                // Given
                System.Globalization.CultureInfo.CurrentCulture = System.Globalization.CultureInfo.GetCultureInfo("en");
                ThrowOnTraceEventType(TraceEventType.Error);
                IReadOnlyList<TestDocument> documents = GetDocumentsFromSources(
                    Path.Combine(TestContext.CurrentContext.TestDirectory, "Samples", "Flamme.png"),
                    Path.Combine(TestContext.CurrentContext.TestDirectory, "Samples", "RomantiqueInitials.ttf"));
                Xmp directoryMetadata = new Xmp(skipElementOnMissingMandatoryData: true)
                    .WithMetadata("xmpRights:UsageTerms", "Copyright", true);

                // When
                IReadOnlyList<TestDocument> results = await ExecuteAsync(documents, directoryMetadata);

                // Then
                results.Count.ShouldBe(1);
            }

            [Test]
            public async Task DontSkipMissingMandatory()
            {
                // Given
                System.Globalization.CultureInfo.CurrentCulture = System.Globalization.CultureInfo.GetCultureInfo("en");
                ThrowOnTraceEventType(TraceEventType.Error);
                IReadOnlyList<TestDocument> documents = GetDocumentsFromSources(
                    Path.Combine(TestContext.CurrentContext.TestDirectory, "Samples", "Flamme.png"),
                    Path.Combine(TestContext.CurrentContext.TestDirectory, "Samples", "RomantiqueInitials.ttf"));
                Xmp directoryMetadata = new Xmp(skipElementOnMissingMandatoryData: false)
                    .WithMetadata("xmpRights:UsageTerms", "Copyright", true);

                // When
                IReadOnlyList<TestDocument> results = await ExecuteAsync(documents, directoryMetadata);

                // Then
                results.Count.ShouldBe(2);
            }

            [Test]
            public async Task UsingNonDefaultNamespace()
            {
                // Given
                System.Globalization.CultureInfo.CurrentCulture = System.Globalization.CultureInfo.GetCultureInfo("en");
                IReadOnlyList<TestDocument> documents = GetDocumentsFromSources(Path.Combine(TestContext.CurrentContext.TestDirectory, "Samples", "Flamme.png"));
                Xmp directoryMetadata = new Xmp()
                    .WithNamespace("http://ns.adobe.com/xap/1.0/rights/", "bla")
                    .WithMetadata("bla:UsageTerms", "Copyright");

                // When
                IReadOnlyList<TestDocument> results = await ExecuteAsync(documents, directoryMetadata);

                // Then
                results.Single()["Copyright"]
                    .ShouldBe("This work is licensed under a <a rel=\"license\" href=\"http://creativecommons.org/licenses/by-sa/4.0/\">Creative Commons Attribution-ShareAlike 4.0 International License</a>.");
            }

            private IReadOnlyList<TestDocument> GetDocumentsFromSources(params string[] pathArray) =>
                pathArray.Select(x => new TestDocument(x, (FilePath)null, File.OpenRead(x))).ToList();
        }
    }
}