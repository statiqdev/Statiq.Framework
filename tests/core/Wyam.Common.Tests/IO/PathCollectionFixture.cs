using System;
using NUnit.Framework;
using Wyam.Common.IO;
using Wyam.Testing;

namespace Wyam.Common.Tests.IO
{
    [TestFixture(typeof(DirectoryPath))]
    [TestFixture(typeof(FilePath))]
    [Parallelizable(ParallelScope.Self | ParallelScope.Children)]
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
            if (typeof(TPath) == typeof(DirectoryPath))
            {
                _upperCaseA = (TPath)(NormalizedPath)new DirectoryPath("A");
                _lowerCaseA = (TPath)(NormalizedPath)new DirectoryPath("a");
                _upperCaseB = (TPath)(NormalizedPath)new DirectoryPath("B");
                _lowerCaseB = (TPath)(NormalizedPath)new DirectoryPath("b");
                _upperCaseC = (TPath)(NormalizedPath)new DirectoryPath("C");
                _lowerCaseC = (TPath)(NormalizedPath)new DirectoryPath("c");
            }
            else if (typeof(TPath) == typeof(FilePath))
            {
                _upperCaseA = (TPath)(NormalizedPath)new FilePath("A.txt");
                _lowerCaseA = (TPath)(NormalizedPath)new FilePath("a.txt");
                _upperCaseB = (TPath)(NormalizedPath)new FilePath("B.txt");
                _lowerCaseB = (TPath)(NormalizedPath)new FilePath("b.txt");
                _upperCaseC = (TPath)(NormalizedPath)new FilePath("C.txt");
                _lowerCaseC = (TPath)(NormalizedPath)new FilePath("c.txt");
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
