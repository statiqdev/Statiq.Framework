using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using Shouldly;
using Statiq.Testing;

namespace Statiq.Core.Tests.Execution
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
                IConfiguration configuration = GetConfiguration();

                // When
                object graph = Settings.BuildConfigurationObject(configuration, true);

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

        private static IConfiguration GetConfiguration()
        {
            string json = @"{
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
            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);
            writer.Write(json);
            writer.Flush();
            stream.Position = 0;
            return new ConfigurationBuilder().AddJsonStream(stream).Build();
        }
    }
}
