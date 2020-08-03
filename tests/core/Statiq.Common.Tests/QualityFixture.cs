using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Shouldly;

namespace Statiq.Common.Tests
{
    [TestFixture]
    public class QualityFixture
    {
        [Test]
        public void FlatNamespace()
        {
            // Given, When
            string[] namespaces = typeof(IExecutionContext).Assembly.GetTypes().Select(x => x.Namespace).Distinct().Where(x => x is object).ToArray();

            // Then
            namespaces.ShouldBe(new[] { "Statiq.Common" });
        }
    }
}
