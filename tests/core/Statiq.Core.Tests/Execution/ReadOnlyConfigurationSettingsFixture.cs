using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using Shouldly;
using Statiq.Common;
using Statiq.Testing;

namespace Statiq.Core.Tests.Execution
{
    [TestFixture]
    public class ReadOnlyConfigurationSettingsFixture : BaseFixture
    {
        public class TryGetValueTests : ReadOnlyConfigurationSettingsFixture
        {
            [Test]
            public void GetsNormalValue()
            {
                // Given
                IConfiguration configuration = GetConfiguration(@"
{
  ""foo"": ""ABC {1+2} XYZ""
}");
                TestExecutionContext context = new TestExecutionContext();
                context.ScriptHelper = new ScriptHelper(context);
                ReadOnlyConfigurationSettings settings = new ReadOnlyConfigurationSettings(context, configuration, null);

                // When
                bool result = settings.TryGetValue("foo", out object value);

                // Then
                result.ShouldBeTrue();
                value.ShouldBe("ABC {1+2} XYZ");
            }

            [Test]
            public void GetsOVerridenValue()
            {
                // Given
                IConfiguration configuration = GetConfiguration(@"
{
  ""foo"": ""ABC {1+2} XYZ""
}");
                TestExecutionContext context = new TestExecutionContext();
                context.ScriptHelper = new ScriptHelper(context);
                Dictionary<string, object> overrides = new Dictionary<string, object>
                {
                    { "foo", 123 }
                };
                ReadOnlyConfigurationSettings settings = new ReadOnlyConfigurationSettings(context, configuration, overrides);

                // When
                bool result = settings.TryGetValue("foo", out object value);

                // Then
                result.ShouldBeTrue();
                value.ShouldBe(123);
            }

            [Test]
            public void EvaluatesExpression()
            {
                // Given
                IConfiguration configuration = GetConfiguration(@"
{
  ""foo"": ""=> $\""ABC {1+2} XYZ\""""
}");
                TestExecutionContext context = new TestExecutionContext();
                context.ScriptHelper = new ScriptHelper(context);
                ReadOnlyConfigurationSettings settings = new ReadOnlyConfigurationSettings(context, configuration, null);

                // When
                bool result = settings.TryGetValue("foo", out object value);

                // Then
                result.ShouldBeTrue();
                value.ShouldBe("ABC 3 XYZ");
            }

            [Test]
            public void EvaluatesReturnStatement()
            {
                // Given
                IConfiguration configuration = GetConfiguration(@"
{
  ""foo"": ""=> return $\""ABC {1+2} XYZ\"";""
}");
                TestExecutionContext context = new TestExecutionContext();
                context.ScriptHelper = new ScriptHelper(context);
                ReadOnlyConfigurationSettings settings = new ReadOnlyConfigurationSettings(context, configuration, null);

                // When
                bool result = settings.TryGetValue("foo", out object value);

                // Then
                result.ShouldBeTrue();
                value.ShouldBe("ABC 3 XYZ");
            }

            [Test]
            public void EvaluatesMultipleStatements()
            {
                // Given
                IConfiguration configuration = GetConfiguration(@"
{
  ""foo"": ""=> { int x = 1 + 2; return $\""ABC {x} XYZ\""; }""
}");
                TestExecutionContext context = new TestExecutionContext();
                context.ScriptHelper = new ScriptHelper(context);
                ReadOnlyConfigurationSettings settings = new ReadOnlyConfigurationSettings(context, configuration, null);

                // When
                bool result = settings.TryGetValue("foo", out object value);

                // Then
                result.ShouldBeTrue();
                value.ShouldBe("ABC 3 XYZ");
            }

            [Test]
            public void EvaluatesMultipleStatementsWithoutBraces()
            {
                // Given
                IConfiguration configuration = GetConfiguration(@"
{
  ""foo"": ""=> int x = 1 + 2; return $\""ABC {x} XYZ\"";""
}");
                TestExecutionContext context = new TestExecutionContext();
                context.ScriptHelper = new ScriptHelper(context);
                ReadOnlyConfigurationSettings settings = new ReadOnlyConfigurationSettings(context, configuration, null);

                // When
                bool result = settings.TryGetValue("foo", out object value);

                // Then
                result.ShouldBeTrue();
                value.ShouldBe("ABC 3 XYZ");
            }

            [Test]
            public void EvaluatesIntExpression()
            {
                // Given
                IConfiguration configuration = GetConfiguration(@"
{
  ""foo"": ""=> 1 + 2""
}");
                TestExecutionContext context = new TestExecutionContext();
                context.ScriptHelper = new ScriptHelper(context);
                ReadOnlyConfigurationSettings settings = new ReadOnlyConfigurationSettings(context, configuration, null);

                // When
                bool result = settings.TryGetValue("foo", out object value);

                // Then
                result.ShouldBeTrue();
                value.ShouldBe(3);
            }

            [Test]
            public void EvaluatesExpressionInNestedSection()
            {
                // Given
                IConfiguration configuration = GetConfiguration(@"
{
  ""section0"": {
    ""foo"": ""=> $\""ABC {1+2} XYZ\""""
  }
}");
                TestExecutionContext context = new TestExecutionContext();
                context.ScriptHelper = new ScriptHelper(context);
                ReadOnlyConfigurationSettings settings = new ReadOnlyConfigurationSettings(context, configuration, null);

                // When
                bool result = settings.TryGetValue("section0:foo", out object value);

                // Then
                result.ShouldBeTrue();
                value.ShouldBe("ABC 3 XYZ");
            }

            [Test]
            public void EvaluatesExpressionThroughNestedSection()
            {
                // Given
                IConfiguration configuration = GetConfiguration(@"
{
  ""section0"": {
    ""foo"": ""=> $\""ABC {1+2} XYZ\""""
  }
}");
                TestExecutionContext context = new TestExecutionContext();
                context.ScriptHelper = new ScriptHelper(context);
                ReadOnlyConfigurationSettings settings = new ReadOnlyConfigurationSettings(context, configuration, null);

                // When
                bool sectionResult = settings.TryGetValue<IMetadata>("section0", out IMetadata section);
                bool result = section.TryGetValue("section0:foo", out object value);

                // Then
                sectionResult.ShouldBeTrue();
                section.ShouldBeOfType<ReadOnlyConfigurationSettings>();
                result.ShouldBeTrue();
                value.ShouldBe("ABC 3 XYZ");
            }

            [Test]
            public void CanAccessOtherValuesInExpression()
            {
                // Given
                IConfiguration configuration = GetConfiguration(@"
{
  ""foo"": ""=> $\""ABC {bar} XYZ\"""",
  ""bar"": 3
}");
                TestExecutionContext context = new TestExecutionContext();
                context.ScriptHelper = new ScriptHelper(context);
                ReadOnlyConfigurationSettings settings = new ReadOnlyConfigurationSettings(context, configuration, null);

                // When
                bool result = settings.TryGetValue("foo", out object value);

                // Then
                result.ShouldBeTrue();
                value.ShouldBe("ABC 3 XYZ");
            }
        }

        private static IConfiguration GetConfiguration(string json)
        {
            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);
            writer.Write(json);
            writer.Flush();
            stream.Position = 0;
            return new ConfigurationBuilder().AddJsonStream(stream).Build();
        }
    }
}
