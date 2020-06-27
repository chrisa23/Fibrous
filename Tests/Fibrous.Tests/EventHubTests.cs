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
        private readonly EventHub _eventHub = new EventHub();
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


        [Test]
        public Task AsyncUnsubscribeCheck()
        {
            using var test = new AsyncTester();
            var sub = _eventHub.Subscribe(test);

            _eventHub.Publish(3);

            sub.Dispose();
            Assert.IsFalse(_eventHub.HasSubscriptions<int>());
            return Task.CompletedTask;
        }

        [Test]
        public void UnsubscribeCheck()
        {
            using var test = new Tester();
            var sub = _eventHub.Subscribe(test);

            _eventHub.Publish(3);

            sub.Dispose();
            Assert.IsFalse(_eventHub.HasSubscriptions<int>());
        }

        public class Tester : IHandle<string>, IHandle<int>, IHaveFiber
        {
            public IFiber Fiber { get; } = new Fiber();
            
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

        public class AsyncTester : IHandleAsync<string>, IHandleAsync<int>, IHaveAsyncFiber
        {
            public IAsyncFiber Fiber { get; } = new AsyncFiber();

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
