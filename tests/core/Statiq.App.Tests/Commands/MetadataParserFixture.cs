using System;
using System.Collections.Generic;
using NUnit.Framework;
using Statiq.Testing;

namespace Statiq.App.Tests.Commands
{
    [TestFixture]
    public class MetadataParserFixture : BaseFixture
    {
        public class ParseTests : MetadataParserFixture
        {
            [Test]
            public void TestKeyOnlyParse()
            {
                // Given
                string[] expected = { "hi", "=hello", "\\=abcd", "key\\=val", "     bjorn  \\=   dad" };

                // When
                IReadOnlyDictionary<string, string> args = MetadataParser.Parse(expected);

                // Then
                Assert.AreEqual(expected.Length, args.Count);
                int i = 0;
                foreach (KeyValuePair<string, string> arg in args)
                {
                    Assert.AreEqual(expected[i].Replace("\\=", "=").Trim(), arg.Key);
                    Assert.IsNull(arg.Value);
                    i++;
                }
            }

            [Test]
            public void TestKeyValueParse()
            {
                // Given
                string[] pairs = { "key=value", "k=v", "except=bro", "awesome====123123", "   keytrimmed    =    value trimmed   " };

                // When
                IReadOnlyDictionary<string, string> args = MetadataParser.Parse(pairs);

                // Then
                Assert.AreEqual(pairs.Length, args.Count);
                foreach (KeyValuePair<string, string> arg in args)
                {
                    Assert.NotNull(arg.Value, "Argument value should not be null.");
                    StringAssert.DoesNotStartWith(" ", arg.Key, "Arguments key should be trimmed.");
                    StringAssert.DoesNotEndWith(" ", arg.Key, "Arguments key should be trimmed.");
                    StringAssert.DoesNotStartWith(" ", (string)arg.Value, "Arguments value should be trimmed.");
                    StringAssert.DoesNotEndWith(" ", (string)arg.Value, "Arguments value should be trimmed.");
                }
            }

            /// <summary>
            /// Same keys are not valid.
            /// </summary>
            [Test]
            public void TestMetadataKeyCollision()
            {
                // Given, When, Then
                Assert.Throws<ArgumentException>(
                    () => MetadataParser.Parse(new[] { "hello=world", "hello=exception" }));
            }
        }
    }
}
