using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using Shouldly;
using Statiq.Common;
using Statiq.Testing;

namespace Statiq.Core.Tests.Shortcodes.Content
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
                context.ScriptHelper = new ScriptHelper(context);
                TestDocument document = new TestDocument();
                EvalShortcode shortcode = new EvalShortcode();
                string shortcodeContent = "return 1 + 2;";

                // When
                string result = await shortcode.ExecuteAsync(null, shortcodeContent, document, context);

                // Then
                result.ShouldBe("3");
            }

            [Test]
            public async Task EvaluatesExpression()
            {
                TestExecutionContext context = new TestExecutionContext();
                context.ScriptHelper = new ScriptHelper(context);
                TestDocument document = new TestDocument();
                EvalShortcode shortcode = new EvalShortcode();
                string shortcodeContent = "1 + 2";

                // When
                string result = await shortcode.ExecuteAsync(null, shortcodeContent, document, context);

                // Then
                result.ShouldBe("3");
            }

            [Test]
            public async Task CanAccessMetadata()
            {
                TestExecutionContext context = new TestExecutionContext();
                context.ScriptHelper = new ScriptHelper(context);
                TestDocument document = new TestDocument
                {
                    { "Foo", "4" }
                };
                EvalShortcode shortcode = new EvalShortcode();
                string shortcodeContent = "return 1 + GetInt(\"Foo\");";

                // When
                string result = await shortcode.ExecuteAsync(null, shortcodeContent, document, context);

                // Then
                result.ShouldBe("5");
            }

            [Test]
            public async Task CanAccessMetadataAsProperties()
            {
                TestExecutionContext context = new TestExecutionContext();
                context.ScriptHelper = new ScriptHelper(context);
                TestDocument document = new TestDocument
                {
                    { "Foo", 4 }
                };
                EvalShortcode shortcode = new EvalShortcode();
                string shortcodeContent = "return 1 + (int)Foo;";

                // When
                string result = await shortcode.ExecuteAsync(null, shortcodeContent, document, context);

                // Then
                result.ShouldBe("5");
            }
        }
    }
}
