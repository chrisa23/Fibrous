using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Fibrous.Tests
{
    [TestFixture]
    public class EventHubTests
    {
        private readonly IEventHub _eventHub = new EventHub();
        [Test]
        public async Task NonAsync()
        {
            using var test = new Tester();
            using var sub = _eventHub.Subscribe(test);
            
            _eventHub.Publish(3);
            _eventHub.Publish(2);
            _eventHub.Publish("test");
            
            await Task.Delay(100);
            
            Assert.AreEqual(6, test.Count);
        }

        [Test]
        public async Task Async()
        {
            using var test = new AsyncTester();
            using var sub = _eventHub.Subscribe(test);
            
            _eventHub.Publish(3);
            _eventHub.Publish(2);
            _eventHub.Publish("test");
            
            await Task.Delay(100);
            
            Assert.AreEqual(6, test.Count);
        }

        public class Tester : IHandle<string>, IHandle<int>, IHaveFiber, IDisposable
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

        public class AsyncTester : IHandleAsync<string>, IHandleAsync<int>, IHaveAsyncFiber, IDisposable
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
