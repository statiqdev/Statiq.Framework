using System.Collections.Generic;
using NUnit.Framework;
using Shouldly;
using Statiq.Common.Meta;
using Statiq.Common.Shortcodes;
using Statiq.Testing;
using Statiq.Testing.Execution;

namespace Statiq.Common.Tests.Shortcodes
{
    [TestFixture]
    public class ShortcodeExtensionsFixture : BaseFixture
    {
        public class ToDictionaryTests : ShortcodeExtensionsFixture
        {
            [Test]
            public void MatchesInCorrectOrder()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                KeyValuePair<string, string>[] args = new KeyValuePair<string, string>[]
                {
                    new KeyValuePair<string, string>(null, "1"),
                    new KeyValuePair<string, string>(null, "2"),
                    new KeyValuePair<string, string>(null, "3")
                };

                // When
                ConvertingDictionary dictionary = args.ToDictionary(context, "A", "B", "C");

                // Then
                dictionary.ShouldBe(
                    new KeyValuePair<string, object>[]
                    {
                        new KeyValuePair<string, object>("A", "1"),
                        new KeyValuePair<string, object>("B", "2"),
                        new KeyValuePair<string, object>("C", "3")
                    },
                    true);
            }

            [Test]
            public void MatchesNamedArguments()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                KeyValuePair<string, string>[] args = new KeyValuePair<string, string>[]
                {
                    new KeyValuePair<string, string>("B", "2"),
                    new KeyValuePair<string, string>("A", "1"),
                    new KeyValuePair<string, string>("C", "3")
                };

                // When
                ConvertingDictionary dictionary = args.ToDictionary(context, "A", "B", "C");

                // Then
                dictionary.ShouldBe(
                    new KeyValuePair<string, object>[]
                    {
                        new KeyValuePair<string, object>("A", "1"),
                        new KeyValuePair<string, object>("B", "2"),
                        new KeyValuePair<string, object>("C", "3")
                    },
                    true);
            }

            [Test]
            public void MatchesPositionalAndNamedArguments()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                KeyValuePair<string, string>[] args = new KeyValuePair<string, string>[]
                {
                    new KeyValuePair<string, string>(null, "1"),
                    new KeyValuePair<string, string>("C", "3"),
                    new KeyValuePair<string, string>("B", "2")
                };

                // When
                ConvertingDictionary dictionary = args.ToDictionary(context, "A", "B", "C");

                // Then
                dictionary.ShouldBe(
                    new KeyValuePair<string, object>[]
                    {
                        new KeyValuePair<string, object>("A", "1"),
                        new KeyValuePair<string, object>("B", "2"),
                        new KeyValuePair<string, object>("C", "3")
                    },
                    true);
            }

            [Test]
            public void ThrowsForPositionalAfterNamedArguments()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                KeyValuePair<string, string>[] args = new KeyValuePair<string, string>[]
                {
                    new KeyValuePair<string, string>(null, "1"),
                    new KeyValuePair<string, string>("C", "3"),
                    new KeyValuePair<string, string>(null, "2")
                };

                // When, Then
                Should.Throw<ShortcodeArgumentException>(() => args.ToDictionary(context, "A", "B", "C"));
            }

            [Test]
            public void ThrowsForDuplicateNamedArguments()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                KeyValuePair<string, string>[] args = new KeyValuePair<string, string>[]
                {
                    new KeyValuePair<string, string>(null, "1"),
                    new KeyValuePair<string, string>("C", "3"),
                    new KeyValuePair<string, string>("A", "2")
                };

                // When, Then
                Should.Throw<ShortcodeArgumentException>(() => args.ToDictionary(context, "A", "B", "C"));
            }
        }
    }
}
