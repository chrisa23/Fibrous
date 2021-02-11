using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Fibrous.Tests
{
    [TestFixture]
    public class FiberSubscriptionTests
    {
        [Test]
        public async Task FilteredSubscribe()
        {
            var result = new List<int>();
            using var fiber = new Fiber();
            using var channel = new Channel<int>();
            using var sub = channel.Subscribe(fiber, result.Add, x => x > 10);
            for (int i = 0; i < 20; i++)
            {
                channel.Publish(i);
            }

            await Task.Delay(TimeSpan.FromMilliseconds(500));
            Assert.AreEqual(9, result.Count);
        }
    }
}
