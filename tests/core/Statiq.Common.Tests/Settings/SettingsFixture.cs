using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using Shouldly;
using Statiq.Core;
using Statiq.Testing;

namespace Statiq.Common.Tests.Settings
{
    [TestFixture]
    public class SettingsFixture : BaseFixture
    {
        public class BuildConfigurationObjectTests : SettingsFixture
        {
            [Test]
            public void ConstructsObjectGraph()
            {
                // Given
                IConfigurationRoot configuration = GetConfiguration();

                // When
                object graph = Common.Settings.BuildConfigurationObject(configuration, null);

                // Then
                IDictionary<string, object> root = graph.ShouldBeAssignableTo<IDictionary<string, object>>();
                root.ShouldContainKeyAndValue("key0", "value0");
                root.ShouldContainKey("section0");
                root.ShouldContainKey("section1");
                root.ShouldContainKey("section2");
                IDictionary<string, object> dictionary = root["section0"].ShouldBeAssignableTo<IDictionary<string, object>>();
                dictionary.ShouldContainKeyAndValue("key1", "value1");
                dictionary.ShouldContainKey("key2");
                IList<object> list = dictionary["key2"].ShouldBeAssignableTo<IList<object>>();
                list.Count.ShouldBe(3);
                list[0].ShouldBe("arr1");
                list[1].ShouldBe("arr2");
                dictionary = list[2].ShouldBeAssignableTo<IDictionary<string, object>>();
                dictionary.ShouldContainKeyAndValue("foo1", "bar1");
                dictionary.ShouldContainKeyAndValue("foo2", "bar2");
                dictionary = root["section1"].ShouldBeAssignableTo<IDictionary<string, object>>();
                dictionary.ShouldContainKeyAndValue("key3", "3");
                dictionary.ShouldContainKeyAndValue("key4", "value4");
                dictionary = root["section2"].ShouldBeAssignableTo<IDictionary<string, object>>();
                dictionary.ShouldContainKey("subsection0");
                IDictionary<string, object> subsection = dictionary["subsection0"].ShouldBeAssignableTo<IDictionary<string, object>>();
                subsection.ShouldContainKeyAndValue("key5", "value5");
                subsection.ShouldContainKeyAndValue("key6", "value6");
                dictionary.ShouldContainKey("subsection1");
                subsection = dictionary["subsection1"].ShouldBeAssignableTo<IDictionary<string, object>>();
                subsection.ShouldContainKeyAndValue("key7", "value7");
                subsection.ShouldContainKeyAndValue("key8", "value8");
            }
        }

