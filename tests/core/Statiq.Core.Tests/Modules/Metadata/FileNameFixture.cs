using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Shouldly;
using Statiq.Common;
using Statiq.Common.Documents;
using Statiq.Common.IO;
using Statiq.Common.Meta;
using Statiq.Core.Modules.Metadata;
using Statiq.Testing;
using Statiq.Testing.Documents;
using Statiq.Testing.Execution;

namespace Statiq.Core.Tests.Modules.Metadata
{
    [TestFixture]
    [NonParallelizable]
    public class FileNameFixture : BaseFixture
    {
        public class ExecuteTests : FileNameFixture
        {
            [TestCase(
                "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-._~:?#[]@!$&'()*+,;=",
                "abcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyz0123456789")]
            [TestCase("Děku.jemeविकीвики-движка", "děku.jemeविकीвикидвижка")]
            [TestCase(
                "this is my title - and some \t\t\t\t\n   clever; (piece) of text here: [ok].",
                "this-is-my-title-and-some-clever-piece-of-text-here-ok")]
            [TestCase(
                "this is my title?!! science and #firstworldproblems :* :sadface=true",
                "this-is-my-title-science-and-firstworldproblems-sadfacetrue")]
            [TestCase(
                "one-two-three--four--five and a six--seven--eight-nine------ten",
                "onetwothreefourfive-and-a-sixseveneightnineten")]
            public async Task FileNameIsConvertedCorrectly(string input, string output)
            {
                // Given
                TestDocument document = new TestDocument(new FilePath(input));
                FileName fileName = new FileName();

                // When
                TestDocument result = await ExecuteAsync(document, fileName).SingleAsync();

                // Then
                result.Destination.FullPath.ShouldBe(output);
            }

            [Test]
            public async Task FileNameShouldBeLowercase()
            {
                // Given
                const string input = "FileName With MiXeD CapS";
                const string output = "filename-with-mixed-caps";
                TestDocument document = new TestDocument(new FilePath(input));
                FileName fileName = new FileName();

                // When
                TestDocument result = await ExecuteAsync(document, fileName).SingleAsync();

                // Then
                result.Destination.FullPath.ShouldBe(output);
            }

            [Test]
            public async Task WithAllowedCharactersDoesNotReplaceProvidedCharacters()
            {
                // Given
                const string input = "this-is-a-.net-tag";
                const string output = "this-is-a-.net-tag";
                TestDocument document = new TestDocument(new FilePath(input));
                FileName fileName = new FileName();

                // When
                fileName = fileName.WithAllowedCharacters(new string[] { "-" });
                TestDocument result = await ExecuteAsync(document, fileName).SingleAsync();

                // Then
                result.Destination.FullPath.ShouldBe(output);
            }

            [Test]
            public async Task WithAllowedCharactersDoesNotReplaceDotAtEnd()
            {
                // Given
                const string input = "this-is-a-.";
                const string output = "thisisa.";
                TestDocument document = new TestDocument(new FilePath(input));
                FileName fileName = new FileName();

                // When
                fileName = fileName.WithAllowedCharacters(new string[] { "." });
                TestDocument result = await ExecuteAsync(document, fileName).SingleAsync();

                // Then
                result.Destination.FullPath.ShouldBe(output);
            }

            public static string[] ReservedChars => FileName.ReservedChars;

            [Test]
            [TestCaseSource(nameof(ReservedChars))]
            public async Task FileNameIsConvertedCorrectlyWithReservedChar(string character)
            {
                // Given
                string manyCharactersWow = new string(character[0], 10);
                string path = string.Format("testing {0} some of {0} these {0}", manyCharactersWow);
                TestDocument document = new TestDocument()
                {
                    new MetadataItem("MyKey", path)
                };
                FileName fileName = new FileName("MyKey");

                // When
                TestDocument result = await ExecuteAsync(document, fileName).SingleAsync();

                // Then
                result.String("MyKey").ShouldBe("testing-some-of-these-");
            }

            [TestCase(null)]
            [TestCase("")]
            [TestCase(" ")]
            public async Task IgnoresNullOrWhiteSpaceStrings(string input)
            {
                // Given
                TestDocument document = new TestDocument
                {
                    new MetadataItem("MyKey", input)
                };
                FileName fileName = new FileName("MyKey");

                // When
                TestDocument result = await ExecuteAsync(document, fileName).SingleAsync();

                // Then
                result.String("MyKey").ShouldBe(input);
            }

            [Test]
            public async Task PreservesExtension()
            {
                // Given
                const string input = "myfile.html";
                const string output = "myfile.html";

                TestDocument document = new TestDocument()
                {
                    new MetadataItem("MyKey", input)
                };
                FileName fileName = new FileName("MyKey");

                // When
                TestDocument result = await ExecuteAsync(document, fileName).SingleAsync();

                // Then
                result.FilePath("MyKey").FullPath.ShouldBe(output);
            }

            [Test]
            public async Task TrimWhitespace()
            {
                // Given
                const string input = "   myfile.html   ";
                const string output = "myfile.html";
                TestDocument document = new TestDocument
                {
                    new MetadataItem("MyKey", input)
                };
                FileName fileName = new FileName("MyKey");

                // When
                TestDocument result = await ExecuteAsync(document, fileName).SingleAsync();

                // Then
                result.FilePath("MyKey").FullPath.ShouldBe(output);
            }
        }
    }
}
