using System;
using NUnit.Framework;
using Statiq.Testing;

namespace Statiq.Common.Tests.IO
{
    [TestFixture(typeof(NormalizedPath))]
    [TestFixture(typeof(NormalizedPath))]
    public class PathCollectionFixture<TPath> : BaseFixture
        where TPath : NormalizedPath
    {
        private readonly TPath _upperCaseA;
        private readonly TPath _lowerCaseA;
        private readonly TPath _upperCaseB;
        private readonly TPath _lowerCaseB;
        private readonly TPath _upperCaseC;
        private readonly TPath _lowerCaseC;

        public PathCollectionFixture()
        {
            if (typeof(TPath) == typeof(NormalizedPath))
            {
                _upperCaseA = (TPath)(NormalizedPath)new NormalizedPath("A");
                _lowerCaseA = (TPath)(NormalizedPath)new NormalizedPath("a");
                _upperCaseB = (TPath)(NormalizedPath)new NormalizedPath("B");
                _lowerCaseB = (TPath)(NormalizedPath)new NormalizedPath("b");
                _upperCaseC = (TPath)(NormalizedPath)new NormalizedPath("C");
                _lowerCaseC = (TPath)(NormalizedPath)new NormalizedPath("c");
            }
            else if (typeof(TPath) == typeof(NormalizedPath))
            {
                _upperCaseA = (TPath)(NormalizedPath)new NormalizedPath("A.txt");
                _lowerCaseA = (TPath)(NormalizedPath)new NormalizedPath("a.txt");
                _upperCaseB = (TPath)(NormalizedPath)new NormalizedPath("B.txt");
                _lowerCaseB = (TPath)(NormalizedPath)new NormalizedPath("b.txt");
                _upperCaseC = (TPath)(NormalizedPath)new NormalizedPath("C.txt");
                _lowerCaseC = (TPath)(NormalizedPath)new NormalizedPath("c.txt");
            }
            else
            {
                throw new InvalidOperationException("Need to specify test paths for generic type");
            }
        }

        public class CountTests : PathCollectionFixture<TPath>
        {
            [Test]
            public void ShouldReturnTheNumberOfPathsInTheCollection()
            {
                // Given
                PathCollection<TPath> collection = new PathCollection<TPath>(new[] { _upperCaseA, _upperCaseB });

                // When, Then
                Assert.AreEqual(2, collection.Count);
            }
        }

        public class AddTests : PathCollectionFixture<TPath>
        {
            [Test]
            public void ShouldAddPathIfNotAlreadyPresent()
            {
                // Given
                PathCollection<TPath> collection = new PathCollection<TPath>();
                collection.Add(_upperCaseB);

                // When
                collection.Add(_upperCaseA);

                // Then
                Assert.AreEqual(2, collection.Count);
            }
        }

        public class AddRangeTests : PathCollectionFixture<TPath>
        {
            [Test]
            public void ShouldAddPathsThatAreNotPresent()
            {
                // Given
                PathCollection<TPath> collection = new PathCollection<TPath>(
                    new[] { _upperCaseA, _upperCaseB });

                // When
                collection.AddRange(new[] { _upperCaseA, _upperCaseB, _upperCaseC });

                // Then
                Assert.AreEqual(3, collection.Count);
            }
        }
    }
}
