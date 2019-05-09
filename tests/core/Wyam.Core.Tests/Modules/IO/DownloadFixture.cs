using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using NUnit.Framework;
using Shouldly;
using Wyam.Common;
using Wyam.Common.Documents;
using Wyam.Common.Meta;
using Wyam.Common.Modules;
using Wyam.Core.Modules.IO;
using Wyam.Testing;
using Wyam.Testing.Documents;
using Wyam.Testing.Execution;

namespace Wyam.Core.Tests.Modules.IO
{
    [TestFixture]
    [NonParallelizable]
    public class DownloadFixture : BaseFixture
    {
        public class ExecuteTests : DownloadFixture
        {
            [Test]
            public async Task SingleHtmlDownloadGetStream()
            {
                // Given
                TestDocument document = new TestDocument();
                TestExecutionContext context = new TestExecutionContext
                {
                    HttpResponseFunc = (_, __) =>
                    {
                        HttpResponseMessage response = new HttpResponseMessage
                        {
                            StatusCode = HttpStatusCode.OK,
                            Content = new StringContent("Fizz")
                        };
                        response.Headers.Add("Foo", "Bar");
                        return response;
                    }
                };
                IModule download = new Download().WithUris("https://wyam.io/");

                // When
                TestDocument result = await ExecuteAsync(document, context, download).SingleAsync();

                // Then
                Assert.IsNotNull(result.Source, "Source cannot be empty");

                Dictionary<string, string> headers = result[Keys.SourceHeaders] as Dictionary<string, string>;

                Assert.IsNotNull(headers, "Header cannot be null");
                Assert.IsTrue(headers.Count > 0, "Headers must contain contents");

                foreach (KeyValuePair<string, string> h in headers)
                {
                    Assert.IsNotEmpty(h.Key, "Header key cannot be empty");
                    Assert.IsNotEmpty(h.Value, "Header value cannot be empty");
                }

                result.Content.ShouldNotBeEmpty();
            }

            [Test]
            public async Task MultipleHtmlDownload()
            {
                // Given
                TestDocument document = new TestDocument();
                TestExecutionContext context = new TestExecutionContext
                {
                    HttpResponseFunc = (_, __) =>
                    {
                        HttpResponseMessage response = new HttpResponseMessage
                        {
                            StatusCode = HttpStatusCode.OK,
                            Content = new StringContent("Fizz")
                        };
                        response.Headers.Add("Foo", "Bar");
                        return response;
                    }
                };
                IModule download = new Download().WithUris("https://wyam.io/", "https://github.com/Wyamio/Wyam");

                // When
                IReadOnlyList<TestDocument> results = await ExecuteAsync(document, context, download);

                // Then
                foreach (TestDocument result in results)
                {
                    Dictionary<string, string> headers = result[Keys.SourceHeaders] as Dictionary<string, string>;

                    Assert.IsNotNull(headers, "Header cannot be null");
                    Assert.IsTrue(headers.Count > 0, "Headers must contain contents");

                    foreach (KeyValuePair<string, string> h in headers)
                    {
                        Assert.IsNotEmpty(h.Key, "Header key cannot be empty");
                        Assert.IsNotEmpty(h.Value, "Header value cannot be empty");
                    }

                    result.Content.ShouldNotBeEmpty();
                }
            }

            [Test]
            public async Task SingleImageDownload()
            {
                // Given
                TestDocument document = new TestDocument();
                TestExecutionContext context = new TestExecutionContext
                {
                    HttpResponseFunc = (_, __) =>
                    {
                        return new HttpResponseMessage
                        {
                            StatusCode = HttpStatusCode.OK,
                            Content = new ByteArrayContent(new byte[] { 0x01, 0x01 })
                        };
                    }
                };
                IModule download = new Download().WithUris("https://wyam.io/assets/img/logo.png");

                // When
                TestDocument result = await ExecuteAsync(document, context, download).SingleAsync();

                // Then
                using (Stream stream = await result.GetStreamAsync())
                {
                    stream.ReadByte().ShouldNotBe(-1);
                }
            }

            [Test]
            public async Task SingleImageDownloadWithRequestHeader()
            {
                // Given
                TestDocument document = new TestDocument();
                TestExecutionContext context = new TestExecutionContext
                {
                    HttpResponseFunc = (_, __) =>
                    {
                        return new HttpResponseMessage
                        {
                            StatusCode = HttpStatusCode.OK,
                            Content = new ByteArrayContent(new byte[] { 0x01, 0x01 })
                        };
                    }
                };
                RequestHeaders header = new RequestHeaders();
                header.Accept.Add("image/jpeg");
                IModule download = new Download().WithUri("https://wyam.io/assets/img/logo.png", header);

                // When
                TestDocument result = await ExecuteAsync(document, context, download).SingleAsync();

                // Then
                using (Stream stream = await result.GetStreamAsync())
                {
                    stream.ReadByte().ShouldNotBe(-1);
                }
            }
        }
    }
}
