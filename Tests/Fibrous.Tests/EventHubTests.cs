using System;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Fibrous.Tests;

[TestFixture]
public class EventHubTests
{
    private readonly EventHub _eventHub = new();

    [Test]
    public async Task NonAsync()
    {
        using Tester test = new();
        using IDisposable sub = _eventHub.Subscribe(test.Fiber, test);

        _eventHub.Publish(3);
        _eventHub.Publish(2);
        _eventHub.Publish("test");

        await Task.Delay(100);

        Assert.AreEqual(6, test.Count);
    }

    [Test]
    public async Task Async()
    {
        using AsyncTester test = new();
        using IDisposable sub = _eventHub.Subscribe(test.Fiber, test);

        _eventHub.Publish(3);
        _eventHub.Publish(2);
        _eventHub.Publish("test");

        await Task.Delay(100);

        Assert.AreEqual(6, test.Count);
    }


    [Test]
    public Task AsyncUnsubscribeCheck()
    {
        using AsyncTester test = new();
        IDisposable sub = _eventHub.Subscribe(test.Fiber, test);

        _eventHub.Publish(3);

        sub.Dispose();
        Assert.IsFalse(_eventHub.HasSubscriptions<int>());
        return Task.CompletedTask;
    }

    [Test]
    public void UnsubscribeCheck()
    {
        using Tester test = new();
        IDisposable sub = _eventHub.Subscribe(test.Fiber, test);

        _eventHub.Publish(3);

        sub.Dispose();
        Assert.IsFalse(_eventHub.HasSubscriptions<int>());
    }

    public class Tester : IHandle<string>, IHandle<int>, IDisposable
    {
        public IFiber Fiber { get; } = new Fiber();

        public int Count { get; private set; }

        public void Dispose() => Fiber?.Dispose();

        public void Handle(int message) => Count += message;

        public void Handle(string message) => Count += 1;
    }

    public class AsyncTester : IHandleAsync<string>, IHandleAsync<int>, IDisposable
    {
        public IAsyncFiber Fiber { get; } = new AsyncFiber();

        public int Count { get; private set; }

        public void Dispose() => Fiber?.Dispose();

        public Task HandleAsync(int message)
        {
            Count += message;
            return Task.CompletedTask;
        }

        public Task HandleAsync(string message)
        {
            Count += 1;
            return Task.CompletedTask;
        }
    }
}
