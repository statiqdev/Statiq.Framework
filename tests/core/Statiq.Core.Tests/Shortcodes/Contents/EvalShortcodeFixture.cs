using System.Threading.Tasks;
using NUnit.Framework;
using Shouldly;
using Statiq.Testing;

namespace Statiq.Core.Tests.Shortcodes.Contents
{
    [TestFixture]
    public class EvalShortcodeFixture : BaseFixture
    {
        public class ExecuteTests : EvalShortcodeFixture
        {
            [Test]
            public async Task RendersEval()
            {
                TestExecutionContext context = new TestExecutionContext();
                TestDocument document = new TestDocument();
                EvalShortcode shortcode = new EvalShortcode();
                string shortcodeContent = "return 1 + 2;";

                // When
                TestDocument result = (TestDocument)await shortcode.ExecuteAsync(null, shortcodeContent, document, context);

                // Then
                result.Content.ShouldBe("3");
            }
        }
    }
}
