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

        [Test]
        public void ThrottledTest()
        {
            IEventChannel eventChannel = new EventChannel();
            using var reset = new AutoResetEvent(false);
            int i = 0;
            void Receive()
            {
                i++;
                reset.Set();
            }

            using var fiber = new Fiber();

            eventChannel.SubscribeThrottled(fiber, Receive, TimeSpan.FromSeconds(.5));
            for (int j = 0; j < 10; j++)
            {
                eventChannel.Trigger();
            }
            

            Assert.IsTrue(reset.WaitOne(TimeSpan.FromSeconds(1)));
            Assert.AreEqual(1, i);
        }
    }
}
