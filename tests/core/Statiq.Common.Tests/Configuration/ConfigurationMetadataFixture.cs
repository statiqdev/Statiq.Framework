using System;
using System.Collections.Generic;
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
        public class CountTests : ConfigurationMetadataFixture
        {
            [Test]
            public void EnsureCountIsNotRecursive()
            {
                // Given
                IConfiguration configuration = new ConfigurationBuilder()
                    .AddInMemoryCollection(new Dictionary<string, string>
                    {
                        { "Foo", "1" },
                        { "Bar", "2" }
                    }).Build();
                ConfigurationMetadata metadata = new ConfigurationMetadata(configuration);

                // When
                int count = metadata.Count;

                // Then
                count.ShouldBe(2);
            }
        }
    }
}
