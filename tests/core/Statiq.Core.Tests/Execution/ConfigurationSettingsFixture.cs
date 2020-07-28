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
    public class ConfigurationSettingsFixture : BaseFixture
    {
        public class TryGetValueTests : ConfigurationSettingsFixture
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
                ConfigurationSettings settings = new ConfigurationSettings(context, configuration, null);

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
                ConfigurationSettings settings = new ConfigurationSettings(context, configuration, overrides);

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
                ConfigurationSettings settings = new ConfigurationSettings(context, configuration, null);

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
                ConfigurationSettings settings = new ConfigurationSettings(context, configuration, null);

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
                ConfigurationSettings settings = new ConfigurationSettings(context, configuration, null);

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
                ConfigurationSettings settings = new ConfigurationSettings(context, configuration, null);

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
                ConfigurationSettings settings = new ConfigurationSettings(context, configuration, null);

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
                ConfigurationSettings settings = new ConfigurationSettings(context, configuration, null);

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
                ConfigurationSettings settings = new ConfigurationSettings(context, configuration, null);

                // When
                bool sectionResult = settings.TryGetValue<IMetadata>("section0", out IMetadata section);
                bool result = section.TryGetValue("section0:foo", out object value);

                // Then
                sectionResult.ShouldBeTrue();
                section.ShouldBeOfType<ConfigurationSettings>();
                result.ShouldBeTrue();
                value.ShouldBe("ABC 3 XYZ");
            }

            [Test]
            public void CanAccessOtherValuesInExpression()
            {
                // Given
                IConfiguration configuration = GetConfiguration(@"
{
  ""foo"": ""=> $\""ABC {Get(\""bar\"")} XYZ\"""",
  ""bar"": 3
}");
                TestExecutionContext context = new TestExecutionContext();
                context.ScriptHelper = new ScriptHelper(context);
                ConfigurationSettings settings = new ConfigurationSettings(context, configuration, null);

                // When
                bool result = settings.TryGetValue("foo", out object value);

                // Then
                result.ShouldBeTrue();
                value.ShouldBe("ABC 3 XYZ");
            }
        }

        public class ConstructorTests : ConfigurationSettingsFixture
        {
            [Test]
            public void SettingsOverrideConfiguration()
            {
                // Given
                IConfiguration configuration = GetConfiguration(@"
{
  ""foo"": ""fizz""
}");
                IDictionary<string, object> settings = new Dictionary<string, object>
                {
                    { "Foo", "Buzz" }
                };
                TestExecutionContext context = new TestExecutionContext();
                context.ScriptHelper = new ScriptHelper(context);

                // When
                ConfigurationSettings configurationSettings = new ConfigurationSettings(context, configuration, settings);

                // Then
                configurationSettings.TryGetValue("foo", out object value).ShouldBeTrue();
                value.ShouldBe("Buzz");
            }

            [Test]
            public void EvaluatesSettingExpressionInCtor()
            {
                // Given
                IDictionary<string, object> settings = new Dictionary<string, object>
                {
                    { "Foo", @"=> $""ABC {1+2} XYZ""" }
                };
                IConfiguration configuration = new ConfigurationRoot(Array.Empty<IConfigurationProvider>());
                TestExecutionContext context = new TestExecutionContext();
                context.ScriptHelper = new ScriptHelper(context);

                // When
                ConfigurationSettings configurationSettings = new ConfigurationSettings(context, configuration, settings);

                // Then
                configurationSettings.TryGetValue("foo", out object value).ShouldBeTrue();
                value.ShouldBe("ABC 3 XYZ");
            }
        }

        public class IndexerTests : ConfigurationSettingsFixture
        {
            [Test]
            public void EvaluatesLateAddedSettingExpression()
            {
                // Given
                IConfiguration configuration = new ConfigurationRoot(Array.Empty<IConfigurationProvider>());
                TestExecutionContext context = new TestExecutionContext();
                context.ScriptHelper = new ScriptHelper(context);
                ConfigurationSettings configurationSettings = new ConfigurationSettings(context, configuration, null);

                // When
                configurationSettings.Add("Foo", @"=> $""ABC {1+2} XYZ""");

                // Then
                configurationSettings.TryGetValue("foo", out object value).ShouldBeTrue();
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
