using System;
using System.Threading;
using NUnit.Framework;

namespace Fibrous.Tests
{
    [TestFixture]
    public class EventChannelTests
    {
        [Test]
        public void BasicTest()
        {
            IEventChannel eventChannel = new EventChannel();
            using var reset = new AutoResetEvent(false);
            void Receive() => reset.Set();
            using var fiber = new Fiber();
            
            eventChannel.Subscribe(fiber, Receive);

            eventChannel.Trigger();

            Assert.IsTrue(reset.WaitOne(TimeSpan.FromSeconds(1)));
        }
    }
}
