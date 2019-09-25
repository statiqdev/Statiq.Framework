using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using Shouldly;
using Statiq.Common;
using Statiq.Testing;

namespace Statiq.Core.Tests.Configuration
{
    [TestFixture]
    public class SettingsFixture : BaseFixture
    {
        public class CountTests : SettingsFixture
        {
            [Test]
            public void EnsureCountIsNotRecursive()
            {
                // Given
                ISettings settings = new Settings();
                settings.Add("Foo", "Bar");

                // When
                int count = settings.Count;

                // Then
                count.ShouldBe(1); // Includes the default settings
            }
        }
    }
}
