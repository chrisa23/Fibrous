using NUnit.Framework;

namespace Fibrous.Tests
{
    [TestFixture]
    public class CleanupTests
    {
        [Test]
        public void ChannelSubscription()
        {
            var fiber = new StubFiber();
            var channel = new Channel<int>();
            fiber.Subscribe(channel, i => { });
            Assert.AreEqual(true, channel.HasSubscriptions());
            fiber.Dispose();
            Assert.AreEqual(false, channel.HasSubscriptions());
            var sub = fiber.Subscribe(channel, i => { });
            Assert.AreEqual(true, channel.HasSubscriptions());
            sub.Dispose();
            Assert.AreEqual(false, channel.HasSubscriptions());
        }
    }
}