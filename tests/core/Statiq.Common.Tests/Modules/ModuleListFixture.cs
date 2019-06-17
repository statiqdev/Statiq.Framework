using NUnit.Framework;
using Shouldly;
using Statiq.Common.Modules;
using Statiq.Testing;
using Statiq.Testing.Modules;

namespace Statiq.Common.Tests.Modules
{
    [TestFixture]
    [Parallelizable(ParallelScope.Self | ParallelScope.Children)]
    public class ModuleListFixture : BaseFixture
    {
        public class AddTests : ModuleListFixture
        {
            [Test]
            public void AddModule()
            {
                // Given
                ModuleList list = new ModuleList();
                CountModule count = new CountModule("A");

                // When
                list.Add(count);

                // Then
                list.ShouldBe(new[] { count });
            }
        }

        public class ContainsTests : ModuleListFixture
        {
            [Test]
            public void ContainsModule()
            {
                // Given
                ModuleList list = new ModuleList();
                CountModule count = new CountModule("A");

                // When
                list.Add(count);

                // Then
                Assert.That(list.Contains(count), Is.True);
            }

            [Test]
            public void DoesNotContainModule()
            {
                // Given
                ModuleList list = new ModuleList();
                CountModule count = new CountModule("A");
                CountModule count2 = new CountModule("B");

                // When
                list.Add(count);

                // Then
                Assert.That(list.Contains(count2), Is.False);
            }
        }

        public class RemoveTests : ModuleListFixture
        {
            [Test]
            public void RemovesModule()
            {
                // Given
                CountModule count = new CountModule("A");
                CountModule count2 = new CountModule("B");
                ModuleList list = new ModuleList(count, count2);

                // When
                bool result = list.Remove(count);

                // Then
                Assert.That(result, Is.True);
                Assert.That(list, Is.EqualTo(new[] { count2 }));
            }

            [Test]
            public void ReturnsFalseWhenNotRemoved()
            {
                // Given
                CountModule count = new CountModule("A");
                CountModule count2 = new CountModule("B");
                ModuleList list = new ModuleList(count2);

                // When
                bool result = list.Remove(count);

                // Then
                Assert.That(result, Is.False);
                Assert.That(list, Is.EqualTo(new[] { count2 }));
            }
        }

        public class IndexOfTests : ModuleListFixture
        {
            [Test]
            public void ReturnsIndex()
            {
                // Given
                CountModule count = new CountModule("A");
                CountModule count2 = new CountModule("B");
                ModuleList list = new ModuleList(count, count2);

                // When
                int index = list.IndexOf(count2);

                // Then
                Assert.That(index, Is.EqualTo(1));
            }

            [Test]
            public void ReturnsNegativeIndexWhenNotFound()
            {
                // Given
                CountModule count = new CountModule("A");
                CountModule count2 = new CountModule("B");
                ModuleList list = new ModuleList(count);

                // When
                int index = list.IndexOf(count2);

                // Then
                Assert.That(index, Is.LessThan(0));
            }
        }

        public class InsertTests : ModuleListFixture
        {
            [Test]
            public void InsertsModule()
            {
                // Given
                CountModule count = new CountModule("A");
                CountModule count2 = new CountModule("B");
                ModuleList list = new ModuleList(count2);

                // When
                list.Insert(0, count);

                // Then
                Assert.That(list, Is.EqualTo(new[] { count, count2 }));
            }
        }
    }
}
