using System;
using System.Collections;
using System.Dynamic;
using System.Threading.Tasks;
using NUnit.Framework;
using Shouldly;
using Statiq.Common;
using Statiq.Testing;

namespace Statiq.Json.Tests
{
    [TestFixture]
    public class JsonFixture : BaseFixture
    {
        private static string _jsonContent = @"{
  ""Email"": ""james@example.com"",
  ""Active"": true,
  ""CreatedDate"": ""2013-01-20T00:00:00Z"",
  ""Roles"": [
    ""User"",
    ""Admin""
  ]
}";

        public class ExecuteTests : JsonFixture
        {
            [Test]
            public async Task GeneratesDynamicObject()
            {
                // Given
                TestDocument document = new TestDocument(_jsonContent);
                Json json = new Json("MyJson");

                // When
                TestDocument result = await ExecuteAsync(document, json).SingleAsync();

                // Then
                result.Count.ShouldBe(6); // Includes property metadata
                result["MyJson"].ShouldBeOfType<ExpandoObject>();
                ((string)((dynamic)result["MyJson"]).Email).ShouldBe("james@example.com");
                ((bool)((dynamic)result["MyJson"]).Active).ShouldBeTrue();
                ((DateTime)((dynamic)result["MyJson"]).CreatedDate).ShouldBe(new DateTime(2013, 1, 20, 0, 0, 0, DateTimeKind.Utc));
                ((IEnumerable)((dynamic)result["MyJson"]).Roles).ShouldBe(new[] { "User", "Admin" });
            }

            [Test]
            public async Task FlattensTopLevel()
            {
                // Given
                TestDocument document = new TestDocument(_jsonContent);
                Json json = new Json();

                // When
                TestDocument result = await ExecuteAsync(document, json).SingleAsync();

                // Then
                result.Count.ShouldBe(9); // Includes property metadata
                ((string)result["Email"]).ShouldBe("james@example.com");
                ((bool)result["Active"]).ShouldBeTrue();
                ((DateTime)result["CreatedDate"]).ShouldBe(new DateTime(2013, 1, 20, 0, 0, 0, DateTimeKind.Utc));
                ((IEnumerable)result["Roles"]).ShouldBe(new[] { "User", "Admin" });
            }

            [Test]
            [NonParallelizable]
            public async Task ReturnsDocumentOnError()
            {
                // Given
                RemoveListener();
                TestDocument document = new TestDocument("asdf");
                Json json = new Json("MyJson");

                // When
                TestDocument result = await ExecuteAsync(document, json).SingleAsync();

                // Then
                result.ShouldBe(document);
            }
        }
    }
}