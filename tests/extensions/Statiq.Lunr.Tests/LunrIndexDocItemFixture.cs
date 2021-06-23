using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Shouldly;
using Statiq.Common;
using Statiq.Testing;

namespace Statiq.Lunr.Tests
{
    [TestFixture]
    public class LunrIndexDocItemFixture : BaseFixture
    {
        public class ConstructorTests : LunrIndexDocItemFixture
        {
            [Test]
            public void ThrowsForNullDocument()
            {
                // Given, When, Then
                Should.Throw<ArgumentNullException>(() => new LunrIndexDocItem(null));
            }

            [Test]
            public void ThrowsForNullDocumentAlternate()
            {
                // Given, When, Then
                Should.Throw<ArgumentNullException>(() => new LunrIndexDocItem(null, "Foo", "Bar"));
            }
        }

        public class TitleTests : LunrIndexDocItemFixture
        {
            [Test]
            public void GetsTitleFromDocument()
            {
                // Given
                TestDocument document = new TestDocument
                {
                    { Keys.Title, "Foo" }
                };
                LunrIndexDocItem item = new LunrIndexDocItem(document);

                // When
                string title = item.Title;

                // Then
                title.ShouldBe("Foo");
            }

            [Test]
            public void ConstructorOverridesDocumentTitle()
            {
                // Given
                TestDocument document = new TestDocument
                {
                    { Keys.Title, "Foo" }
                };
                LunrIndexDocItem item = new LunrIndexDocItem(document, "Bar", "Bazz");

                // When
                string title = item.Title;

                // Then
                title.ShouldBe("Bar");
            }
        }

        public class ContentTests : LunrIndexDocItemFixture
        {
            [Test]
            public async Task GetsContentFromDocument()
            {
                // Given
                TestDocument document = new TestDocument("Fizz");
                LunrIndexDocItem item = new LunrIndexDocItem(document);

                // When
                string content = await item.GetContentAsync();

                // Then
                content.ShouldBe("Fizz");
            }

            [Test]
            public async Task ConstructorOverridesDocumentContent()
            {
                // Given
                TestDocument document = new TestDocument("Fizz");
                LunrIndexDocItem item = new LunrIndexDocItem(document, "Bar", "Bazz");

                // When
                string content = await item.GetContentAsync();

                // Then
                content.ShouldBe("Bazz");
            }
        }
    }
}