        public class TryGetValueTests : SettingsFixture
        {
            [Test]
            public void GetsNormalValue()
            {
                // Given
                IConfigurationRoot configuration = GetConfiguration(@"
{
  ""foo"": ""ABC {1+2} XYZ""
}");
                TestExecutionContext context = new TestExecutionContext();
                context.ScriptHelper = new ScriptHelper(context);
                Common.Settings settings = new Common.Settings(configuration).WithExecutionState(context);

                // When
                bool result = settings.TryGetValue("foo", out object value);

                // Then
                result.ShouldBeTrue();
                value.ShouldBe("ABC {1+2} XYZ");
            }

            [Test]
            public void GetsOverridenValue()
            {
                // Given
                IConfigurationRoot configuration = GetConfiguration(@"
{
  ""foo"": ""ABC {1+2} XYZ""
}");
                TestExecutionContext context = new TestExecutionContext();
                context.ScriptHelper = new ScriptHelper(context);
                Common.Settings settings = new Common.Settings(configuration);
                settings["foo"] = 123;
                settings = settings.WithExecutionState(context);

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
                IConfigurationRoot configuration = GetConfiguration(@"
{
  ""foo"": ""=> $\""ABC {1+2} XYZ\""""
}");
                TestExecutionContext context = new TestExecutionContext();
                context.ScriptHelper = new ScriptHelper(context);
                Common.Settings settings = new Common.Settings(configuration).WithExecutionState(context);

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
                IConfigurationRoot configuration = GetConfiguration(@"
{
  ""foo"": ""=> return $\""ABC {1+2} XYZ\"";""
}");
                TestExecutionContext context = new TestExecutionContext();
                context.ScriptHelper = new ScriptHelper(context);
                Common.Settings settings = new Common.Settings(configuration).WithExecutionState(context);

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
                IConfigurationRoot configuration = GetConfiguration(@"
{
  ""foo"": ""=> { int x = 1 + 2; return $\""ABC {x} XYZ\""; }""
}");
                TestExecutionContext context = new TestExecutionContext();
                context.ScriptHelper = new ScriptHelper(context);
                Common.Settings settings = new Common.Settings(configuration).WithExecutionState(context);

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
                IConfigurationRoot configuration = GetConfiguration(@"
{
  ""foo"": ""=> int x = 1 + 2; return $\""ABC {x} XYZ\"";""
}");
                TestExecutionContext context = new TestExecutionContext();
                context.ScriptHelper = new ScriptHelper(context);
                Common.Settings settings = new Common.Settings(configuration).WithExecutionState(context);

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
                IConfigurationRoot configuration = GetConfiguration(@"
{
  ""foo"": ""=> 1 + 2""
}");
                TestExecutionContext context = new TestExecutionContext();
                context.ScriptHelper = new ScriptHelper(context);
                Common.Settings settings = new Common.Settings(configuration).WithExecutionState(context);

                // When
                bool result = settings.TryGetValue("foo", out object value);

                // Then
                result.ShouldBeTrue();
                value.ShouldBe(3);
            }

            [Test]
            public void CanAccessOtherValuesInExpression()
            {
                // Given
                IConfigurationRoot configuration = GetConfiguration(@"
{
  ""foo"": ""=> $\""ABC {Get(\""bar\"")} XYZ\"""",
  ""bar"": 3
}");
                TestExecutionContext context = new TestExecutionContext();
                context.ScriptHelper = new ScriptHelper(context);
                Common.Settings settings = new Common.Settings(configuration).WithExecutionState(context);

                // When
                bool result = settings.TryGetValue("foo", out object value);

                // Then
                result.ShouldBeTrue();
                value.ShouldBe("ABC 3 XYZ");
            }
        }

        public class IndexerTests : SettingsFixture
        {
            [Test]
            public void EvaluatesLateAddedSettingExpression()
            {
                // Given
                IConfigurationRoot configuration = new ConfigurationRoot(Array.Empty<IConfigurationProvider>());
                TestExecutionContext context = new TestExecutionContext();
                context.ScriptHelper = new ScriptHelper(context);
                Common.Settings settings = new Common.Settings(configuration).WithExecutionState(context);

                // When
                settings.Add("Foo", @"=> $""ABC {1+2} XYZ""");

                // Then
                settings.TryGetValue("foo", out object value).ShouldBeTrue();
                value.ShouldBe("ABC 3 XYZ");
            }
        }

        public class IConfigurationTests : SettingsFixture
        {
            [Test]
            public void EvaluatesExpressionInNestedSection()
            {
                // Given
                IConfigurationRoot configuration = GetConfiguration(@"
{
  ""section0"": {
    ""foo"": ""=> $\""ABC {1+2} XYZ\""""
  }
}");
                TestExecutionContext context = new TestExecutionContext();
                context.ScriptHelper = new ScriptHelper(context);
                IConfiguration settingsConfiguration = new Common.Settings(configuration).WithExecutionState(context);

                // When
                IConfigurationSection section = settingsConfiguration.GetSection("section0:foo");

                // Then
                section.Key.ShouldBe("foo");
                section.Path.ShouldBe("section0:foo");
                section.Value.ShouldBe("ABC 3 XYZ");
            }

            [Test]
            public void EvaluatesExpressionThroughNestedSection()
            {
                // Given
                IConfigurationRoot configuration = GetConfiguration(@"
{
  ""section0"": {
    ""foo"": ""=> $\""ABC {1+2} XYZ\""""
  }
}");
                TestExecutionContext context = new TestExecutionContext();
                context.ScriptHelper = new ScriptHelper(context);
                IConfiguration settingsConfiguration = new Common.Settings(configuration).WithExecutionState(context);

                // When
                IConfigurationSection section = settingsConfiguration.GetSection("section0");
                IConfigurationSection result = section.GetSection("foo");

                // Then
                section.Key.ShouldBe("Section0");
                section.Path.ShouldBe("section0");
                section.Value.ShouldBeNull();
                result.Key.ShouldBe("foo");
                result.Path.ShouldBe("section0:foo");
                result.Value.ShouldBe("ABC 3 XYZ");
            }
        }

        private static IConfigurationRoot GetConfiguration(string json = Json)
        {
            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);
            writer.Write(json);
            writer.Flush();
            stream.Position = 0;
            return new ConfigurationBuilder().AddJsonStream(stream).Build();
        }

        private const string Json = @"{
  ""key0"": ""value0"",
  ""section0"": {
    ""key1"": ""value1"",
    ""key2"": [
      ""arr1"",
      ""arr2"",
      {
        ""foo1"": ""bar1"",
        ""foo2"": ""bar2""
      }
    ]
  },
  ""section1"": {
    ""key3"": ""3"",
    ""key4"": ""value4""
  },
  ""section2"": {
    ""subsection0"" : {
      ""key5"": ""value5"",
      ""key6"": ""value6""
    },
    ""subsection1"" : {
      ""key7"": ""value7"",
      ""key8"": ""value8""
    }
  }
}";
    }
}
