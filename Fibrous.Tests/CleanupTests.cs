namespace Fibrous.Tests
{
    using Fibrous.Channels;
    using Fibrous.Fibers;
    using NUnit.Framework;

    [TestFixture]
    public class CleanupTests
    {
        [Test]
        public void ChannelSubscription()
        {
            var fiber = StubFiber.StartNew();
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