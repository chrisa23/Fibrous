using System.Threading;
using NUnit.Framework;

namespace Fibrous.Tests
{
    [TestFixture]
    public class EventBusTests
    {
        [Test]
        public void EventBusInt()
        {
            using Fiber fiber = new Fiber();
            using AutoResetEvent reset = new AutoResetEvent(false);
            EventBus<int>.Subscribe(fiber, _ => reset.Set());
            EventBus<int>.Publish(0);
            Assert.IsTrue(reset.WaitOne(100));
        }

        [Test]
        public void EventBusMixed()
        {
            using Fiber fiber = new Fiber();
            using Fiber fiber2 = new Fiber();
            using AutoResetEvent reset = new AutoResetEvent(false);
            using AutoResetEvent reset2 = new AutoResetEvent(false);
            EventBus<int>.Subscribe(fiber, _ => reset.Set());
            EventBus<string>.Subscribe(fiber2, _ => reset2.Set());
            EventBus<int>.Publish(0);
            EventBus<string>.Publish("!");
            Assert.IsTrue(WaitHandle.WaitAll(new[] {reset, reset2}, 100));
        }
    }
}
