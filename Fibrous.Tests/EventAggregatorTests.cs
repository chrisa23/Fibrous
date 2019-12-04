using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Fibrous.Tests
{
    [TestFixture]
    class EventAggregatorTests
    {
        private IEventAggregator eventAggregator = new EventAggregator();
        [Test]
        public async Task NonAsync()
        {
            var test = new Tester();
            using var sub = eventAggregator.Subscribe(test);
            eventAggregator.Publish(3);
            eventAggregator.Publish(2);
            eventAggregator.Publish("test");
            await Task.Delay(100);
            Assert.AreEqual(6, test.Count);
        }
        [Test]
        public async Task Async()
        {
            var test = new AsyncTester();
            using var sub = eventAggregator.Subscribe(test);
            eventAggregator.Publish(3);
            eventAggregator.Publish(2);
            eventAggregator.Publish("test");
            await Task.Delay(100);
            Assert.AreEqual(6, test.Count);
        }
        public class Tester : IHandle<string>, IHandle<int>, IDisposable
        {
            public IFiber Fiber { get; } = Fibrous.Fiber.StartNew();
            
            public int Count { get; private set; }

            public void Handle(int message)
            {
                Count += message;
            }

            public void Handle(string message)
            {
                Count += 1;
            }

            public void Dispose()
            {
                Fiber?.Dispose();
            }
        }

        public class AsyncTester : IHandleAsync<string>, IHandleAsync<int>, IDisposable
        {
            public IAsyncFiber Fiber { get; } = AsyncFiber.StartNew();

            public int Count { get; private set; }

            public Task Handle(int message)
            {
                Count += message;
                return Task.CompletedTask;
            }

            public Task Handle(string message)
            {
                Count += 1;
                return Task.CompletedTask;
            }

            public void Dispose()
            {
                Fiber?.Dispose();
            }
        }
    }
}
