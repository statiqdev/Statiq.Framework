using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Shouldly;
using Statiq.Common;
using Statiq.Testing;

namespace Statiq.Html.Tests
{
    [TestFixture]
    public class GlobalizationFixture : BaseFixture
    {
        [Test]
        public async Task AddProperDirectionAttributesToRtlText()
        {
            // Given
            TestDocument document = new TestDocument(@"
<h1>Hello World <span>سڵاو جیهان</span> Hello World</h1>
<p>This is very nice.</p>
<div>
  <p>اهلا و سهلا</p>
  <p>This is so cool</p>
</div>

<ul>
<li>بەڵێ</li>
<li>Yes</li>
</ul>");
            Globalization module = new Globalization();

            // When
            TestDocument result = await ExecuteAsync(document, module).SingleAsync();

            string expected = @"<html><head></head><body><h1>Hello World <span>سڵاو جیهان</span> Hello World</h1>
<p>This is very nice.</p>
<div dir='rtl'>
  <p>اهلا و سهلا</p>
  <p dir='ltr'>This is so cool</p>
</div>

<ul dir='rtl'>
<li>بەڵێ</li>
<li>Yes</li>
</ul></body></html>".Replace("'", "\"");

            // Then
            result.Content.ShouldBe(expected, StringCompareShould.IgnoreLineEndings);
        }
    }
}
