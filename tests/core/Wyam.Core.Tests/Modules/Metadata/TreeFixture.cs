using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Shouldly;
using Wyam.Common;
using Wyam.Common.Configuration;
using Wyam.Common.Documents;
using Wyam.Common.Execution;
using Wyam.Common.IO;
using Wyam.Common.Meta;
using Wyam.Core.Modules.Metadata;
using Wyam.Testing;
using Wyam.Testing.Documents;
using Wyam.Testing.Execution;

namespace Wyam.Core.Tests.Modules.Metadata
{
    [TestFixture]
    [NonParallelizable]
    public class TreeFixture : BaseFixture
    {
        public class ExecuteTests : TreeFixture
        {
            [Test]
            public async Task GetsTreeWithCommonRoot()
            {
                // Given
                TestDocument[] inputs = GetDocumentsFromRelativePaths(
                    "root/a/2.txt",
                    "root/b/3.txt",
                    "root/a/1.txt",
                    "root/b/x/4.txt",
                    "root/c/d/5.txt",
                    "root/6.txt");
                Tree tree = new Tree().WithNesting();

                // When
                TestDocument result = await ExecuteAsync(inputs, tree).SingleAsync();

                // Then
                VerifyTree(
                    result,
                    "index.html",
                    "root/index.html",
                    "root/6.txt",
                    "root/a/index.html",
                    "root/a/1.txt",
                    "root/a/2.txt",
                    "root/b/index.html",
                    "root/b/3.txt",
                    "root/b/x/index.html",
                    "root/b/x/4.txt",
                    "root/c/index.html",
                    "root/c/d/index.html",
                    "root/c/d/5.txt");
            }

            [Test]
            public async Task GetsTree()
            {
                // Given
                TestDocument[] inputs = GetDocumentsFromRelativePaths(
                    "a/2.txt",
                    "b/3.txt",
                    "a/1.txt",
                    "b/x/4.txt",
                    "c/d/5.txt",
                    "6.txt");
                Tree tree = new Tree().WithNesting();

                // When
                TestDocument result = await ExecuteAsync(inputs, tree).SingleAsync();

                // Then
                VerifyTree(
                    result,
                    "index.html",
                    "6.txt",
                    "a/index.html",
                    "a/1.txt",
                    "a/2.txt",
                    "b/index.html",
                    "b/3.txt",
                    "b/x/index.html",
                    "b/x/4.txt",
                    "c/index.html",
                    "c/d/index.html",
                    "c/d/5.txt");
            }

            [Test]
            public async Task GetsPlaceholderWithSource()
            {
                // Given
                TestDocument[] inputs = GetDocumentsFromRelativePaths(
                    "a/2.txt",
                    "a/1.txt");
                Tree tree = new Tree().WithNesting();

                // When
                TestDocument result = await ExecuteAsync(inputs, tree).SingleAsync();

                // Then
                VerifyTree(
                    result,
                    "index.html",
                    "a/index.html",
                    "a/1.txt",
                    "a/2.txt");
                result.Source.ShouldBe("/input/index.html");
                result.Document(Keys.Next).Source.ShouldBe("/input/a/index.html");
            }

            [Test]
            public async Task CollapseRoot()
            {
                // Given
                TestDocument[] inputs = GetDocumentsFromRelativePaths(
                    "a/2.txt",
                    "b/3.txt",
                    "a/1.txt",
                    "b/x/4.txt",
                    "c/d/5.txt",
                    "6.txt");
                Tree tree = new Tree().WithNesting(true, true);

                // When
                IReadOnlyList<TestDocument> results = await ExecuteAsync(inputs, tree);

                // Then
                results.Count.ShouldBe(4);
                results.Select(x => x.FilePath(Keys.RelativeFilePath).FullPath)
                    .ShouldBe(new[] { "a/index.html", "b/index.html", "c/index.html", "6.txt" }, true);
            }

            [Test]
            public async Task GetsPreviousSibling()
            {
                // Given
                TestDocument[] inputs = GetDocumentsFromRelativePaths(
                    "root/a/2.txt",
                    "root/a/3.txt",
                    "root/a/1.txt");
                Tree tree = new Tree().WithNesting();

                // When
                IReadOnlyList<TestDocument> results = await ExecuteAsync(inputs, tree);

                // Then
                IDocument document = FindTreeNode(results[0], "root/a/2.txt");
                document.Document(Keys.PreviousSibling).FilePath(Keys.RelativeFilePath).FullPath.ShouldBe("root/a/1.txt");
            }

            [Test]
            public async Task GetsNextSibling()
            {
                // Given
                TestDocument[] inputs = GetDocumentsFromRelativePaths(
                    "root/a/2.txt",
                    "root/a/3.txt",
                    "root/a/1.txt");
                Tree tree = new Tree().WithNesting();

                // When
                IReadOnlyList<TestDocument> results = await ExecuteAsync(inputs, tree);

                // Then
                TestDocument document = FindTreeNode(results[0], "root/a/2.txt");
                document.Document(Keys.NextSibling).FilePath(Keys.RelativeFilePath).FullPath.ShouldBe("root/a/3.txt");
            }

