using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Shouldly;

namespace Statiq.App.Tests
{
    [TestFixture]
    public class QualityFixture
    {
        [Test]
        public void FlatNamespace()
        {
            // Given, When
            string[] namespaces = typeof(PipelineBuilder).Assembly.GetTypes().Select(x => x.Namespace).Distinct().Where(x => x is object).ToArray();

            // Then
            namespaces.ShouldBe(new[] { "Statiq.App" });
        }
    }
}
