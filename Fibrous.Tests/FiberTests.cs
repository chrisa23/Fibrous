namespace Fibrous.Tests
{
    using NUnit.Framework;

    [TestFixture]
    public class FiberTests
    {
        [Test]
        public void ExecuteOnlyAfterStart()
        {
            FiberTester.ExecuteOnlyAfterStart(new ThreadFiber());
            FiberTester.ExecuteOnlyAfterStart(new PoolFiber());
            FiberTester.ExecuteOnlyAfterStart(new StubFiber());
        }

        [Test]
        public void InOrderExecution()
        {
            FiberTester.InOrderExecution(ThreadFiber.StartNew());
            FiberTester.InOrderExecution(PoolFiber.StartNew());
            FiberTester.InOrderExecution(StubFiber.StartNew());
        }

        [Test]
        public void TestPubSubSimple()
        {
            FiberTester.TestPubSubSimple(ThreadFiber.StartNew());
            FiberTester.TestPubSubSimple(PoolFiber.StartNew());
            FiberTester.TestPubSubSimple(StubFiber.StartNew());
        }

        [Test]
        public void TestPubSubWithFilter()
        {
            FiberTester.TestPubSubWithFilter(ThreadFiber.StartNew());
            FiberTester.TestPubSubWithFilter(PoolFiber.StartNew());
            FiberTester.TestPubSubWithFilter(StubFiber.StartNew());
        }

        [Test]
        public void TestReqReply()
        {
            FiberTester.TestReqReply1(ThreadFiber.StartNew());
            FiberTester.TestReqReply1(PoolFiber.StartNew());
            FiberTester.TestReqReply1(StubFiber.StartNew());
        }

        [Test]
        public void TestBatching()
        {
            FiberTester.TestBatching(ThreadFiber.StartNew());
            FiberTester.TestBatching(PoolFiber.StartNew());
            FiberTester.TestBatching(StubFiber.StartNew());
            FiberTester.TestBatchingWithKey(ThreadFiber.StartNew());
            FiberTester.TestBatchingWithKey(PoolFiber.StartNew());
            FiberTester.TestBatchingWithKey(StubFiber.StartNew());
        }

        [Test]
        public void TestTwoFibers()
        {
            FiberTester.TestPubSubWExtraFiber(ThreadFiber.StartNew(), ThreadFiber.StartNew());
            FiberTester.TestPubSubWExtraFiber(PoolFiber.StartNew(), ThreadFiber.StartNew());
            FiberTester.TestPubSubWExtraFiber(PoolFiber.StartNew(), PoolFiber.StartNew());
            FiberTester.TestPubSubWExtraFiber(PoolFiber.StartNew(), StubFiber.StartNew());
        }
    }
}