using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using Shouldly;
using Statiq.Testing;

namespace Statiq.Common.Tests.Scripting
{
    [TestFixture]
    public class IScriptHelperTestFixture : BaseFixture
    {
        public class TryGetScriptStringTests : IScriptHelperTestFixture
        {
            [TestCase("foo", false, null)]
            [TestCase("=foo", false, null)]
            [TestCase("=>foo", true, "foo")]
            [TestCase(" =foo", false, null)]
            [TestCase(" =>foo", true, "foo")]
            [TestCase("= >foo", false, null)]
            [TestCase("=> foo", true, " foo")]
            [TestCase(" => foo", true, " foo")]
            [TestCase("bar=>foo", false, null)]
            [TestCase("  => foo ", true, " foo ")]
            [TestCase(" = > foo", false, null)]
            public void GetsScriptString(string input, bool expected, string expectedScript)
            {
                // Given, When
                bool result = IScriptHelper.TryGetScriptString(input, out string resultScript);

                // Then
                result.ShouldBe(expected);
                resultScript.ShouldBe(expectedScript);
            }
        }
    }
}
