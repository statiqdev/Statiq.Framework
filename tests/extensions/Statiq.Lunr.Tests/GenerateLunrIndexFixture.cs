using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Shouldly;
using Statiq.Common;
using Statiq.Testing;

namespace Statiq.Lunr.Tests
{
    [TestFixture]
    public class GenerateLunrIndexFixture : BaseFixture
    {
        public class ExecuteTests : GenerateLunrIndexFixture
        {
            [Test]
            public async Task AddsDocumentsToIndex()
            {
                // Given
                TestDocument a = new TestDocument((NormalizedPath)"a/a.html", "Fizz")
                {
                    { Keys.Title, "Foo" }
                };
                TestDocument b = new TestDocument((NormalizedPath)"b.html", "Buzz")
                {
                    { Keys.Title, "Bar" }
                };
                GenerateLunrIndexOld module = new GenerateLunrIndexOld();

                // When
                TestDocument result = await ExecuteAsync(new[] { a, b }, module).SingleAsync();

                // Then
                result.Content.ShouldContain(@"url:'/a/a.html'");
                result.Content.ShouldContain(@"url:'/b.html'");
            }

            [Test]
            public async Task HidesDocumentFromSearchIndex()
            {
                // Given
                TestDocument a = new TestDocument((NormalizedPath)"a/a.html", "Fizz")
                {
                    { Keys.Title, "Foo" }
                };
                TestDocument b = new TestDocument((NormalizedPath)"b.html", "Buzz")
                {
                    { Keys.Title, "Bar" },
                    { LunrKeys.HideFromSearchIndex, true }
                };
                GenerateLunrIndexOld module = new GenerateLunrIndexOld();

                // When
                TestDocument result = await ExecuteAsync(new[] { a, b }, module).SingleAsync();

                // Then
                result.Content.ShouldContain(@"url:'/a/a.html'");
                result.Content.ShouldNotContain(@"url:'/b.html'");
            }

            [Test]
            public async Task DefaultOutputDestination()
            {
                // Given
                TestDocument a = new TestDocument((NormalizedPath)"a/a.html", "Fizz")
                {
                    { Keys.Title, "Foo" }
                };
                TestDocument b = new TestDocument((NormalizedPath)"b.html", "Buzz")
                {
                    { Keys.Title, "Bar" }
                };
                GenerateLunrIndexOld module = new GenerateLunrIndexOld();

                // When
                TestDocument result = await ExecuteAsync(new[] { a, b }, module).SingleAsync();

                // Then
                result.Destination.ShouldBe("searchindex.js");
            }

            [Test]
            public async Task SetsOutputDestination()
            {
                // Given
                TestDocument a = new TestDocument((NormalizedPath)"a/a.html", "Fizz")
                {
                    { Keys.Title, "Foo" }
                };
                TestDocument b = new TestDocument((NormalizedPath)"b.html", "Buzz")
                {
                    { Keys.Title, "Bar" }
                };
                GenerateLunrIndexOld module = new GenerateLunrIndexOld().WithDestination("search/index.js");

                // When
                TestDocument result = await ExecuteAsync(new[] { a, b }, module).SingleAsync();

                // Then
                result.Destination.ShouldBe("search/index.js");
            }

            [Test]
            public async Task DoesNotIncludeHostByDefault()
            {
                // Given
                TestDocument a = new TestDocument((NormalizedPath)"a/a.html", "Fizz")
                {
                    { Keys.Title, "Foo" }
                };
                TestExecutionContext context = new TestExecutionContext();
                context.Settings.Add(Keys.Host, "bar.com");
                GenerateLunrIndexOld module = new GenerateLunrIndexOld();

                // When
                TestDocument result = await ExecuteAsync(a, context, module).SingleAsync();

                // Then
                result.Content.ShouldContain(@"url:'/a/a.html'");
            }

            [Test]
            public async Task IncludesHost()
            {
                // Given
                TestDocument a = new TestDocument((NormalizedPath)"a/a.html", "Fizz")
                {
                    { Keys.Title, "Foo" }
                };
                TestExecutionContext context = new TestExecutionContext();
                context.Settings.Add(Keys.Host, "bar.com");
                GenerateLunrIndexOld module = new GenerateLunrIndexOld().IncludeHost();

                // When
                TestDocument result = await ExecuteAsync(a, context, module).SingleAsync();

                // Then
                result.Content.ShouldContain(@"url:'http://bar.com/a/a.html'");
            }

            [Test]
            public async Task TransformsScript()
            {
                // Given
                TestDocument a = new TestDocument((NormalizedPath)"a/a.html", "Fizz")
                {
                    { Keys.Title, "Foo" }
                };
                TestExecutionContext context = new TestExecutionContext();
                context.Settings.Add(Keys.Host, "bar.com");
                GenerateLunrIndexOld module = new GenerateLunrIndexOld().WithScript((b, ctx) =>
                {
                    b.Append("HI!");
                    return b.ToString();
                });

                // When
                TestDocument result = await ExecuteAsync(a, context, module).SingleAsync();

                // Then
                result.Content.ShouldEndWith("HI!");
            }

            [Test]
            public async Task UsesCustomIndexItem()
            {
                // Given
                TestDocument a = new TestDocument((NormalizedPath)"a/a.html", "Fizz")
                {
                    { Keys.Title, "Foo" }
                };
                TestDocument b = new TestDocument((NormalizedPath)"b.html", "Buzz")
                {
                    { Keys.Title, "Bar" },
                    {
                        LunrKeys.LunrIndexItem,
                        new LunrIndexItem("/c/c.html", "Baz", "Fizz")
                    }
                };
                GenerateLunrIndexOld module = new GenerateLunrIndexOld();

                // When
                TestDocument result = await ExecuteAsync(new[] { a, b }, module).SingleAsync();

                // Then
                result.Content.ShouldContain(@"url:'/a/a.html'");
                result.Content.ShouldNotContain(@"url:'/b.html'");
                result.Content.ShouldContain(@"url:'/c/c.html'");
                result.Content.ShouldContain(@"title:""Baz""");
                result.Content.ShouldContain(@"content:""Fizz""");
            }
        }
    }
}
