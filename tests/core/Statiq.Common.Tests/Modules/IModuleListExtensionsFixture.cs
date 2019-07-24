using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using Statiq.Testing;

namespace Statiq.Common.Tests.Modules
{
    [TestFixture]
    public class IModuleListExtensionsFixture : BaseFixture
    {
        public class InsertAfterFirstTests : IModuleListExtensionsFixture
        {
            [Test]
            public void InsertAfterFirst()
            {
                // Given
                IModuleList collection = new ModuleList(
                    new RedModule(),
                    new RedModule(),
                    new GreenModule());

                // When
                collection.InsertAfterFirst<RedModule>(new CountModule("foo"));

                // Then
                Assert.AreEqual(collection[0].GetType(), typeof(RedModule));
                Assert.AreEqual(collection[1].GetType(), typeof(CountModule));
                Assert.AreEqual(collection[2].GetType(), typeof(RedModule));
                Assert.AreEqual(collection[3].GetType(), typeof(GreenModule));
            }
        }

        public class InsertBeforeFirstTests : IModuleListExtensionsFixture
        {
            [Test]
            public void InsertBeforeFirst()
            {
                // Given
                IModuleList collection = new ModuleList(
                    new RedModule(),
                    new RedModule(),
                    new GreenModule());

                // When
                collection.InsertBeforeFirst<RedModule>(new CountModule("foo"));

                // Then
                Assert.AreEqual(collection[0].GetType(), typeof(CountModule));
                Assert.AreEqual(collection[1].GetType(), typeof(RedModule));
                Assert.AreEqual(collection[2].GetType(), typeof(RedModule));
                Assert.AreEqual(collection[3].GetType(), typeof(GreenModule));
            }
        }

        public class InsertAfterLastTests : IModuleListExtensionsFixture
        {
            [Test]
            public void InsertAfterLast()
            {
                // Given
                IModuleList collection = new ModuleList(
                    new RedModule(),
                    new RedModule(),
                    new GreenModule());

                // When
                collection.InsertAfterLast<RedModule>(new CountModule("foo"));

                // Then
                Assert.AreEqual(collection[0].GetType(), typeof(RedModule));
                Assert.AreEqual(collection[1].GetType(), typeof(RedModule));
                Assert.AreEqual(collection[2].GetType(), typeof(CountModule));
                Assert.AreEqual(collection[3].GetType(), typeof(GreenModule));
            }
        }

        public class InsertBeforeLastTests : IModuleListExtensionsFixture
        {
            [Test]
            public void InsertBeforeLast()
            {
                // Given
                IModuleList collection = new ModuleList(
                    new RedModule(),
                    new RedModule(),
                    new GreenModule());

                // When
                collection.InsertBeforeLast<RedModule>(new CountModule("foo"));

                // Then
                Assert.AreEqual(collection[0].GetType(), typeof(RedModule));
                Assert.AreEqual(collection[1].GetType(), typeof(CountModule));
                Assert.AreEqual(collection[2].GetType(), typeof(RedModule));
                Assert.AreEqual(collection[3].GetType(), typeof(GreenModule));
            }
        }

        public class ReplaceFirstTests : IModuleListExtensionsFixture
        {
            [Test]
            public void ReplaceFirst()
            {
                // Given
                IModuleList collection = new ModuleList(
                    new RedModule(),
                    new CountModule("mykey1"),
                    new CountModule("mykey2"),
                    new GreenModule());

                // When
                collection.ReplaceFirst<CountModule>(new CountModule("replacedKey"));

                // Then
                Assert.AreEqual("replacedKey", ((CountModule)collection[1]).ValueKey);
                Assert.AreEqual("mykey2", ((CountModule)collection[2]).ValueKey);
            }
        }

        public class ReplaceLastTests : IModuleListExtensionsFixture
        {
            [Test]
            public void ReplaceLast()
            {
                // Given
                IModuleList collection = new ModuleList(
                    new RedModule(),
                    new CountModule("mykey1"),
                    new CountModule("mykey2"),
                    new GreenModule());

                // When
                collection.ReplaceLast<CountModule>(new CountModule("replacedKey"));

                // Then
                Assert.AreEqual("mykey1", ((CountModule)collection[1]).ValueKey);
                Assert.AreEqual("replacedKey", ((CountModule)collection[2]).ValueKey);
            }
        }

        private class RedModule : IModule
        {
            public Task<IEnumerable<IDocument>> ExecuteAsync(IExecutionContext context) => throw new NotImplementedException();
        }

        private class GreenModule : IModule
        {
            public Task<IEnumerable<IDocument>> ExecuteAsync(IExecutionContext context) => throw new NotImplementedException();
        }
    }
}
