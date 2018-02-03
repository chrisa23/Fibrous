namespace Fibrous.Tests
{
    using Fibrous.Channels;
    using Fibrous.Fibers;
    using NUnit.Framework;

    [TestFixture]
    public class ReqReplyTests
    {
        [Test]
        public void TestName()
        {
            IRequestChannel<int, int> channel = new RequestChannel<int, int>();
            IFiber fiber1 = PoolFiber.StartNew();
        }
    }
}