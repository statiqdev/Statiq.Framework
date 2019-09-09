using NUnit.Framework;
using Shouldly;
using Statiq.Testing;

namespace Statiq.Common.Tests.Documents
{
    [TestFixture]
    public class IDocumentFixture : BaseFixture
    {
        public class GetParentTests : IDocumentFixture
        {
            [Test]
            public void GetsParent()
            {
                // Given
                IDocument a = new TestDocument("A");
                IDocument x = new TestDocument("X")
                {
                    {
                        Keys.Children,
                        new IDocument[]
                        {
                            a,
                            new TestDocument("B")
                        }
                    }
                };
                IDocument y = new TestDocument("Y");

                // When
                IDocument result = a.GetParent(new[] { x, y });

                // Then
                result.ShouldBe(x);
            }

            [Test]
            public void ReturnsNullWhenNoParent()
            {
                // Given
                IDocument a = new TestDocument("A");
                IDocument x = new TestDocument("X")
                {
                    {
                        Keys.Children,
                        new IDocument[]
                        {
                            new TestDocument("B")
                        }
                    }
                };
                IDocument y = new TestDocument("Y");

                // When
                IDocument result = a.GetParent(new[] { x, y });

                // Then
                result.ShouldBeNull();
            }

            [Test]
            public void GetsRecursiveParent()
            {
                // Given
                IDocument a = new TestDocument("A");
                IDocument x = new TestDocument("X")
                {
                    {
                        Keys.Children,
                        new IDocument[]
                        {
                            a,
                            new TestDocument("B")
                        }
                    }
                };
                IDocument y = new TestDocument("Y");
                IDocument z = new TestDocument("Z")
                {
                    {
                        Keys.Children,
                        new IDocument[]
                        {
                            new TestDocument("C"),
                            x
                        }
                    }
                };

                // When
                IDocument result = a.GetParent(new[] { y, z });

                // Then
                result.ShouldBe(x);
            }

            [Test]
            public void ReturnsNullForRecursiveParentWhenNotRecursive()
            {
                // Given
                IDocument a = new TestDocument("A");
                IDocument x = new TestDocument("X")
                {
                    {
                        Keys.Children,
                        new IDocument[]
                        {
                            a,
                            new TestDocument("B")
                        }
                    }
                };
                IDocument y = new TestDocument("Y");
                IDocument z = new TestDocument("Z")
                {
                    {
                        Keys.Children,
                        new IDocument[]
                        {
                            new TestDocument("C"),
                            x
                        }
                    }
                };

                // When
                IDocument result = a.GetParent(new[] { y, z }, false);

                // Then
                result.ShouldBeNull();
            }
        }
    }
}
