using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Wyam.Common.Execution;
using Wyam.Common.Modules;
using Wyam.Core.Execution;
using Wyam.Core.Modules.Control;
using Wyam.Core.Modules.IO;
using Wyam.Testing;
using Wyam.Testing.Modules;

namespace Wyam.Common.Tests.Modules
{
    [TestFixture]
    [Parallelizable(ParallelScope.Self | ParallelScope.Children)]
    public class IModuleListExtensionsFixture : BaseFixture
    {
        public class InsertAfterFirstTests : IModuleListExtensionsFixture
        {
            [Test]
            public void InsertAfterFirst()
            {
                // Given
                IPipeline collection = new ExecutionPipeline("Test", new IModule[]
                {
                    new ReadFiles("*.md"),
                    new ReadFiles("*.md"),
                    new WriteFiles()
                });

                // When
                collection.InsertAfterFirst<ReadFiles>(new CountModule("foo"));

                // Then
                Assert.AreEqual(collection[0].GetType(), typeof(ReadFiles));
                Assert.AreEqual(collection[1].GetType(), typeof(CountModule));
                Assert.AreEqual(collection[2].GetType(), typeof(ReadFiles));
                Assert.AreEqual(collection[3].GetType(), typeof(WriteFiles));
            }
        }

        public class InsertBeforeFirstTests : IModuleListExtensionsFixture
        {
            [Test]
            public void InsertBeforeFirst()
            {
                // Given
                IPipeline collection = new ExecutionPipeline("Test", new IModule[]
                {
                    new ReadFiles("*.md"),
                    new ReadFiles("*.md"),
                    new WriteFiles()
                });

                // When
                collection.InsertBeforeFirst<ReadFiles>(new CountModule("foo"));

                // Then
                Assert.AreEqual(collection[0].GetType(), typeof(CountModule));
                Assert.AreEqual(collection[1].GetType(), typeof(ReadFiles));
                Assert.AreEqual(collection[2].GetType(), typeof(ReadFiles));
                Assert.AreEqual(collection[3].GetType(), typeof(WriteFiles));
            }
        }

        public class InsertAfterLastTests : IModuleListExtensionsFixture
        {
            [Test]
            public void InsertAfterLast()
            {
                // Given
                IPipeline collection = new ExecutionPipeline("Test", new IModule[]
                {
                    new ReadFiles("*.md"),
                    new ReadFiles("*.md"),
                    new WriteFiles()
                });

                // When
                collection.InsertAfterLast<ReadFiles>(new CountModule("foo"));

                // Then
                Assert.AreEqual(collection[0].GetType(), typeof(ReadFiles));
                Assert.AreEqual(collection[1].GetType(), typeof(ReadFiles));
                Assert.AreEqual(collection[2].GetType(), typeof(CountModule));
                Assert.AreEqual(collection[3].GetType(), typeof(WriteFiles));
            }
        }

        public class InsertBeforeLastTests : IModuleListExtensionsFixture
        {
            [Test]
            public void InsertBeforeLast()
            {
                // Given
                IPipeline collection = new ExecutionPipeline("Test", new IModule[]
                {
                    new ReadFiles("*.md"),
                    new ReadFiles("*.md"),
                    new WriteFiles()
                });

                // When
                collection.InsertBeforeLast<ReadFiles>(new CountModule("foo"));

                // Then
                Assert.AreEqual(collection[0].GetType(), typeof(ReadFiles));
                Assert.AreEqual(collection[1].GetType(), typeof(CountModule));
                Assert.AreEqual(collection[2].GetType(), typeof(ReadFiles));
                Assert.AreEqual(collection[3].GetType(), typeof(WriteFiles));
            }
        }

        public class ReplaceFirstTests : IModuleListExtensionsFixture
        {
            [Test]
            public void ReplaceFirst()
            {
                // Given
                IPipeline collection = new ExecutionPipeline("Test", new IModule[]
                {
                    new ReadFiles("*.md"),
                    new CountModule("mykey1"),
                    new CountModule("mykey2"),
                    new WriteFiles()
                });

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
                IPipeline collection = new ExecutionPipeline("Test", new IModule[]
                {
                    new ReadFiles("*.md"),
                    new CountModule("mykey1"),
                    new CountModule("mykey2"),
                    new WriteFiles()
                });

                // When
                collection.ReplaceLast<CountModule>(new CountModule("replacedKey"));

                // Then
                Assert.AreEqual("mykey1", ((CountModule)collection[1]).ValueKey);
                Assert.AreEqual("replacedKey", ((CountModule)collection[2]).ValueKey);
            }
        }
    }
}
