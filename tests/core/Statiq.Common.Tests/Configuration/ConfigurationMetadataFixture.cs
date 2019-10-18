using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using Shouldly;
using Statiq.Testing;

namespace Statiq.Common.Tests.Configuration
{
    [TestFixture]
    public class ConfigurationMetadataFixture : BaseFixture
    {
        public class ContainsKeyTests : ConfigurationMetadataFixture
        {
            [TestCase("key0", true)]
            [TestCase("section0", true)]
            [TestCase("section0:key1", true)]
            [TestCase("sectiona", false)]
            [TestCase("section2:subsection0", true)]
            [TestCase("section2:subsection0:key5", true)]
            public void ContainsKey(string key, bool expected)
            {
                // Given
                IConfiguration configuration = GetConfiguration();
                ConfigurationMetadata metadata = new ConfigurationMetadata(configuration);

                // When
                bool result = metadata.ContainsKey(key);

                // Then
                result.ShouldBe(expected);
            }
        }

        public class TryGetRawTests : ConfigurationMetadataFixture
        {
            [Test]
            public void GetsSimpleValue()
            {
                // Given
                IConfiguration configuration = GetConfiguration();
                ConfigurationMetadata metadata = new ConfigurationMetadata(configuration);

                // When
                bool result = metadata.TryGetRaw("key0", out object value);

                // Then
                result.ShouldBeTrue();
                value.ShouldBe("value0");
            }

            [Test]
            public void GetsSectionValue()
            {
                // Given
                IConfiguration configuration = GetConfiguration();
                ConfigurationMetadata metadata = new ConfigurationMetadata(configuration);

                // When
                bool result = metadata.TryGetRaw("section0", out object value);
                object value2 = null;
                bool result2 = (value as ConfigurationMetadata)?.TryGetRaw("key1", out value2) ?? false;

                // Then
                result.ShouldBeTrue();
                value.ShouldBeOfType<ConfigurationMetadata>();
                result2.ShouldBeTrue();
                value2.ShouldBe("value1");
            }
        }

        public class CountTests : ConfigurationMetadataFixture
        {
            [Test]
            public void EnsureCountIsNotRecursive()
            {
                // Given
                IConfiguration configuration = GetConfiguration();
                ConfigurationMetadata metadata = new ConfigurationMetadata(configuration);

                // When
                int count = metadata.Count;

                // Then
                count.ShouldBe(14);
            }
        }

        private static IConfiguration GetConfiguration()
        {
            string json = @"
{
  ""key0"": ""value0"",
  ""section0"": {
    ""key1"": ""value1"",
    ""key2"": ""value2""
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
