using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Statiq.Common;
using Statiq.Testing;
using Statiq.Testing.Documents;
using Statiq.Testing.Execution;
using Statiq.Testing.IO;

namespace Statiq.CodeAnalysis.Tests
{
    public abstract class AnalyzeCSharpBaseFixture : BaseFixture
    {
        protected TestDocument GetDocument(string content) => new TestDocument(content);

        protected TestExecutionContext GetContext() => new TestExecutionContext
        {
            FileSystem = new TestFileSystem
            {
                RootPath = new DirectoryPath(TestContext.CurrentContext.TestDirectory)
            }
        };

        protected TestDocument GetResult(IReadOnlyList<TestDocument> results, string name)
        {
            return results.Single(x => x["Name"].Equals(name));
        }

        protected TestDocument GetMember(IReadOnlyList<TestDocument> results, string className, string memberName)
        {
            return GetResult(results, className)
                .Get<IEnumerable<IDocument>>("Members")
                .Cast<TestDocument>()
                .Single(x => x["Name"].Equals(memberName));
        }

        protected TestDocument GetParameter(IReadOnlyList<TestDocument> results, string className, string methodName, string parameterName)
        {
            return GetResult(results, className)
                .Get<IEnumerable<IDocument>>("Members")
                .Cast<TestDocument>()
                .Single(x => x["Name"].Equals(methodName))
                .Get<IEnumerable<IDocument>>("Parameters")
                .Cast<TestDocument>()
                .Single(x => x["Name"].Equals(parameterName));
        }
    }
}
