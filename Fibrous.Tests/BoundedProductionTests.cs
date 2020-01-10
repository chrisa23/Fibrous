using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Fibrous.Tests
{
    [TestFixture]
    public  class BoundedProductionTests
    {
        [Test]
        public void SlowerConsumer()
        {
            using var fiber1 = new Fiber(size: 4);
            using var fiber2 = new Fiber();
            int count = 0;
            var reset = new AutoResetEvent(false);
            void Action(int o)
            {
                count++;
                Thread.Sleep(100);
                if (count == 10)
                    reset.Set();
            }

            
            var channel = new Channel<int>();
            channel.Subscribe(fiber1, Action);
            fiber2.Schedule(() => channel.Publish(0), TimeSpan.FromMilliseconds(10), TimeSpan.FromMilliseconds(20));
            Assert.IsTrue(reset.WaitOne(TimeSpan.FromSeconds(2)));
        }

        [Test]
        public void AsyncSlowerConsumer()
        {
            using var fiber1 = new AsyncFiber( size: 4);
            using var fiber2 = new Fiber();
            int count = 0;
            var reset = new AutoResetEvent(false);
            async Task Action(int o)
            {
                count++;
                await Task.Delay(100);
                if (count == 10)
                    reset.Set();
            }


            var channel = new Channel<int>();
            channel.Subscribe(fiber1, Action);
            fiber2.Schedule(() => channel.Publish(0), TimeSpan.FromMilliseconds(10), TimeSpan.FromMilliseconds(20));
            Assert.IsTrue(reset.WaitOne(TimeSpan.FromSeconds(2)));
        }
    }
}
