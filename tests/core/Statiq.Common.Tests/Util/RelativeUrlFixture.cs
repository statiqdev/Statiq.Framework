﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Statiq.Testing;

namespace Statiq.Common.Tests.Util
{
    [TestFixture]
    public class RelativeUrlFixture : BaseFixture
    {
        public class CreateRelativeUrlTests : RelativeUrlFixture
        {
            [TestCase(null, false)]
            [TestCase("", false)]
            [TestCase("~", false)]
            [TestCase("~foo", false)]
            [TestCase("foo", false)]
            [TestCase("/foo", false)]
            [TestCase("~/", true)]
            [TestCase("~/foo", true)]
            public void ShouldParseRoot(string url, bool expected)
            {
                // When
                RelativeUrl relativeUrl = new RelativeUrl(url);

                // Then
                Assert.AreEqual(expected, relativeUrl.HasRoot);
            }

            [TestCase(null, "")]
            [TestCase("", "")]
            [TestCase("#", "")]
            [TestCase("/#", "")]
            [TestCase("/?#", "")]
            [TestCase("~/#", "")]
            [TestCase("~#", "")]
            [TestCase("#", "")]
            [TestCase("#fragment", "fragment")]
            [TestCase("?#fragment", "fragment")]
            [TestCase("?a=b#fragment", "fragment")]
            [TestCase("/foo#fragment", "fragment")]
            [TestCase("/foo?#fragment", "fragment")]
            [TestCase("/foo?a=b#fragment", "fragment")]
            [TestCase("~#fragment", "fragment")]
            [TestCase("~/#fragment", "fragment")]
            [TestCase("~/foo#fragment", "fragment")]
            [TestCase("~/foo?#fragment", "fragment")]
            [TestCase("~/foo?a=b#fragment", "fragment")]
            public void ShouldParseFragment(string url, string expected)
            {
                // When
                RelativeUrl relativeUrl = new RelativeUrl(url);

                // Then
                Assert.AreEqual(expected, relativeUrl.Fragment);
            }

            [TestCase(null, "")]
            [TestCase("", "")]
            [TestCase("~", "")]
            [TestCase("~/", "")]
            [TestCase("/", "")]
            [TestCase("/foo", "")]
            [TestCase("/foo#", "")]
            [TestCase("/foo#fragment", "")]
            [TestCase("foo#fragment", "")]
            [TestCase("?", "")]
            [TestCase("/?", "")]
            [TestCase("~/?", "")]
            [TestCase("?query", "query")]
            [TestCase("~?query", "query")]
            [TestCase("~/?query", "query")]
            [TestCase("~/foo?query", "query")]
            [TestCase("~/foo?query#", "query")]
            [TestCase("~/foo?query#fragment", "query")]
            [TestCase("~/foo/bar?query#fragment", "query")]
            [TestCase("foo?query", "query")]
            [TestCase("foo/bar?query", "query")]
            [TestCase("foo/bar?query#", "query")]
            [TestCase("foo/bar?query#fragment", "query")]
            [TestCase("/foo?query", "query")]
            [TestCase("/foo/bar?query", "query")]
            [TestCase("/foo?query#", "query")]
            [TestCase("/foo/bar?query#", "query")]
            [TestCase("/foo/bar?query#fragment", "query")]
            [TestCase("/?query", "query")]
            [TestCase("foo/?query", "query")]
            [TestCase("/foo/?query", "query")]
            [TestCase("foo/bar/?query", "query")]
            [TestCase("/foo/bar/?query", "query")]
            public void ShouldParseQuery(string url, string expected)
            {
                // When
                RelativeUrl relativeUrl = new RelativeUrl(url);

                // Then
                Assert.AreEqual(expected, relativeUrl.Query);
            }

            [TestCase(null, null)]
            [TestCase("", "")]
            [TestCase("?", "")]
            [TestCase("#", "")]
            [TestCase("?#", "")]
            [TestCase("~", "~")]
            [TestCase("~/", "/")]
            [TestCase("/", "/")]
            [TestCase("foo", "foo")]
            [TestCase("~/foo", "/foo")]
            [TestCase("~/foo/", "/foo")]
            [TestCase("~/foo/bar", "/foo/bar")]
            [TestCase("~/foo/bar/", "/foo/bar")]
            [TestCase("foo?", "foo")]
            [TestCase("/foo?", "/foo")]
            [TestCase("foo/bar?", "foo/bar")]
            [TestCase("foo/bar/?", "foo/bar")]
            [TestCase("/foo/bar?", "/foo/bar")]
            [TestCase("foo#", "foo")]
            [TestCase("foo/#", "foo")]
            [TestCase("foo/bar#", "foo/bar")]
            [TestCase("foo/bar/#", "foo/bar")]
            [TestCase("/foo#", "/foo")]
            [TestCase("/foo/#", "/foo")]
            [TestCase("/foo/bar#", "/foo/bar")]
            [TestCase("/foo/bar/#", "/foo/bar")]
            [TestCase("foo?#", "foo")]
            [TestCase("/foo?query#", "/foo")]
            [TestCase("/foo?query#fragment", "/foo")]
            public void ShouldParsePath(string url, string expected)
            {
                // When
                RelativeUrl relativeUrl = new RelativeUrl(url);

                // Then
                Assert.AreEqual(expected, (string)relativeUrl.Path);
            }

            [TestCase("~", null, "~")]
            [TestCase("~/", null, "")]
            [TestCase("~", "root", "~")]
            [TestCase("~/", "root", "/root")]
            [TestCase("~/foo", "root", "/root/foo")]
            [TestCase("foo", "root", "foo")]
            [TestCase("/foo", "root", "/foo")]
            [TestCase("~/foo?a=b#fragment", "root", "/root/foo?a=b#fragment")]
            public void ShouldCreateUrl(string url, string root, string expected)
            {
                // When
                RelativeUrl relativeUrl = new RelativeUrl(url, root);

                // Then
                Assert.AreEqual(expected, relativeUrl.ToString());
            }
        }
    }
}
