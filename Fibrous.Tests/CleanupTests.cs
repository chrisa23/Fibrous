using System;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Fibrous.Tests
{
    [TestFixture]
    public class CleanupTests
    {
        [Test]
        public void ChannelSubscription()
        {
            RunTest(new StubFiber(), new Channel<int>(), (f,c) => f.Subscribe(c, i => { }));
            RunTest(new Fiber(), new Channel<int>(), (f, c) => f.Subscribe(c, i => { }));
            RunTest(new StubFiber(), new Channel<int>(), (f, c) =>f.SubscribeToBatch(c, i => { }, TimeSpan.FromSeconds(1)));
            RunTest(new Fiber(), new Channel<int>(), (f, c) => f.SubscribeToBatch(c, i => { }, TimeSpan.FromSeconds(1)));
            RunTest(new StubFiber(), new Channel<int>(), (f, c) => f.SubscribeToLast(c, i => { }, TimeSpan.FromSeconds(1)));
            RunTest(new Fiber(), new Channel<int>(), (f, c) => f.SubscribeToLast(c, i => { }, TimeSpan.FromSeconds(1)));
            RunTest(new StubFiber(), new Channel<int>(), (f, c) => f.SubscribeToKeyedBatch(c, x => x, i => { }, TimeSpan.FromSeconds(1)));
            RunTest(new Fiber(), new Channel<int>(), (f, c) => f.SubscribeToKeyedBatch(c, x => x, i => { }, TimeSpan.FromSeconds(1)));
        }


        public static void RunTest(IFiber fiber, Channel<int> channel, Func<IFiber, Channel<int>, IDisposable> f)
        {
            Assert.AreEqual(false, channel.HasSubscriptions);
            var sub = f(fiber, channel);
            Assert.AreEqual(true, channel.HasSubscriptions);
            sub.Dispose();

            Assert.AreEqual(false, channel.HasSubscriptions);
            fiber.Dispose();
            channel.Dispose();
        }

        [Test]
        public void AsyncChannelSubscription()
        {
            RunTest(new AsyncStubFiber(), new Channel<int>(), (f, c) => f.Subscribe(c, i => Task.CompletedTask));
            RunTest(new AsyncFiber(), new Channel<int>(), (f, c) => f.Subscribe(c, i => Task.CompletedTask));
            RunTest(new AsyncStubFiber(), new Channel<int>(), (f, c) => f.SubscribeToBatch(c, i => Task.CompletedTask, TimeSpan.FromSeconds(1)));
            RunTest(new AsyncFiber(), new Channel<int>(), (f, c) => f.SubscribeToBatch(c, i => Task.CompletedTask, TimeSpan.FromSeconds(1)));
            RunTest(new AsyncStubFiber(), new Channel<int>(), (f, c) => f.SubscribeToLast(c, i => Task.CompletedTask, TimeSpan.FromSeconds(1)));
            RunTest(new AsyncFiber(), new Channel<int>(), (f, c) => f.SubscribeToLast(c, i => Task.CompletedTask, TimeSpan.FromSeconds(1)));
            RunTest(new AsyncStubFiber(), new Channel<int>(), (f, c) => f.SubscribeToKeyedBatch(c, x => x, i => Task.CompletedTask, TimeSpan.FromSeconds(1)));
            RunTest(new AsyncFiber(), new Channel<int>(), (f, c) => f.SubscribeToKeyedBatch(c, x => x, i => Task.CompletedTask, TimeSpan.FromSeconds(1)));
        }


        public static void RunTest(IAsyncFiber fiber, Channel<int> channel, Func<IAsyncFiber, Channel<int>, IDisposable> f)
        {
            Assert.AreEqual(false, channel.HasSubscriptions);
            var sub = f(fiber, channel);
            Assert.AreEqual(true, channel.HasSubscriptions);
            sub.Dispose();

            Assert.AreEqual(false, channel.HasSubscriptions);
            fiber.Dispose();
            channel.Dispose();
        }

    }
}