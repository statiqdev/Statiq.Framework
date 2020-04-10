using System.Collections.Generic;
using NUnit.Framework;
using Shouldly;
using Statiq.Testing;

namespace Statiq.Common.Tests.Documents
{
    [TestFixture]
    public class ObjectDocumentExtensionsFixture : BaseFixture
    {
        public class ToDocumentTests : ObjectDocumentExtensionsFixture
        {
            [Test]
            public void GetsProperty()
            {
                // Given
                CustomObject obj = new CustomObject
                {
                    Foo = "abc"
                };
                IDocument document = obj.ToDocument();

                // When
                object result = document["Foo"];

                // Then
                result.ShouldBe("abc");
            }

            [Test]
            public void GetsPropertyFromObjectType()
            {
                // Given
                CustomObject obj = new CustomObject
                {
                    Foo = "abc"
                };
                IDocument document = ((object)obj).ToDocument();

                // When
                object result = document["Foo"];

                // Then
                result.ShouldBe("abc");
            }
        }

        private class CustomObject
        {
            public string Foo { get; set; }
        }
    }
}