            [Test]
            public async Task GetsPrevious()
            {
                // Given
                TestDocument[] inputs = GetDocumentsFromRelativePaths(
                    "root/a/2.txt",
                    "root/a/3.txt",
                    "root/a/1.txt");
                Tree tree = new Tree().WithNesting();

                // When
                IReadOnlyList<TestDocument> results = await ExecuteAsync(inputs, tree);

                // Then
                TestDocument document = FindTreeNode(results[0], "root/a/2.txt");
                document.Document(Keys.Previous).FilePath(Keys.RelativeFilePath).FullPath.ShouldBe("root/a/1.txt");
            }

            [Test]
            public async Task GetsPreviousUpTree()
            {
                // Given
                TestDocument[] inputs = GetDocumentsFromRelativePaths(
                    "root/a/2.txt",
                    "root/a/3.txt",
                    "root/a/1.txt",
                    "root/b/4.txt");
                Tree tree = new Tree().WithNesting();

                // When
                IReadOnlyList<TestDocument> results = await ExecuteAsync(inputs, tree);

                // Then
                TestDocument document = FindTreeNode(results[0], "root/b/4.txt");
                document.Document(Keys.Previous).FilePath(Keys.RelativeFilePath).FullPath.ShouldBe("root/b/index.html");
            }

            [Test]
            public async Task SplitsTree()
            {
                // Given
                TestDocument[] inputs = GetDocumentsFromRelativePaths(
                    "root/a/2.txt",
                    "root/b/index.html",
                    "root/a/1.txt",
                    "root/b/4.txt");
                Tree tree = new Tree()
                    .WithNesting()
                    .WithRoots(Config.FromDocument(doc => doc.FilePath(Keys.RelativeFilePath).FullPath.EndsWith("b/index.html")));

                // When
                IReadOnlyList<TestDocument> results = await ExecuteAsync(inputs, tree);

                // Then
                results.Count.ShouldBe(2);
                VerifyTree(
                    results[0],
                    "root/b/index.html",
                    "root/b/4.txt");
                VerifyTree(
                    results[1],
                    "index.html",
                    "root/index.html",
                    "root/a/index.html",
                    "root/a/1.txt",
                    "root/a/2.txt");
            }

            [Test]
            public async Task FlatTree()
            {
                // Given
                TestDocument[] inputs = GetDocumentsFromRelativePaths(
                    "root/a/b/2.txt",
                    "root/a/3.txt",
                    "root/a/1.txt");
                Tree tree = new Tree();

                // When
                IReadOnlyList<TestDocument> results = await ExecuteAsync(inputs, tree);

                // Then
                VerifyTreeChildren(
                    results[0],
                    "root/a/b/2.txt");
                VerifyTreeChildren(
                    results[1],
                    "root/a/3.txt");
                VerifyTreeChildren(
                    results[2],
                    "root/a/1.txt");
                VerifyTreeChildren(
                    results[3],
                    "root/a/b/index.html",
                    "root/a/b/2.txt");
                VerifyTreeChildren(
                    results[4],
                    "root/a/index.html",
                    "root/a/1.txt",
                    "root/a/3.txt",
                    "root/a/b/index.html");
                VerifyTreeChildren(
                    results[5],
                    "root/index.html",
                    "root/a/index.html");
            }

            private TestDocument FindTreeNode(TestDocument first, string relativeFilePath)
            {
                while (first != null && first.FilePath(Keys.RelativeFilePath).FullPath != relativeFilePath)
                {
                    first = (TestDocument)first.Document(Keys.Next);
                }
                return first;
            }

            private void VerifyTree(TestDocument document, params string[] relativeFilePaths)
            {
                foreach (string relativeFilePath in relativeFilePaths)
                {
                    document.ShouldNotBeNull();
                    document.FilePath(Keys.RelativeFilePath).FullPath.ShouldBe(relativeFilePath);
                    document = (TestDocument)document.Document(Keys.Next);
                }
            }

            private void VerifyTreeChildren(TestDocument parent, string parentPath, params string[] childFilePaths)
            {
                parent.ShouldNotBeNull();
                parent.FilePath(Keys.RelativeFilePath).FullPath.ShouldBe(parentPath);
                IReadOnlyList<IDocument> children = parent.DocumentList(Keys.Children);
                children.Select(x => x.FilePath(Keys.RelativeFilePath).FullPath).ShouldBe(childFilePaths);
            }

            private TestDocument[] GetDocumentsFromRelativePaths(params string[] relativeFilePaths) =>
                relativeFilePaths.Select(x => new TestDocument(new MetadataItems
                {
                    new MetadataItem(Keys.RelativeFilePath, new FilePath(x))
                })).ToArray();
        }
    }
}
