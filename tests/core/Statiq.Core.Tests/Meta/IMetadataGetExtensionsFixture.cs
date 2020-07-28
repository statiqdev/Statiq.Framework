using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using Shouldly;
using Statiq.Common;
using Statiq.Testing;

namespace Statiq.Core.Tests.Meta
{
    // These tests exist in Statiq.Core so they have access to the ScriptHelper implementation
    [TestFixture]
    public class IMetadataGetExtensionsFixture : BaseFixture
    {
        public class TryGetValueTests : IMetadataGetExtensionsFixture
        {
            [Test]
            public void EvaluatesSimpleScript()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                context.ScriptHelper = new ScriptHelper(context);
                TestMetadata metadata = new TestMetadata();

                // When
                bool result = metadata.TryGetValue("=> 1 + 2", out int value);

                // Then
                result.ShouldBeTrue();
                value.ShouldBe(3);
            }

            [Test]
            public void EvaluatesSimpleScriptWhenConversionFails()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                context.ScriptHelper = new ScriptHelper(context);
                TestMetadata metadata = new TestMetadata();

                // When
                bool result = metadata.TryGetValue("=> 1 + 2", out TryGetValueTests value);

                // Then
                result.ShouldBeFalse();
                value.ShouldBeNull();
            }

            [Test]
            public void EvaluatesScriptWithMetadataAccess()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                context.ScriptHelper = new ScriptHelper(context);
                TestMetadata metadata = new TestMetadata()
                {
                    { "Foo", 3 }
                };

                // When
                bool result = metadata.TryGetValue("=> 1 + GetInt(\"Foo\")", out int value);

                // Then
                result.ShouldBeTrue();
                value.ShouldBe(4);
            }
        }
    }
}
