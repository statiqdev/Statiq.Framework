using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using Shouldly;
using Statiq.Common;
using Statiq.Testing;

namespace Statiq.Core.Tests.Events
{
    [TestFixture]
    public class EventCollectionFixture : BaseFixture
    {
        public class RaiseTests : EventCollectionFixture
        {
            [Test]
            public async Task SubscribesAsyncEvent()
            {
                // Given
                IEventCollection eventCollection = new EventCollection();
                eventCollection.Subscribe<TestEventArgs>(async args =>
                {
                    args.Foo = 4;
                    await Task.CompletedTask;
                });
                TestEventArgs args = new TestEventArgs { Foo = 1 };

                // When
                await eventCollection.RaiseAsync(args);

                // Then
                args.Foo.ShouldBe(4);
            }

            [Test]
            public async Task SubscribesSyncEvent()
            {
                // Given
                IEventCollection eventCollection = new EventCollection();
                eventCollection.Subscribe<TestEventArgs>(args =>
                {
                    args.Foo = 4;
                });
                TestEventArgs args = new TestEventArgs { Foo = 1 };

                // When
                await eventCollection.RaiseAsync(args);

                // Then
                args.Foo.ShouldBe(4);
            }

            [Test]
            public async Task SubscribesMultipleEvents()
            {
                // Given
                IEventCollection eventCollection = new EventCollection();
                eventCollection.Subscribe<TestEventArgs>(async args =>
                {
                    args.Foo++;
                    await Task.CompletedTask;
                });
                eventCollection.Subscribe<TestEventArgs>(async args =>
                {
                    args.Foo++;
                    await Task.CompletedTask;
                });
                eventCollection.Subscribe<TestEventArgs>(args =>
                {
                    args.Foo++;
                });
                TestEventArgs args = new TestEventArgs { Foo = 3 };

                // When
                await eventCollection.RaiseAsync(args);

                // Then
                args.Foo.ShouldBe(6);
            }
        }

        public class TestEventArgs
        {
            public int Foo { get; set; }
        }
    }
}
